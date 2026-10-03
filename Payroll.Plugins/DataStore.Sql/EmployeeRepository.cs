using System.Data;
using Dapper;
using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins.DataStore.Sql;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public EmployeeRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Employee>> GetEmployeesAsync(string? searchTerm = null, int? departmentId = null, bool? isActive = null)
    {
        using var conn = _connectionFactory.CreateConnection();
        
        string sql = @"
            SELECT e.EmployeeId, e.FullName, e.Email, e.PhoneNumber, e.IdentityNumber, e.DepartmentId, e.HireDate, e.IsActive,
                   d.DepartmentId, d.DepartmentName, d.Description,
                   c.ContractId, c.EmployeeId, c.ContractNumber, c.BaseSalary, c.Allowance, c.EffectiveFrom, c.EffectiveTo, c.IsActive
            FROM dbo.Employee e
            INNER JOIN dbo.Department d ON e.DepartmentId = d.DepartmentId
            LEFT JOIN dbo.Contract c ON e.EmployeeId = c.EmployeeId AND c.IsActive = 1
            WHERE (@SearchTerm IS NULL 
                   OR e.FullName LIKE '%' + @SearchTerm + '%' 
                   OR e.Email LIKE '%' + @SearchTerm + '%'
                   OR e.PhoneNumber LIKE '%' + @SearchTerm + '%')
              AND (@DepartmentId IS NULL OR e.DepartmentId = @DepartmentId)
              AND (@IsActive IS NULL OR e.IsActive = @IsActive)
            ORDER BY e.FullName";

        var empMap = new Dictionary<int, Employee>();

        var rows = await conn.QueryAsync<Employee, Department, Contract, Employee>(
            sql,
            (employee, department, contract) =>
            {
                if (!empMap.TryGetValue(employee.EmployeeId, out var currentEmp))
                {
                    currentEmp = employee;
                    currentEmp.Department = department;
                    currentEmp.Contracts = new List<Contract>();
                    empMap.Add(currentEmp.EmployeeId, currentEmp);
                }

                if (contract != null && currentEmp.Contracts != null && !currentEmp.Contracts.Any(c => c.ContractId == contract.ContractId))
                {
                    currentEmp.Contracts.Add(contract);
                }

                return currentEmp;
            },
            new { SearchTerm = searchTerm, DepartmentId = departmentId, IsActive = isActive },
            splitOn: "DepartmentId,ContractId");

        return empMap.Values.ToList();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
    {
        using var conn = _connectionFactory.CreateConnection();
        
        string sql = @"
            SELECT EmployeeId, FullName, Email, PhoneNumber, IdentityNumber, DepartmentId, HireDate, IsActive
            FROM dbo.Employee WHERE EmployeeId = @EmployeeId;

            SELECT d.DepartmentId, d.DepartmentName, d.Description
            FROM dbo.Department d
            WHERE d.DepartmentId = (SELECT DepartmentId FROM dbo.Employee WHERE EmployeeId = @EmployeeId);

            SELECT ContractId, EmployeeId, ContractNumber, BaseSalary, Allowance, EffectiveFrom, EffectiveTo, IsActive
            FROM dbo.Contract
            WHERE EmployeeId = @EmployeeId ORDER BY EffectiveFrom DESC;

            SELECT DependentId, EmployeeId, FullName, Relationship, DateOfBirth, IdentityNumber, IsActive
            FROM dbo.Dependent
            WHERE EmployeeId = @EmployeeId ORDER BY FullName;";

        using var multi = await conn.QueryMultipleAsync(sql, new { EmployeeId = employeeId });
        var employee = await multi.ReadFirstOrDefaultAsync<Employee>();

        if (employee != null)
        {
            employee.Department = await multi.ReadFirstOrDefaultAsync<Department>();
            employee.Contracts = (await multi.ReadAsync<Contract>()).ToList();
            employee.Dependents = (await multi.ReadAsync<Dependent>()).ToList();
        }

        return employee;
    }

    public async Task<int> AddEmployeeAsync(Employee employee)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.Employee (FullName, Email, PhoneNumber, IdentityNumber, DepartmentId, HireDate, IsActive)
            VALUES (@FullName, @Email, @PhoneNumber, @IdentityNumber, @DepartmentId, @HireDate, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        int id = await conn.ExecuteScalarAsync<int>(sql, employee);
        employee.EmployeeId = id;
        return id;
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.Employee
            SET FullName = @FullName,
                Email = @Email,
                PhoneNumber = @PhoneNumber,
                IdentityNumber = @IdentityNumber,
                DepartmentId = @DepartmentId,
                HireDate = @HireDate,
                IsActive = @IsActive
            WHERE EmployeeId = @EmployeeId";

        await conn.ExecuteAsync(sql, employee);
    }

    public async Task DeleteEmployeeAsync(int employeeId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "UPDATE dbo.Employee SET IsActive = 0 WHERE EmployeeId = @EmployeeId";
        await conn.ExecuteAsync(sql, new { EmployeeId = employeeId });
    }
}
