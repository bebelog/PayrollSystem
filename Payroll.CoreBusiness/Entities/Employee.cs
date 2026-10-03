namespace Payroll.CoreBusiness.Entities;

public class Employee
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Department? Department { get; set; }
    public List<Contract>? Contracts { get; set; }
    public List<Dependent>? Dependents { get; set; }

    public Contract? ActiveContract => Contracts?.FirstOrDefault(c => c.IsActive);
}
