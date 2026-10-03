using System.Data;
using Dapper;
using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins.DataStore.Sql;

public class PayrollRepository : IPayrollRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public PayrollRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PayrollPeriod?> GetPeriodAsync(int month, int year)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT PeriodId, Month, Year, StandardWorkingDays, Status, CreatedAt, ClosedAt FROM dbo.PayrollPeriod WHERE Month = @Month AND Year = @Year";
        return await conn.QueryFirstOrDefaultAsync<PayrollPeriod>(sql, new { Month = month, Year = year });
    }

    public async Task<PayrollPeriod?> GetPeriodByIdAsync(int periodId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT PeriodId, Month, Year, StandardWorkingDays, Status, CreatedAt, ClosedAt FROM dbo.PayrollPeriod WHERE PeriodId = @PeriodId";
        return await conn.QueryFirstOrDefaultAsync<PayrollPeriod>(sql, new { PeriodId = periodId });
    }

    public async Task<IEnumerable<PayrollPeriod>> GetAllPeriodsAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT PeriodId, Month, Year, StandardWorkingDays, Status, CreatedAt, ClosedAt FROM dbo.PayrollPeriod ORDER BY Year DESC, Month DESC";
        var result = await conn.QueryAsync<PayrollPeriod>(sql);
        return result.ToList();
    }

    public async Task<int> CreatePeriodAsync(PayrollPeriod period)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.PayrollPeriod (Month, Year, StandardWorkingDays, Status, CreatedAt)
            VALUES (@Month, @Year, @StandardWorkingDays, @Status, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        int id = await conn.ExecuteScalarAsync<int>(sql, period);
        period.PeriodId = id;
        return id;
    }

    public async Task UpdatePeriodStatusAsync(int periodId, PayrollPeriodStatus status)
    {
        using var conn = _connectionFactory.CreateConnection();
        string sql = status == PayrollPeriodStatus.Closed 
            ? "UPDATE dbo.PayrollPeriod SET Status = @Status, ClosedAt = GETDATE() WHERE PeriodId = @PeriodId"
            : "UPDATE dbo.PayrollPeriod SET Status = @Status WHERE PeriodId = @PeriodId";

        await conn.ExecuteAsync(sql, new { PeriodId = periodId, Status = (int)status });
    }

    public async Task<Payslip?> GetPayslipAsync(int periodId, int employeeId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT p.PayslipId, p.PeriodId, p.EmployeeId, p.BaseSalarySnapshot, p.StandardDaysSnapshot, p.ActualDaysSnapshot,
                   p.WorkingSalary, p.AllowanceSnapshot, p.GrossSalary, p.TotalInsuranceDeduction, p.PersonalDeductionSnapshot,
                   p.DependentDeductionSnapshot, p.DependentCountSnapshot, p.TaxableIncome, p.PersonalIncomeTax, p.NetSalary, p.CreatedAt,
                   e.EmployeeId, e.FullName, e.Email, e.PhoneNumber, e.IdentityNumber, e.DepartmentId,
                   d.DepartmentId, d.DepartmentName,
                   per.PeriodId, per.Month, per.Year, per.StandardWorkingDays, per.Status
            FROM dbo.Payslip p
            INNER JOIN dbo.Employee e ON p.EmployeeId = e.EmployeeId
            INNER JOIN dbo.Department d ON e.DepartmentId = d.DepartmentId
            INNER JOIN dbo.PayrollPeriod per ON p.PeriodId = per.PeriodId
            WHERE p.PeriodId = @PeriodId AND p.EmployeeId = @EmployeeId;

            SELECT LineId, PayslipId, LineType, ItemCode, ItemName, RateOrThresholdSnapshot, Amount, Note
            FROM dbo.PayslipLine
            WHERE PayslipId = (SELECT PayslipId FROM dbo.Payslip WHERE PeriodId = @PeriodId AND EmployeeId = @EmployeeId)
            ORDER BY LineType, LineId;";

        using var multi = await conn.QueryMultipleAsync(sql, new { PeriodId = periodId, EmployeeId = employeeId });
        var payslipList = multi.Read<Payslip, Employee, Department, PayrollPeriod, Payslip>((p, emp, dept, period) =>
        {
            emp.Department = dept;
            p.Employee = emp;
            p.Period = period;
            return p;
        }, splitOn: "EmployeeId,DepartmentId,PeriodId");

        var payslip = payslipList.FirstOrDefault();
        if (payslip != null)
        {
            payslip.Lines = (await multi.ReadAsync<PayslipLine>()).ToList();
        }
        return payslip;
    }

    public async Task<Payslip?> GetPayslipByIdAsync(int payslipId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT p.PayslipId, p.PeriodId, p.EmployeeId, p.BaseSalarySnapshot, p.StandardDaysSnapshot, p.ActualDaysSnapshot,
                   p.WorkingSalary, p.AllowanceSnapshot, p.GrossSalary, p.TotalInsuranceDeduction, p.PersonalDeductionSnapshot,
                   p.DependentDeductionSnapshot, p.DependentCountSnapshot, p.TaxableIncome, p.PersonalIncomeTax, p.NetSalary, p.CreatedAt,
                   e.EmployeeId, e.FullName, e.Email, e.PhoneNumber, e.IdentityNumber, e.DepartmentId,
                   d.DepartmentId, d.DepartmentName,
                   per.PeriodId, per.Month, per.Year, per.StandardWorkingDays, per.Status
            FROM dbo.Payslip p
            INNER JOIN dbo.Employee e ON p.EmployeeId = e.EmployeeId
            INNER JOIN dbo.Department d ON e.DepartmentId = d.DepartmentId
            INNER JOIN dbo.PayrollPeriod per ON p.PeriodId = per.PeriodId
            WHERE p.PayslipId = @PayslipId;

            SELECT LineId, PayslipId, LineType, ItemCode, ItemName, RateOrThresholdSnapshot, Amount, Note
            FROM dbo.PayslipLine
            WHERE PayslipId = @PayslipId
            ORDER BY LineType, LineId;";

        using var multi = await conn.QueryMultipleAsync(sql, new { PayslipId = payslipId });
        var payslipList = multi.Read<Payslip, Employee, Department, PayrollPeriod, Payslip>((p, emp, dept, period) =>
        {
            emp.Department = dept;
            p.Employee = emp;
            p.Period = period;
            return p;
        }, splitOn: "EmployeeId,DepartmentId,PeriodId");

        var payslip = payslipList.FirstOrDefault();
        if (payslip != null)
        {
            payslip.Lines = (await multi.ReadAsync<PayslipLine>()).ToList();
        }
        return payslip;
    }

    public async Task<IEnumerable<Payslip>> GetPayslipsByPeriodIdAsync(int periodId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT p.PayslipId, p.PeriodId, p.EmployeeId, p.BaseSalarySnapshot, p.StandardDaysSnapshot, p.ActualDaysSnapshot,
                   p.WorkingSalary, p.AllowanceSnapshot, p.GrossSalary, p.TotalInsuranceDeduction, p.PersonalDeductionSnapshot,
                   p.DependentDeductionSnapshot, p.DependentCountSnapshot, p.TaxableIncome, p.PersonalIncomeTax, p.NetSalary, p.CreatedAt,
                   e.EmployeeId, e.FullName, e.Email, e.PhoneNumber, e.IdentityNumber, e.DepartmentId,
                   d.DepartmentId, d.DepartmentName
            FROM dbo.Payslip p
            INNER JOIN dbo.Employee e ON p.EmployeeId = e.EmployeeId
            INNER JOIN dbo.Department d ON e.DepartmentId = d.DepartmentId
            WHERE p.PeriodId = @PeriodId
            ORDER BY d.DepartmentName, e.FullName";

        var result = await conn.QueryAsync<Payslip, Employee, Department, Payslip>(
            sql,
            (payslip, emp, dept) =>
            {
                emp.Department = dept;
                payslip.Employee = emp;
                return payslip;
            },
            new { PeriodId = periodId },
            splitOn: "EmployeeId,DepartmentId");

        return result.ToList();
    }

    /// <summary>
    /// RÀNG BUỘC TOÀN VẸN ACID: Lưu đồng thời Payslip (Header) và PayslipLine (Line) trong 1 Transaction
    /// </summary>
    public async Task SaveCalculatedPayslipsAsync(int periodId, IEnumerable<Payslip> payslips)
    {
        using var conn = _connectionFactory.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            // 1. Xóa toàn bộ phiếu và dòng chi tiết cũ của kỳ này (nếu đã tính trước đó)
            await conn.ExecuteAsync(@"
                DELETE FROM dbo.PayslipLine 
                WHERE PayslipId IN (SELECT PayslipId FROM dbo.Payslip WHERE PeriodId = @PeriodId)",
                new { PeriodId = periodId }, tx);

            await conn.ExecuteAsync("DELETE FROM dbo.Payslip WHERE PeriodId = @PeriodId", new { PeriodId = periodId }, tx);

            // 2. Chèn từng Payslip và các dòng PayslipLine trong transaction
            const string insertPayslipSql = @"
                INSERT INTO dbo.Payslip (
                    PeriodId, EmployeeId, BaseSalarySnapshot, StandardDaysSnapshot, ActualDaysSnapshot,
                    WorkingSalary, AllowanceSnapshot, GrossSalary, TotalInsuranceDeduction,
                    PersonalDeductionSnapshot, DependentDeductionSnapshot, DependentCountSnapshot,
                    TaxableIncome, PersonalIncomeTax, NetSalary, CreatedAt
                ) VALUES (
                    @PeriodId, @EmployeeId, @BaseSalarySnapshot, @StandardDaysSnapshot, @ActualDaysSnapshot,
                    @WorkingSalary, @AllowanceSnapshot, @GrossSalary, @TotalInsuranceDeduction,
                    @PersonalDeductionSnapshot, @DependentDeductionSnapshot, @DependentCountSnapshot,
                    @TaxableIncome, @PersonalIncomeTax, @NetSalary, @CreatedAt
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            const string insertLineSql = @"
                INSERT INTO dbo.PayslipLine (
                    PayslipId, LineType, ItemCode, ItemName, RateOrThresholdSnapshot, Amount, Note
                ) VALUES (
                    @PayslipId, @LineType, @ItemCode, @ItemName, @RateOrThresholdSnapshot, @Amount, @Note
                );";

            foreach (var p in payslips)
            {
                p.PeriodId = periodId;
                int payslipId = await conn.ExecuteScalarAsync<int>(insertPayslipSql, p, tx);
                p.PayslipId = payslipId;

                foreach (var line in p.Lines)
                {
                    line.PayslipId = payslipId;
                    await conn.ExecuteAsync(insertLineSql, line, tx);
                }
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task DeletePayslipsByPeriodIdAsync(int periodId)
    {
        using var conn = _connectionFactory.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync(@"
                DELETE FROM dbo.PayslipLine 
                WHERE PayslipId IN (SELECT PayslipId FROM dbo.Payslip WHERE PeriodId = @PeriodId)",
                new { PeriodId = periodId }, tx);

            await conn.ExecuteAsync("DELETE FROM dbo.Payslip WHERE PeriodId = @PeriodId", new { PeriodId = periodId }, tx);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
