using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Payrolls;

public interface IViewMyPayslipUseCase
{
    Task<Payslip?> ExecuteAsync(int? loggedEmployeeId, UserRole loggedUserRole, int targetEmployeeId, int periodId);
}

public class ViewMyPayslipUseCase : IViewMyPayslipUseCase
{
    private readonly IPayrollRepository _payrollRepository;

    public ViewMyPayslipUseCase(IPayrollRepository payrollRepository)
    {
        _payrollRepository = payrollRepository;
    }

    public async Task<Payslip?> ExecuteAsync(int? loggedEmployeeId, UserRole loggedUserRole, int targetEmployeeId, int periodId)
    {
        // LUẬT 2: Phân quyền theo dữ liệu. Nhân viên chỉ xem được phiếu lương của chính mình!
        if (loggedUserRole == UserRole.Employee && loggedEmployeeId != targetEmployeeId)
        {
            throw new BusinessRuleException("Bạn không có quyền xem phiếu lương của nhân viên khác.");
        }

        return await _payrollRepository.GetPayslipAsync(periodId, targetEmployeeId);
    }
}
