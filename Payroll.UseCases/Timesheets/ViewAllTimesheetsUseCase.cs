using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Timesheets;

public interface IViewAllTimesheetsUseCase
{
    Task<IEnumerable<Timesheet>> ExecuteAsync(int month, int year, int? departmentId = null, TimesheetStatus? status = null);
}

public class ViewAllTimesheetsUseCase : IViewAllTimesheetsUseCase
{
    private readonly ITimesheetRepository _timesheetRepository;

    public ViewAllTimesheetsUseCase(ITimesheetRepository timesheetRepository)
    {
        _timesheetRepository = timesheetRepository;
    }

    public async Task<IEnumerable<Timesheet>> ExecuteAsync(int month, int year, int? departmentId = null, TimesheetStatus? status = null)
    {
        return await _timesheetRepository.GetTimesheetsByPeriodAsync(month, year, departmentId, status);
    }
}
