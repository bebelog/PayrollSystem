using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Employees;

public interface IViewEmployeesUseCase
{
    Task<IEnumerable<Employee>> ExecuteAsync(string? searchTerm = null, int? departmentId = null, bool? isActive = null);
}

public class ViewEmployeesUseCase : IViewEmployeesUseCase
{
    private readonly IEmployeeRepository _employeeRepository;

    public ViewEmployeesUseCase(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<IEnumerable<Employee>> ExecuteAsync(string? searchTerm = null, int? departmentId = null, bool? isActive = null)
    {
        return await _employeeRepository.GetEmployeesAsync(searchTerm, departmentId, isActive);
    }
}
