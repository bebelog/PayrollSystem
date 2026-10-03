using Payroll.CoreBusiness.DomainServices;
using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Timesheets;

public interface IApproveTimesheetUseCase
{
    Task ExecuteAsync(int timesheetId, int accountantUserId);
}

public class ApproveTimesheetUseCase : IApproveTimesheetUseCase
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IPayrollRepository _payrollRepository;

    public ApproveTimesheetUseCase(ITimesheetRepository timesheetRepository, IPayrollRepository payrollRepository)
    {
        _timesheetRepository = timesheetRepository;
        _payrollRepository = payrollRepository;
    }

    public async Task ExecuteAsync(int timesheetId, int accountantUserId)
    {
        var timesheet = await _timesheetRepository.GetTimesheetByIdAsync(timesheetId);
        if (timesheet == null)
            throw new BusinessRuleException("Không tìm thấy bảng công.");

        var period = await _payrollRepository.GetPeriodAsync(timesheet.Month, timesheet.Year);
        if (period != null)
        {
            PeriodStateMachine.EnsurePeriodNotClosed(period);
        }

        TimesheetStateMachine.ValidateTransition(timesheet.Status, TimesheetStatus.Approved);
        await _timesheetRepository.UpdateTimesheetStatusAsync(timesheetId, TimesheetStatus.Approved, accountantUserId);
    }
}
