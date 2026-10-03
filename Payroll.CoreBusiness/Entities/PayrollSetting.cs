namespace Payroll.CoreBusiness.Entities;

public class PayrollSetting
{
    public int SettingId { get; set; }
    public decimal InsuranceCeiling { get; set; }       // e.g. 50,600,000 VND
    public decimal PersonalDeduction { get; set; }      // e.g. 15,500,000 VND
    public decimal DependentDeduction { get; set; }     // e.g. 6,200,000 VND
    public DateTime EffectiveFrom { get; set; }
    public string SourceNote { get; set; } = "GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC";
}
