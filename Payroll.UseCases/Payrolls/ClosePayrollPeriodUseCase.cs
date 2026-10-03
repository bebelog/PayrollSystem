using Payroll.CoreBusiness.DomainServices;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Payrolls;

public interface IClosePayrollPeriodUseCase
{
    Task ExecuteAsync(int periodId);
}

public class ClosePayrollPeriodUseCase : IClosePayrollPeriodUseCase
{
    private readonly IPayrollRepository _payrollRepository;

    public ClosePayrollPeriodUseCase(IPayrollRepository payrollRepository)
    {
        _payrollRepository = payrollRepository;
    }

    public async Task ExecuteAsync(int periodId)
    {
        var period = await _payrollRepository.GetPeriodByIdAsync(periodId);
        if (period == null)
            throw new BusinessRuleException("Không tìm thấy kỳ lương.");

        if (period.Status != PayrollPeriodStatus.Calculated)
            throw new BusinessRuleException("Chỉ có thể chốt kỳ lương khi đã hoàn tất tính lương (trạng thái Đã tính).");

        PeriodStateMachine.ValidateTransition(period.Status, PayrollPeriodStatus.Closed);
        await _payrollRepository.UpdatePeriodStatusAsync(periodId, PayrollPeriodStatus.Closed);
    }
}
