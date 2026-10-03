using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Timesheets;

public interface IViewMyTimesheetUseCase
{
    Task<Timesheet?> ExecuteAsync(int? loggedEmployeeId, UserRole loggedUserRole, int targetEmployeeId, int month, int year);
}

public class ViewMyTimesheetUseCase : IViewMyTimesheetUseCase
{
    private readonly ITimesheetRepository _timesheetRepository;

    public ViewMyTimesheetUseCase(ITimesheetRepository timesheetRepository)
    {
        _timesheetRepository = timesheetRepository;
    }

    public async Task<Timesheet?> ExecuteAsync(int? loggedEmployeeId, UserRole loggedUserRole, int targetEmployeeId, int month, int year)
    {
        // LUẬT 2: Phân quyền theo dữ liệu. Nhân viên chỉ được xem bảng công của chính mình!
        if (loggedUserRole == UserRole.Employee && loggedEmployeeId != targetEmployeeId)
        {
            throw new BusinessRuleException("Bạn không có quyền xem bảng công của nhân viên khác.");
        }

        return await _timesheetRepository.GetTimesheetAsync(targetEmployeeId, month, year);
    }
}
