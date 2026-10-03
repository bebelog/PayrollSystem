using System.Data;
using Dapper;
using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins.DataStore.Sql;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ConfigurationRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PayrollSetting> GetCurrentSettingAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT TOP 1 SettingId, InsuranceCeiling, PersonalDeduction, DependentDeduction, EffectiveFrom, SourceNote FROM dbo.PayrollSetting ORDER BY EffectiveFrom DESC";
        var setting = await conn.QueryFirstOrDefaultAsync<PayrollSetting>(sql);
        return setting ?? new PayrollSetting
        {
            InsuranceCeiling = 50600000m,
            PersonalDeduction = 15500000m,
            DependentDeduction = 6200000m,
            EffectiveFrom = DateTime.Today,
            SourceNote = "GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC"
        };
    }

    public async Task UpdateSettingAsync(PayrollSetting setting)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.PayrollSetting
            SET InsuranceCeiling = @InsuranceCeiling,
                PersonalDeduction = @PersonalDeduction,
                DependentDeduction = @DependentDeduction,
                EffectiveFrom = @EffectiveFrom,
                SourceNote = @SourceNote
            WHERE SettingId = @SettingId";

        await conn.ExecuteAsync(sql, setting);
    }

    public async Task<IEnumerable<DeductionRate>> GetDeductionRatesAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT RateId, Code, Name, Rate, EffectiveFrom, SourceNote FROM dbo.DeductionRate ORDER BY Code";
        var result = await conn.QueryAsync<DeductionRate>(sql);
        return result.ToList();
    }

    public async Task UpdateDeductionRateAsync(DeductionRate rate)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.DeductionRate
            SET Name = @Name,
                Rate = @Rate,
                EffectiveFrom = @EffectiveFrom,
                SourceNote = @SourceNote
            WHERE RateId = @RateId";

        await conn.ExecuteAsync(sql, rate);
    }

    public async Task<IEnumerable<TaxBracket>> GetTaxBracketsAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT BracketId, BracketOrder, ThresholdMin, ThresholdMax, TaxRate, SourceNote FROM dbo.TaxBracket ORDER BY BracketOrder";
        var result = await conn.QueryAsync<TaxBracket>(sql);
        return result.ToList();
    }

    public async Task UpdateTaxBracketAsync(TaxBracket bracket)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.TaxBracket
            SET ThresholdMin = @ThresholdMin,
                ThresholdMax = @ThresholdMax,
                TaxRate = @TaxRate,
                SourceNote = @SourceNote
            WHERE BracketId = @BracketId";

        await conn.ExecuteAsync(sql, bracket);
    }
}
