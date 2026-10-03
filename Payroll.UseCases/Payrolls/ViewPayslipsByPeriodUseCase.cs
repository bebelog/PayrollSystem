using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Payrolls;

public interface IViewPayslipsByPeriodUseCase
{
    Task<IEnumerable<Payslip>> ExecuteAsync(int periodId);
}

public class ViewPayslipsByPeriodUseCase : IViewPayslipsByPeriodUseCase
{
    private readonly IPayrollRepository _payrollRepository;

    public ViewPayslipsByPeriodUseCase(IPayrollRepository payrollRepository)
    {
        _payrollRepository = payrollRepository;
    }

    public async Task<IEnumerable<Payslip>> ExecuteAsync(int periodId)
    {
        return await _payrollRepository.GetPayslipsByPeriodIdAsync(periodId);
    }
}
