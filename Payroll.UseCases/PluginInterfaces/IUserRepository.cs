using Payroll.CoreBusiness.Entities;

namespace Payroll.UseCases.PluginInterfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByUsernameAsync(string username);
    Task<AppUser?> GetByIdAsync(int userId);
    Task<AppUser?> GetByEmployeeIdAsync(int employeeId);
    Task<int> CreateUserAsync(AppUser user);
    Task UpdateUserAsync(AppUser user);
}
