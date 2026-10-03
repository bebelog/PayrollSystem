using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Employees;

public interface IViewDepartmentsUseCase
{
    Task<IEnumerable<Department>> ExecuteAsync();
}

public class ViewDepartmentsUseCase : IViewDepartmentsUseCase
{
    private readonly IDepartmentRepository _departmentRepository;

    public ViewDepartmentsUseCase(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<IEnumerable<Department>> ExecuteAsync()
    {
        return await _departmentRepository.GetDepartmentsAsync();
    }
}
