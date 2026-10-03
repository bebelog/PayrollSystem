using Payroll.CoreBusiness.DomainServices;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Timesheets;

public interface IRevertTimesheetUseCase
{
    Task ExecuteAsync(int timesheetId);
}

public class RevertTimesheetUseCase : IRevertTimesheetUseCase
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IPayrollRepository _payrollRepository;

    public RevertTimesheetUseCase(ITimesheetRepository timesheetRepository, IPayrollRepository payrollRepository)
    {
        _timesheetRepository = timesheetRepository;
        _payrollRepository = payrollRepository;
    }

    public async Task ExecuteAsync(int timesheetId)
    {
        var timesheet = await _timesheetRepository.GetTimesheetByIdAsync(timesheetId);
        if (timesheet == null)
            throw new BusinessRuleException("Không tìm thấy bảng công.");

        var period = await _payrollRepository.GetPeriodAsync(timesheet.Month, timesheet.Year);
        if (period != null)
        {
            PeriodStateMachine.EnsurePeriodNotClosed(period);
        }

        TimesheetStateMachine.ValidateTransition(timesheet.Status, TimesheetStatus.Draft);
        await _timesheetRepository.UpdateTimesheetStatusAsync(timesheetId, TimesheetStatus.Draft, null);
    }
}