using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Payrolls;

public interface IViewPayrollPeriodsUseCase
{
    Task<IEnumerable<PayrollPeriod>> ExecuteAsync();
    Task<PayrollPeriod?> GetByIdAsync(int periodId);
}

public class ViewPayrollPeriodsUseCase : IViewPayrollPeriodsUseCase
{
    private readonly IPayrollRepository _payrollRepository;

    public ViewPayrollPeriodsUseCase(IPayrollRepository payrollRepository)
    {
        _payrollRepository = payrollRepository;
    }

    public async Task<IEnumerable<PayrollPeriod>> ExecuteAsync()
    {
        return await _payrollRepository.GetAllPeriodsAsync();
    }

    public async Task<PayrollPeriod?> GetByIdAsync(int periodId)
    {
        return await _payrollRepository.GetPeriodByIdAsync(periodId);
    }
}
