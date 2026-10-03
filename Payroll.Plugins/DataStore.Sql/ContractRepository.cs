using System.Data;
using Dapper;
using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins.DataStore.Sql;

public class ContractRepository : IContractRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ContractRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Contract>> GetContractsByEmployeeIdAsync(int employeeId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT ContractId, EmployeeId, ContractNumber, BaseSalary, Allowance, EffectiveFrom, EffectiveTo, IsActive
            FROM dbo.Contract
            WHERE EmployeeId = @EmployeeId
            ORDER BY EffectiveFrom DESC";

        var result = await conn.QueryAsync<Contract>(sql, new { EmployeeId = employeeId });
        return result.ToList();
    }

    public async Task<Contract?> GetActiveContractByEmployeeIdAsync(int employeeId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT TOP 1 ContractId, EmployeeId, ContractNumber, BaseSalary, Allowance, EffectiveFrom, EffectiveTo, IsActive
            FROM dbo.Contract
            WHERE EmployeeId = @EmployeeId AND IsActive = 1
            ORDER BY EffectiveFrom DESC";

        return await conn.QueryFirstOrDefaultAsync<Contract>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> AddContractAsync(Contract contract)
    {
        using var conn = _connectionFactory.CreateConnection();
        
        // Nếu hợp đồng mới là active, set các hợp đồng cũ thành inactive
        if (contract.IsActive)
        {
            await conn.ExecuteAsync("UPDATE dbo.Contract SET IsActive = 0 WHERE EmployeeId = @EmployeeId", new { contract.EmployeeId });
        }

        const string sql = @"
            INSERT INTO dbo.Contract (EmployeeId, ContractNumber, BaseSalary, Allowance, EffectiveFrom, EffectiveTo, IsActive)
            VALUES (@EmployeeId, @ContractNumber, @BaseSalary, @Allowance, @EffectiveFrom, @EffectiveTo, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        int id = await conn.ExecuteScalarAsync<int>(sql, contract);
        contract.ContractId = id;
        return id;
    }

    public async Task UpdateContractAsync(Contract contract)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.Contract
            SET ContractNumber = @ContractNumber,
                BaseSalary = @BaseSalary,
                Allowance = @Allowance,
                EffectiveFrom = @EffectiveFrom,
                EffectiveTo = @EffectiveTo,
                IsActive = @IsActive
            WHERE ContractId = @ContractId";

        await conn.ExecuteAsync(sql, contract);
    }
}
