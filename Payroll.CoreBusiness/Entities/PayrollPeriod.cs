using Payroll.CoreBusiness.Enums;

namespace Payroll.CoreBusiness.Entities;

public class PayrollPeriod
{
    public int PeriodId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal StandardWorkingDays { get; set; } = 22m;
    public PayrollPeriodStatus Status { get; set; } = PayrollPeriodStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ClosedAt { get; set; }

    public List<Payslip>? Payslips { get; set; }
}
