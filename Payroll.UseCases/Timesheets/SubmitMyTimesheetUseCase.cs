using Payroll.CoreBusiness.DomainServices;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Timesheets;

public interface ISubmitMyTimesheetUseCase
{
    Task ExecuteAsync(int? loggedEmployeeId, UserRole loggedUserRole, int timesheetId);
}

public class SubmitMyTimesheetUseCase : ISubmitMyTimesheetUseCase
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IPayrollRepository _payrollRepository;

    public SubmitMyTimesheetUseCase(ITimesheetRepository timesheetRepository, IPayrollRepository payrollRepository)
    {
        _timesheetRepository = timesheetRepository;
        _payrollRepository = payrollRepository;
    }

    public async Task ExecuteAsync(int? loggedEmployeeId, UserRole loggedUserRole, int timesheetId)
    {
        var timesheet = await _timesheetRepository.GetTimesheetByIdAsync(timesheetId);
        if (timesheet == null)
            throw new BusinessRuleException("Không tìm thấy bảng công.");

        if (loggedUserRole == UserRole.Employee && loggedEmployeeId != timesheet.EmployeeId)
            throw new BusinessRuleException("Bạn không có quyền thao tác trên bảng công của người khác.");

        var period = await _payrollRepository.GetPeriodAsync(timesheet.Month, timesheet.Year);
        if (period != null)
        {
            PeriodStateMachine.EnsurePeriodNotClosed(period);
        }

        TimesheetStateMachine.ValidateTransition(timesheet.Status, TimesheetStatus.Submitted);
        await _timesheetRepository.UpdateTimesheetStatusAsync(timesheetId, TimesheetStatus.Submitted);
    }
}
