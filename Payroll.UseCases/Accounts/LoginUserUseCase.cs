using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Accounts;

public interface ILoginUserUseCase
{
    Task<AppUser> ExecuteAsync(string username, string password);
}

public class LoginUserUseCase : ILoginUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginUserUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AppUser> ExecuteAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new BusinessRuleException("Vui lòng nhập tên đăng nhập và mật khẩu.");

        var user = await _userRepository.GetByUsernameAsync(username.Trim());
        if (user == null || !user.IsActive)
            throw new BusinessRuleException("Tài khoản không tồn tại hoặc đã bị khóa.");

        bool isValidPassword = _passwordHasher.VerifyPassword(password, user.PasswordHash);
        if (!isValidPassword)
            throw new BusinessRuleException("Mật khẩu không chính xác.");

        return user;
    }
}
