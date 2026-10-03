using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Employees;

public interface IGetEmployeeByIdUseCase
{
    Task<Employee?> ExecuteAsync(int employeeId);
}

public class GetEmployeeByIdUseCase : IGetEmployeeByIdUseCase
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetEmployeeByIdUseCase(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<Employee?> ExecuteAsync(int employeeId)
    {
        return await _employeeRepository.GetEmployeeByIdAsync(employeeId);
    }
}
