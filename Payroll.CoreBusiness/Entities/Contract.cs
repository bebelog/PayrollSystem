namespace Payroll.CoreBusiness.Entities;

public class Contract
{
    public int ContractId { get; set; }
    public int EmployeeId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal Allowance { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;

    public Employee? Employee { get; set; }
}
