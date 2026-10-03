namespace Payroll.CoreBusiness.Entities;

public class Dependent
{
    public int DependentId { get; set; }
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? IdentityNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public Employee? Employee { get; set; }
}
