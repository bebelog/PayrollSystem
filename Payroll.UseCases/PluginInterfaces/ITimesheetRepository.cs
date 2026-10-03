using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;

namespace Payroll.UseCases.PluginInterfaces;

public interface ITimesheetRepository
{
    Task<Timesheet?> GetTimesheetAsync(int employeeId, int month, int year);
    Task<Timesheet?> GetTimesheetByIdAsync(int timesheetId);
    Task<IEnumerable<Timesheet>> GetTimesheetsByPeriodAsync(int month, int year, int? departmentId = null, TimesheetStatus? status = null);
    Task<int> CreateOrUpdateTimesheetHeaderAsync(Timesheet timesheet);
    Task SaveTimesheetEntriesAsync(int timesheetId, IEnumerable<TimesheetEntry> entries);
    Task UpdateTimesheetStatusAsync(int timesheetId, TimesheetStatus status, int? approvedByUserId = null);
    Task<TimesheetEntry?> GetEntryByDateAsync(int timesheetId, DateTime workDate);
    Task UpsertTimesheetEntryAsync(TimesheetEntry entry);
}
