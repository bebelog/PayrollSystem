namespace Payroll.CoreBusiness.Entities;

public class TaxBracket
{
    public int BracketId { get; set; }
    public int BracketOrder { get; set; }
    public decimal ThresholdMin { get; set; }
    public decimal? ThresholdMax { get; set; }
    public decimal TaxRate { get; set; }                // 0.05, 0.10, 0.20, 0.30, 0.35
    public string SourceNote { get; set; } = "GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC";
}
