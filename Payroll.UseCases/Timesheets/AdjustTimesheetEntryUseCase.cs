using Payroll.CoreBusiness.DomainServices;
using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Timesheets;

public interface IAdjustTimesheetEntryUseCase
{
    Task ExecuteAsync(int timesheetId, DateTime workDate, TimesheetDayStatus dayStatus, string? note);
}

public class AdjustTimesheetEntryUseCase : IAdjustTimesheetEntryUseCase
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IPayrollRepository _payrollRepository;

    public AdjustTimesheetEntryUseCase(ITimesheetRepository timesheetRepository, IPayrollRepository payrollRepository)
    {
        _timesheetRepository = timesheetRepository;
        _payrollRepository = payrollRepository;
    }

    public async Task ExecuteAsync(int timesheetId, DateTime workDate, TimesheetDayStatus dayStatus, string? note)
    {
        var timesheet = await _timesheetRepository.GetTimesheetByIdAsync(timesheetId);
        if (timesheet == null)
            throw new BusinessRuleException("Không tìm thấy bảng công.");

        var period = await _payrollRepository.GetPeriodAsync(timesheet.Month, timesheet.Year);
        if (period != null)
        {
            PeriodStateMachine.EnsurePeriodNotClosed(period);
        }

        var entry = new TimesheetEntry
        {
            TimesheetId = timesheetId,
            WorkDate = workDate.Date,
            DayStatus = dayStatus,
            Note = note
        };

        await _timesheetRepository.UpsertTimesheetEntryAsync(entry);
    }
}