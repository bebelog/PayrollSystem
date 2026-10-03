using Payroll.CoreBusiness.DomainServices;
using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Timesheets;

public interface IRecordAttendanceTodayUseCase
{
    Task<TimesheetEntry> CheckInTodayAsync(int employeeId);
    Task<TimesheetEntry> CheckOutTodayAsync(int employeeId);
    Task<TimesheetEntry?> GetTodayAttendanceAsync(int employeeId);
    Task ExecuteAsync(int employeeId);
}

public class RecordAttendanceTodayUseCase : IRecordAttendanceTodayUseCase
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IPayrollRepository _payrollRepository;

    public RecordAttendanceTodayUseCase(ITimesheetRepository timesheetRepository, IPayrollRepository payrollRepository)
    {
        _timesheetRepository = timesheetRepository;
        _payrollRepository = payrollRepository;
    }

    public async Task<TimesheetEntry?> GetTodayAttendanceAsync(int employeeId)
    {
        var today = DateTime.Today;
        var timesheet = await _timesheetRepository.GetTimesheetAsync(employeeId, today.Month, today.Year);
        if (timesheet == null) return null;
        return await _timesheetRepository.GetEntryByDateAsync(timesheet.TimesheetId, today);
    }

    public async Task<TimesheetEntry> CheckInTodayAsync(int employeeId)
    {
        var now = DateTime.Now;
        var today = DateTime.Today;

        var period = await _payrollRepository.GetPeriodAsync(today.Month, today.Year);
        if (period != null)
        {
            PeriodStateMachine.EnsurePeriodNotClosed(period);
        }

        var timesheet = await _timesheetRepository.GetTimesheetAsync(employeeId, today.Month, today.Year);
        int timesheetId;

        if (timesheet == null)
        {
            var newTimesheet = new Timesheet
            {
                EmployeeId = employeeId,
                Month = today.Month,
                Year = today.Year,
                Status = TimesheetStatus.Draft
            };
            timesheetId = await _timesheetRepository.CreateOrUpdateTimesheetHeaderAsync(newTimesheet);
        }
        else
        {
            if (timesheet.Status == TimesheetStatus.Approved)
                throw new BusinessRuleException("Bảng công tháng này đã được duyệt, không thể chấm công thêm.");

            timesheetId = timesheet.TimesheetId;
        }

        var existingEntry = await _timesheetRepository.GetEntryByDateAsync(timesheetId, today);
        if (existingEntry != null && existingEntry.CheckInTime.HasValue)
        {
            throw new BusinessRuleException($"Hôm nay bạn đã chấm công vào lúc {existingEntry.CheckInTime.Value:HH:mm:ss} rồi.");
        }

        var entry = existingEntry ?? new TimesheetEntry
        {
            TimesheetId = timesheetId,
            WorkDate = today
        };

        entry.DayStatus = TimesheetDayStatus.Working;
        entry.CheckInTime = now;
        entry.WorkingHours = 8.0m;
        entry.Note = $"Chấm công vào lúc {now:HH:mm:ss}";

        await _timesheetRepository.UpsertTimesheetEntryAsync(entry);
        return entry;
    }

    public async Task<TimesheetEntry> CheckOutTodayAsync(int employeeId)
    {
        var now = DateTime.Now;
        var today = DateTime.Today;

        var period = await _payrollRepository.GetPeriodAsync(today.Month, today.Year);
        if (period != null)
        {
            PeriodStateMachine.EnsurePeriodNotClosed(period);
        }

        var timesheet = await _timesheetRepository.GetTimesheetAsync(employeeId, today.Month, today.Year);
        if (timesheet == null)
            throw new BusinessRuleException("Bạn chưa chấm công vào hôm nay, không thể chấm công ra.");

        if (timesheet.Status == TimesheetStatus.Approved)
            throw new BusinessRuleException("Bảng công tháng này đã được duyệt, không thể sửa đổi.");

        var existingEntry = await _timesheetRepository.GetEntryByDateAsync(timesheet.TimesheetId, today);
        if (existingEntry == null || !existingEntry.CheckInTime.HasValue)
            throw new BusinessRuleException("Bạn chưa chấm công vào hôm nay, không thể chấm công ra.");

        if (existingEntry.CheckOutTime.HasValue)
            throw new BusinessRuleException($"Hôm nay bạn đã chấm công ra lúc {existingEntry.CheckOutTime.Value:HH:mm:ss} rồi.");

        existingEntry.CheckOutTime = now;
        double diffHours = (now - existingEntry.CheckInTime.Value).TotalHours;
        decimal hours = (diffHours >= 0.1) ? Math.Min(8.0m, Math.Round((decimal)diffHours, 1)) : Math.Max(0.05m, Math.Round((decimal)diffHours, 2));
        existingEntry.WorkingHours = hours;
        existingEntry.Note = $"Vào: {existingEntry.CheckInTime.Value:HH:mm:ss} | Ra: {now:HH:mm:ss} ({hours}h)";

        await _timesheetRepository.UpsertTimesheetEntryAsync(existingEntry);
        return existingEntry;
    }

    public async Task ExecuteAsync(int employeeId)
    {
        await CheckInTodayAsync(employeeId);
    }
}