using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Configurations;

public interface IUpdatePayrollSettingsUseCase
{
    Task ExecuteAsync(PayrollSetting setting);
}

public class UpdatePayrollSettingsUseCase : IUpdatePayrollSettingsUseCase
{
    private readonly IConfigurationRepository _configurationRepository;

    public UpdatePayrollSettingsUseCase(IConfigurationRepository configurationRepository)
    {
        _configurationRepository = configurationRepository;
    }

    public async Task ExecuteAsync(PayrollSetting setting)
    {
        if (setting.InsuranceCeiling <= 0)
            throw new BusinessRuleException("Trần đóng bảo hiểm phải lớn hơn 0.");

        if (setting.PersonalDeduction < 0 || setting.DependentDeduction < 0)
            throw new BusinessRuleException("Mức giảm trừ không được âm.");

        await _configurationRepository.UpdateSettingAsync(setting);
    }
}
