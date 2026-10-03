using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Employees;

public interface IUpdateEmployeeUseCase
{
    Task ExecuteAsync(Employee employee);
}

public class UpdateEmployeeUseCase : IUpdateEmployeeUseCase
{
    private readonly IEmployeeRepository _employeeRepository;

    public UpdateEmployeeUseCase(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task ExecuteAsync(Employee employee)
    {
        if (string.IsNullOrWhiteSpace(employee.FullName))
            throw new BusinessRuleException("Họ và tên nhân viên không được để trống.");

        await _employeeRepository.UpdateEmployeeAsync(employee);
    }
}
