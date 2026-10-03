namespace Payroll.CoreBusiness.Entities;

public class Payslip
{
    public int PayslipId { get; set; }
    public int PeriodId { get; set; }
    public int EmployeeId { get; set; }

    // Snapshot dữ liệu lương tại thời điểm tính
    public decimal BaseSalarySnapshot { get; set; }
    public decimal StandardDaysSnapshot { get; set; }
    public decimal ActualDaysSnapshot { get; set; }
    public decimal WorkingSalary { get; set; }
    public decimal AllowanceSnapshot { get; set; }
    public decimal GrossSalary { get; set; }
    
    // Bảo hiểm
    public decimal TotalInsuranceDeduction { get; set; }

    // Giảm trừ & Thu nhập tính thuế
    public decimal PersonalDeductionSnapshot { get; set; }
    public decimal DependentDeductionSnapshot { get; set; }
    public int DependentCountSnapshot { get; set; }
    public decimal TaxableIncome { get; set; } // Thu nhập tính thuế sau giảm trừ (không âm)

    // Thuế & Lương thực lĩnh
    public decimal PersonalIncomeTax { get; set; }
    public decimal NetSalary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Employee? Employee { get; set; }
    public PayrollPeriod? Period { get; set; }
    public List<PayslipLine> Lines { get; set; } = new();
}
