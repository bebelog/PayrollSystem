using Payroll.CoreBusiness.Entities;

namespace Payroll.UseCases.PluginInterfaces;

public interface IDepartmentRepository
{
    Task<IEnumerable<Department>> GetDepartmentsAsync();
    Task<Department?> GetDepartmentByIdAsync(int departmentId);
}
