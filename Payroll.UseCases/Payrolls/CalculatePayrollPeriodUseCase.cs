using Payroll.CoreBusiness.DomainServices;
using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Payrolls;

public interface ICalculatePayrollPeriodUseCase
{
    Task<int> ExecuteAsync(int periodId);
}

public class CalculatePayrollPeriodUseCase : ICalculatePayrollPeriodUseCase
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IDependentRepository _dependentRepository;
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IConfigurationRepository _configurationRepository;

    public CalculatePayrollPeriodUseCase(
        IPayrollRepository payrollRepository,
        IEmployeeRepository employeeRepository,
        IContractRepository contractRepository,
        IDependentRepository dependentRepository,
        ITimesheetRepository timesheetRepository,
        IConfigurationRepository configurationRepository)
    {
        _payrollRepository = payrollRepository;
        _employeeRepository = employeeRepository;
        _contractRepository = contractRepository;
        _dependentRepository = dependentRepository;
        _timesheetRepository = timesheetRepository;
        _configurationRepository = configurationRepository;
    }

    public async Task<int> ExecuteAsync(int periodId)
    {
        var period = await _payrollRepository.GetPeriodByIdAsync(periodId);
        if (period == null)
            throw new BusinessRuleException("Không tìm thấy kỳ lương chỉ định.");

        // LUẬT 2: Kỳ đã chốt không cho tính lại
        PeriodStateMachine.EnsurePeriodNotClosed(period);

        // Lấy cấu hình (không hardcode)
        var setting = await _configurationRepository.GetCurrentSettingAsync();
        var deductionRates = await _configurationRepository.GetDeductionRatesAsync();
        var taxBrackets = await _configurationRepository.GetTaxBracketsAsync();

        // Lấy danh sách nhân viên đang làm việc
        var employees = await _employeeRepository.GetEmployeesAsync(isActive: true);
        var calculatedPayslips = new List<Payslip>();

        foreach (var emp in employees)
        {
            var contract = await _contractRepository.GetActiveContractByEmployeeIdAsync(emp.EmployeeId);
            if (contract == null)
            {
                // Bỏ qua nhân viên chưa có hợp đồng có hiệu lực
                continue;
            }

            var timesheet = await _timesheetRepository.GetTimesheetAsync(emp.EmployeeId, period.Month, period.Year);
            if (timesheet == null)
            {
                // Không có bảng công thì bỏ qua hoặc báo lỗi
                continue;
            }

            // LUẬT 2: Chỉ tính lương cho nhân viên có Timesheet ĐÃ DUYỆT
            TimesheetStateMachine.EnsureCanCalculateSalary(timesheet);

            decimal actualDays = timesheet.CalculateActualWorkingDays();
            var dependents = await _dependentRepository.GetDependentsByEmployeeIdAsync(emp.EmployeeId);
            int activeDependentsCount = dependents.Count(d => d.IsActive);

            // LUẬT 1: Tính toán bảng lương theo công thức thuần
            var calcResult = PayrollCalculator.Calculate(
                contract,
                actualDays,
                period.StandardWorkingDays,
                activeDependentsCount,
                setting,
                deductionRates,
                taxBrackets);

            var payslip = new Payslip
            {
                PeriodId = periodId,
                EmployeeId = emp.EmployeeId,
                BaseSalarySnapshot = calcResult.BaseSalarySnapshot,
                StandardDaysSnapshot = calcResult.StandardDaysSnapshot,
                ActualDaysSnapshot = calcResult.ActualDaysSnapshot,
                WorkingSalary = calcResult.WorkingSalary,
                AllowanceSnapshot = calcResult.AllowanceSnapshot,
                GrossSalary = calcResult.GrossSalary,
                TotalInsuranceDeduction = calcResult.TotalInsuranceDeduction,
                PersonalDeductionSnapshot = calcResult.PersonalDeductionSnapshot,
                DependentDeductionSnapshot = calcResult.DependentDeductionSnapshot,
                DependentCountSnapshot = calcResult.DependentCountSnapshot,
                TaxableIncome = calcResult.TaxableIncome,
                PersonalIncomeTax = calcResult.PersonalIncomeTax,
                NetSalary = calcResult.NetSalary,
                Lines = calcResult.Lines
            };

            calculatedPayslips.Add(payslip);
        }

        if (calculatedPayslips.Count == 0)
        {
            throw new BusinessRuleException("Không có nhân viên nào thỏa mãn điều kiện (có hợp đồng và bảng công Đã duyệt) để tính lương.");
        }

        // Lưu toàn bộ Payslip + PayslipLine trong 1 DB Transaction (ACID)
        await _payrollRepository.SaveCalculatedPayslipsAsync(periodId, calculatedPayslips);

        // Chuyển trạng thái kỳ thành Đã tính
        PeriodStateMachine.ValidateTransition(period.Status, PayrollPeriodStatus.Calculated);
        await _payrollRepository.UpdatePeriodStatusAsync(periodId, PayrollPeriodStatus.Calculated);

        return calculatedPayslips.Count;
    }
}
