using Payroll.CoreBusiness.Enums;

namespace Payroll.CoreBusiness.Entities;

public class TimesheetEntry
{
    public int EntryId { get; set; }
    public int TimesheetId { get; set; }
    public DateTime WorkDate { get; set; }
    public TimesheetDayStatus DayStatus { get; set; } = TimesheetDayStatus.Working;
    public string? Note { get; set; }

    public Timesheet? Timesheet { get; set; }
}
