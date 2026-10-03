using Payroll.CoreBusiness.Entities;

namespace Payroll.UseCases.PluginInterfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetEmployeesAsync(string? searchTerm = null, int? departmentId = null, bool? isActive = null);
    Task<Employee?> GetEmployeeByIdAsync(int employeeId);
    Task<int> AddEmployeeAsync(Employee employee);
    Task UpdateEmployeeAsync(Employee employee);
    Task DeleteEmployeeAsync(int employeeId);
}
