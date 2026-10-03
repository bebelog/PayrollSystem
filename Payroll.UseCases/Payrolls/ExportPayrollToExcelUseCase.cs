using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Payrolls;

public interface IExportPayrollToExcelUseCase
{
    Task<byte[]> ExecuteAsync(int periodId);
}

public class ExportPayrollToExcelUseCase : IExportPayrollToExcelUseCase
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IPayrollExporter _payrollExporter;

    public ExportPayrollToExcelUseCase(IPayrollRepository payrollRepository, IPayrollExporter payrollExporter)
    {
        _payrollRepository = payrollRepository;
        _payrollExporter = payrollExporter;
    }

    public async Task<byte[]> ExecuteAsync(int periodId)
    {
        var period = await _payrollRepository.GetPeriodByIdAsync(periodId);
        if (period == null)
            throw new BusinessRuleException("Không tìm thấy kỳ lương.");

        var payslips = await _payrollRepository.GetPayslipsByPeriodIdAsync(periodId);
        return await _payrollExporter.ExportPayrollToExcelAsync(period, payslips);
    }
}
