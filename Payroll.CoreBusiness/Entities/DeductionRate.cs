namespace Payroll.CoreBusiness.Entities;

public class DeductionRate
{
    public int RateId { get; set; }
    public string Code { get; set; } = string.Empty;    // BHXH, BHYT, BHTN
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }                   // 0.08, 0.015, 0.01
    public DateTime EffectiveFrom { get; set; }
    public string SourceNote { get; set; } = "GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC";
}
