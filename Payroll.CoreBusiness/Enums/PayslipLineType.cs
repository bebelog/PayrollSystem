namespace Payroll.CoreBusiness.Enums;

public enum PayslipLineType
{
    Earning = 1,         // Thu nhập (Lương, phụ cấp)
    Insurance = 2,       // Trừ bảo hiểm (BHXH, BHYT, BHTN)
    TaxDeduction = 3,    // Giảm trừ thuế (Bản thân, người phụ thuộc)
    Tax = 4,             // Thuế TNCN
    OtherDeduction = 5   // Các khoản khấu trừ khác
}
