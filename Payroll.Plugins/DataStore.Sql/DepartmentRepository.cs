using System.Data;
using Dapper;
using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins.DataStore.Sql;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DepartmentRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Department>> GetDepartmentsAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT DepartmentId, DepartmentName, Description, CreatedAt FROM dbo.Department ORDER BY DepartmentName";
        var result = await conn.QueryAsync<Department>(sql);
        return result.ToList();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int departmentId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT DepartmentId, DepartmentName, Description, CreatedAt FROM dbo.Department WHERE DepartmentId = @DepartmentId";
        return await conn.QueryFirstOrDefaultAsync<Department>(sql, new { DepartmentId = departmentId });
    }
}
