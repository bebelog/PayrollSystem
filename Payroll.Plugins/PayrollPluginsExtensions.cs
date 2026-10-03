using Microsoft.Extensions.DependencyInjection;
using Payroll.Plugins.DataStore.Sql;
using Payroll.Plugins.Exporters;
using Payroll.Plugins.Security;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins;

public static class PayrollPluginsExtensions
{
    public static IServiceCollection AddPayrollSqlPlugins(this IServiceCollection services, string connectionString)
    {
        var sqlConnFactory = new SqlConnectionFactory(connectionString);
        services.AddSingleton<ISqlConnectionFactory>(sqlConnFactory);
        services.AddSingleton<SqlConnectionFactory>(sqlConnFactory);

        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IDependentRepository, DependentRepository>();
        services.AddScoped<ITimesheetRepository, TimesheetRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPayrollExporter, PayrollExcelExporter>();

        return services;
    }
}
