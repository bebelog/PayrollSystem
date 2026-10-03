using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Configurations;

public interface IViewPayrollSettingsUseCase
{
    Task<(PayrollSetting Setting, IEnumerable<DeductionRate> Rates, IEnumerable<TaxBracket> Brackets)> ExecuteAsync();
}

public class ViewPayrollSettingsUseCase : IViewPayrollSettingsUseCase
{
    private readonly IConfigurationRepository _configurationRepository;

    public ViewPayrollSettingsUseCase(IConfigurationRepository configurationRepository)
    {
        _configurationRepository = configurationRepository;
    }

    public async Task<(PayrollSetting Setting, IEnumerable<DeductionRate> Rates, IEnumerable<TaxBracket> Brackets)> ExecuteAsync()
    {
        var setting = await _configurationRepository.GetCurrentSettingAsync();
        var rates = await _configurationRepository.GetDeductionRatesAsync();
        var brackets = await _configurationRepository.GetTaxBracketsAsync();
        return (setting, rates, brackets);
    }
}
