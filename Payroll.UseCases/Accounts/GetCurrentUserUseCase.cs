using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Accounts;

public interface IGetCurrentUserUseCase
{
    Task<AppUser?> ExecuteAsync(int userId);
}

public class GetCurrentUserUseCase : IGetCurrentUserUseCase
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AppUser?> ExecuteAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }
}
