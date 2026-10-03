using System.Data;
using Dapper;
using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins.DataStore.Sql;

public class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public UserRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT u.UserId, u.Username, u.PasswordHash, u.Role, u.EmployeeId, u.IsActive, u.CreatedAt,
                   e.EmployeeId, e.FullName, e.Email, e.DepartmentId
            FROM dbo.AppUser u
            LEFT JOIN dbo.Employee e ON u.EmployeeId = e.EmployeeId
            WHERE u.Username = @Username";

        var result = await conn.QueryAsync<AppUser, Employee, AppUser>(
            sql,
            (user, emp) =>
            {
                user.Employee = emp;
                return user;
            },
            new { Username = username },
            splitOn: "EmployeeId");

        return result.FirstOrDefault();
    }

    public async Task<AppUser?> GetByIdAsync(int userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT u.UserId, u.Username, u.PasswordHash, u.Role, u.EmployeeId, u.IsActive, u.CreatedAt,
                   e.EmployeeId, e.FullName, e.Email, e.DepartmentId
            FROM dbo.AppUser u
            LEFT JOIN dbo.Employee e ON u.EmployeeId = e.EmployeeId
            WHERE u.UserId = @UserId";

        var result = await conn.QueryAsync<AppUser, Employee, AppUser>(
            sql,
            (user, emp) =>
            {
                user.Employee = emp;
                return user;
            },
            new { UserId = userId },
            splitOn: "EmployeeId");

        return result.FirstOrDefault();
    }

    public async Task<AppUser?> GetByEmployeeIdAsync(int employeeId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT UserId, Username, PasswordHash, Role, EmployeeId, IsActive, CreatedAt
            FROM dbo.AppUser
            WHERE EmployeeId = @EmployeeId";

        return await conn.QueryFirstOrDefaultAsync<AppUser>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> CreateUserAsync(AppUser user)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.AppUser (Username, PasswordHash, Role, EmployeeId, IsActive, CreatedAt)
            VALUES (@Username, @PasswordHash, @Role, @EmployeeId, @IsActive, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        int id = await conn.ExecuteScalarAsync<int>(sql, user);
        user.UserId = id;
        return id;
    }

    public async Task UpdateUserAsync(AppUser user)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.AppUser
            SET PasswordHash = @PasswordHash,
                Role = @Role,
                IsActive = @IsActive
            WHERE UserId = @UserId";

        await conn.ExecuteAsync(sql, user);
    }
}
