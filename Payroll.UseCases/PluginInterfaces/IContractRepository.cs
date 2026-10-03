using Payroll.CoreBusiness.Entities;

namespace Payroll.UseCases.PluginInterfaces;

public interface IContractRepository
{
    Task<IEnumerable<Contract>> GetContractsByEmployeeIdAsync(int employeeId);
    Task<Contract?> GetActiveContractByEmployeeIdAsync(int employeeId);
    Task<int> AddContractAsync(Contract contract);
    Task UpdateContractAsync(Contract contract);
}
