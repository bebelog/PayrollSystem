using Payroll.CoreBusiness.Enums;

namespace Payroll.CoreBusiness.Entities;

public class Timesheet
{
    public int TimesheetId { get; set; }
    public int EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedByUserId { get; set; }

    public Employee? Employee { get; set; }
    public List<TimesheetEntry> Entries { get; set; } = new();

    public decimal CalculateActualWorkingDays()
    {
        return Entries.Count(e => e.DayStatus == TimesheetDayStatus.Working 
                               || e.DayStatus == TimesheetDayStatus.PaidLeave 
                               || e.DayStatus == TimesheetDayStatus.Holiday);
    }
}
