using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Employees;

public interface ICreateEmployeeUseCase
{
    Task<int> ExecuteAsync(Employee employee, Contract initialContract);
}

public class CreateEmployeeUseCase : ICreateEmployeeUseCase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IContractRepository _contractRepository;

    public CreateEmployeeUseCase(IEmployeeRepository employeeRepository, IContractRepository contractRepository)
    {
        _employeeRepository = employeeRepository;
        _contractRepository = contractRepository;
    }

    public async Task<int> ExecuteAsync(Employee employee, Contract initialContract)
    {
        if (string.IsNullOrWhiteSpace(employee.FullName))
            throw new BusinessRuleException("Họ và tên nhân viên không được để trống.");

        if (string.IsNullOrWhiteSpace(employee.IdentityNumber))
            throw new BusinessRuleException("Số CCCD/CMND không được để trống.");

        if (initialContract.BaseSalary <= 0)
            throw new BusinessRuleException("Lương cơ bản trên hợp đồng phải lớn hơn 0.");

        int employeeId = await _employeeRepository.AddEmployeeAsync(employee);
        initialContract.EmployeeId = employeeId;
        initialContract.IsActive = true;
        await _contractRepository.AddContractAsync(initialContract);

        return employeeId;
    }
}
