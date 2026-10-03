using Payroll.CoreBusiness.Entities;

namespace Payroll.UseCases.PluginInterfaces;

public interface IConfigurationRepository
{
    Task<PayrollSetting> GetCurrentSettingAsync();
    Task UpdateSettingAsync(PayrollSetting setting);
    Task<IEnumerable<DeductionRate>> GetDeductionRatesAsync();
    Task UpdateDeductionRateAsync(DeductionRate rate);
    Task<IEnumerable<TaxBracket>> GetTaxBracketsAsync();
    Task UpdateTaxBracketAsync(TaxBracket bracket);
}
