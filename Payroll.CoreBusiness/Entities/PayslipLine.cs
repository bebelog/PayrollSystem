using Payroll.CoreBusiness.Enums;

namespace Payroll.CoreBusiness.Entities;

public class PayslipLine
{
    public int LineId { get; set; }
    public int PayslipId { get; set; }
    public PayslipLineType LineType { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal? RateOrThresholdSnapshot { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }

    public Payslip? Payslip { get; set; }
}
