using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;

namespace Payroll.UseCases.PluginInterfaces;

public interface IPayrollRepository
{
    Task<PayrollPeriod?> GetPeriodAsync(int month, int year);
    Task<PayrollPeriod?> GetPeriodByIdAsync(int periodId);
    Task<IEnumerable<PayrollPeriod>> GetAllPeriodsAsync();
    Task<int> CreatePeriodAsync(PayrollPeriod period);
    Task UpdatePeriodStatusAsync(int periodId, PayrollPeriodStatus status);
    
    Task<Payslip?> GetPayslipAsync(int periodId, int employeeId);
    Task<Payslip?> GetPayslipByIdAsync(int payslipId);
    Task<IEnumerable<Payslip>> GetPayslipsByPeriodIdAsync(int periodId);
    
    // Cặp Header-Line bắt buộc lưu trong 1 DB Transaction
    Task SaveCalculatedPayslipsAsync(int periodId, IEnumerable<Payslip> payslips);
    Task DeletePayslipsByPeriodIdAsync(int periodId);
}
