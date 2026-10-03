using System.Data;
using Dapper;
using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins.DataStore.Sql;

public class DependentRepository : IDependentRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DependentRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Dependent>> GetDependentsByEmployeeIdAsync(int employeeId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT DependentId, EmployeeId, FullName, Relationship, DateOfBirth, IdentityNumber, IsActive
            FROM dbo.Dependent
            WHERE EmployeeId = @EmployeeId
            ORDER BY FullName";

        var result = await conn.QueryAsync<Dependent>(sql, new { EmployeeId = employeeId });
        return result.ToList();
    }

    public async Task<int> AddDependentAsync(Dependent dependent)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.Dependent (EmployeeId, FullName, Relationship, DateOfBirth, IdentityNumber, IsActive)
            VALUES (@EmployeeId, @FullName, @Relationship, @DateOfBirth, @IdentityNumber, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        int id = await conn.ExecuteScalarAsync<int>(sql, dependent);
        dependent.DependentId = id;
        return id;
    }

    public async Task UpdateDependentAsync(Dependent dependent)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.Dependent
            SET FullName = @FullName,
                Relationship = @Relationship,
                DateOfBirth = @DateOfBirth,
                IdentityNumber = @IdentityNumber,
                IsActive = @IsActive
            WHERE DependentId = @DependentId";

        await conn.ExecuteAsync(sql, dependent);
    }

    public async Task DeleteDependentAsync(int dependentId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "UPDATE dbo.Dependent SET IsActive = 0 WHERE DependentId = @DependentId";
        await conn.ExecuteAsync(sql, new { DependentId = dependentId });
    }
}
