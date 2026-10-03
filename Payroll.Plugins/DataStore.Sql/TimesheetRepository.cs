using System.Data;
using Dapper;
using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins.DataStore.Sql;

public class TimesheetRepository : ITimesheetRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public TimesheetRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Timesheet?> GetTimesheetAsync(int employeeId, int month, int year)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT TimesheetId, EmployeeId, Month, Year, Status, SubmittedAt, ApprovedAt, ApprovedByUserId
            FROM dbo.Timesheet
            WHERE EmployeeId = @EmployeeId AND Month = @Month AND Year = @Year;

            SELECT EntryId, TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note
            FROM dbo.TimesheetEntry
            WHERE TimesheetId = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = @EmployeeId AND Month = @Month AND Year = @Year)
            ORDER BY WorkDate;";

        using var multi = await conn.QueryMultipleAsync(sql, new { EmployeeId = employeeId, Month = month, Year = year });
        var timesheet = await multi.ReadFirstOrDefaultAsync<Timesheet>();
        if (timesheet != null)
        {
            timesheet.Entries = (await multi.ReadAsync<TimesheetEntry>()).ToList();
        }
        return timesheet;
    }

    public async Task<Timesheet?> GetTimesheetByIdAsync(int timesheetId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT TimesheetId, EmployeeId, Month, Year, Status, SubmittedAt, ApprovedAt, ApprovedByUserId
            FROM dbo.Timesheet
            WHERE TimesheetId = @TimesheetId;

            SELECT e.EmployeeId, e.FullName, e.Email, e.DepartmentId, d.DepartmentId, d.DepartmentName
            FROM dbo.Employee e
            INNER JOIN dbo.Department d ON e.DepartmentId = d.DepartmentId
            WHERE e.EmployeeId = (SELECT EmployeeId FROM dbo.Timesheet WHERE TimesheetId = @TimesheetId);

            SELECT EntryId, TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note
            FROM dbo.TimesheetEntry
            WHERE TimesheetId = @TimesheetId
            ORDER BY WorkDate;";

        using var multi = await conn.QueryMultipleAsync(sql, new { TimesheetId = timesheetId });
        var timesheet = await multi.ReadFirstOrDefaultAsync<Timesheet>();
        if (timesheet != null)
        {
            var empList = multi.Read<Employee, Department, Employee>((emp, dept) =>
            {
                emp.Department = dept;
                return emp;
            }, splitOn: "DepartmentId");
            timesheet.Employee = empList.FirstOrDefault();
            timesheet.Entries = (await multi.ReadAsync<TimesheetEntry>()).ToList();
        }
        return timesheet;
    }

    public async Task<IEnumerable<Timesheet>> GetTimesheetsByPeriodAsync(int month, int year, int? departmentId = null, TimesheetStatus? status = null)
    {
        using var conn = _connectionFactory.CreateConnection();
        string sql = @"
            SELECT t.TimesheetId, t.EmployeeId, t.Month, t.Year, t.Status, t.SubmittedAt, t.ApprovedAt, t.ApprovedByUserId,
                   e.EmployeeId, e.FullName, e.Email, e.DepartmentId,
                   d.DepartmentId, d.DepartmentName,
                   te.EntryId, te.TimesheetId, te.WorkDate, te.DayStatus, te.CheckInTime, te.CheckOutTime, te.WorkingHours, te.Note
            FROM dbo.Timesheet t
            INNER JOIN dbo.Employee e ON t.EmployeeId = e.EmployeeId
            INNER JOIN dbo.Department d ON e.DepartmentId = d.DepartmentId
            LEFT JOIN dbo.TimesheetEntry te ON t.TimesheetId = te.TimesheetId
            WHERE t.Month = @Month AND t.Year = @Year
              AND (@DepartmentId IS NULL OR e.DepartmentId = @DepartmentId)
              AND (@Status IS NULL OR t.Status = @Status)
            ORDER BY d.DepartmentName, e.FullName";

        var timesheetMap = new Dictionary<int, Timesheet>();

        var rows = await conn.QueryAsync<Timesheet, Employee, Department, TimesheetEntry, Timesheet>(
            sql,
            (timesheet, employee, department, entry) =>
            {
                if (!timesheetMap.TryGetValue(timesheet.TimesheetId, out var currentTimesheet))
                {
                    currentTimesheet = timesheet;
                    employee.Department = department;
                    currentTimesheet.Employee = employee;
                    currentTimesheet.Entries = new List<TimesheetEntry>();
                    timesheetMap.Add(currentTimesheet.TimesheetId, currentTimesheet);
                }

                if (entry != null && !currentTimesheet.Entries.Any(e => e.EntryId == entry.EntryId))
                {
                    currentTimesheet.Entries.Add(entry);
                }

                return currentTimesheet;
            },
            new { Month = month, Year = year, DepartmentId = departmentId, Status = (int?)status },
            splitOn: "EmployeeId,DepartmentId,EntryId");

        return timesheetMap.Values.ToList();
    }

    public async Task<int> CreateOrUpdateTimesheetHeaderAsync(Timesheet timesheet)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string checkSql = "SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = @EmployeeId AND Month = @Month AND Year = @Year";
        int existingId = await conn.QueryFirstOrDefaultAsync<int>(checkSql, new { timesheet.EmployeeId, timesheet.Month, timesheet.Year });

        if (existingId > 0)
        {
            timesheet.TimesheetId = existingId;
            return existingId;
        }

        const string insertSql = @"
            INSERT INTO dbo.Timesheet (EmployeeId, Month, Year, Status, SubmittedAt, ApprovedAt, ApprovedByUserId)
            VALUES (@EmployeeId, @Month, @Year, @Status, @SubmittedAt, @ApprovedAt, @ApprovedByUserId);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        int id = await conn.ExecuteScalarAsync<int>(insertSql, timesheet);
        timesheet.TimesheetId = id;
        return id;
    }

    public async Task SaveTimesheetEntriesAsync(int timesheetId, IEnumerable<TimesheetEntry> entries)
    {
        using var conn = _connectionFactory.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync("DELETE FROM dbo.TimesheetEntry WHERE TimesheetId = @TimesheetId", new { TimesheetId = timesheetId }, tx);
            const string insertSql = @"
                INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note)
                VALUES (@TimesheetId, @WorkDate, @DayStatus, @CheckInTime, @CheckOutTime, @WorkingHours, @Note);";

            foreach (var e in entries)
            {
                e.TimesheetId = timesheetId;
                await conn.ExecuteAsync(insertSql, e, tx);
            }
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task UpdateTimesheetStatusAsync(int timesheetId, TimesheetStatus status, int? approvedByUserId = null)
    {
        using var conn = _connectionFactory.CreateConnection();
        string sql;
        if (status == TimesheetStatus.Submitted)
        {
            sql = "UPDATE dbo.Timesheet SET Status = @Status, SubmittedAt = GETDATE() WHERE TimesheetId = @TimesheetId";
        }
        else if (status == TimesheetStatus.Approved)
        {
            sql = "UPDATE dbo.Timesheet SET Status = @Status, ApprovedAt = GETDATE(), ApprovedByUserId = @ApprovedByUserId WHERE TimesheetId = @TimesheetId";
        }
        else
        {
            sql = "UPDATE dbo.Timesheet SET Status = @Status WHERE TimesheetId = @TimesheetId";
        }

        await conn.ExecuteAsync(sql, new { TimesheetId = timesheetId, Status = (int)status, ApprovedByUserId = approvedByUserId });
    }

    public async Task<TimesheetEntry?> GetEntryByDateAsync(int timesheetId, DateTime workDate)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT EntryId, TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note FROM dbo.TimesheetEntry WHERE TimesheetId = @TimesheetId AND WorkDate = @WorkDate";
        return await conn.QueryFirstOrDefaultAsync<TimesheetEntry>(sql, new { TimesheetId = timesheetId, WorkDate = workDate.Date });
    }

    public async Task UpsertTimesheetEntryAsync(TimesheetEntry entry)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            IF EXISTS (SELECT 1 FROM dbo.TimesheetEntry WHERE TimesheetId = @TimesheetId AND WorkDate = @WorkDate)
            BEGIN
                UPDATE dbo.TimesheetEntry
                SET DayStatus = @DayStatus,
                    CheckInTime = COALESCE(@CheckInTime, CheckInTime),
                    CheckOutTime = COALESCE(@CheckOutTime, CheckOutTime),
                    WorkingHours = COALESCE(@WorkingHours, WorkingHours),
                    Note = @Note
                WHERE TimesheetId = @TimesheetId AND WorkDate = @WorkDate;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note)
                VALUES (@TimesheetId, @WorkDate, @DayStatus, @CheckInTime, @CheckOutTime, @WorkingHours, @Note);
            END";

        await conn.ExecuteAsync(sql, new
        {
            entry.TimesheetId,
            WorkDate = entry.WorkDate.Date,
            DayStatus = (int)entry.DayStatus,
            entry.CheckInTime,
            entry.CheckOutTime,
            entry.WorkingHours,
            entry.Note
        });
    }
}