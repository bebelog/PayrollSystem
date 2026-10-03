using Payroll.CoreBusiness.Entities;

namespace Payroll.UseCases.PluginInterfaces;

public interface IDependentRepository
{
    Task<IEnumerable<Dependent>> GetDependentsByEmployeeIdAsync(int employeeId);
    Task<int> AddDependentAsync(Dependent dependent);
    Task UpdateDependentAsync(Dependent dependent);
    Task DeleteDependentAsync(int dependentId);
}
