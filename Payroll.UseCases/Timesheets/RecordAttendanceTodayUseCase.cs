using Payroll.CoreBusiness.DomainServices;
using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Timesheets;

public interface IRecordAttendanceTodayUseCase
{
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

    public async Task ExecuteAsync(int employeeId)
    {
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
        if (existingEntry != null && existingEntry.DayStatus == TimesheetDayStatus.Working)
        {
            throw new BusinessRuleException("Hôm nay bạn đã chấm công đi làm rồi.");
        }

        var entry = new TimesheetEntry
        {
            TimesheetId = timesheetId,
            WorkDate = today,
            DayStatus = TimesheetDayStatus.Working,
            Note = $"Chấm công trực tuyến lúc {DateTime.Now:HH:mm:ss}"
        };

        await _timesheetRepository.UpsertTimesheetEntryAsync(entry);
    }
}
