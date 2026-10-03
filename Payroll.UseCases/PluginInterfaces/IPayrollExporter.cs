using Payroll.CoreBusiness.Entities;

namespace Payroll.UseCases.PluginInterfaces;

public interface IPayrollExporter
{
    Task<byte[]> ExportPayrollToExcelAsync(PayrollPeriod period, IEnumerable<Payslip> payslips);
}
