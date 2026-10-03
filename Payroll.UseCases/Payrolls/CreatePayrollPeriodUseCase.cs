using Payroll.CoreBusiness.DomainServices;
using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.UseCases.Payrolls;

public interface ICreatePayrollPeriodUseCase
{
    Task<int> ExecuteAsync(int month, int year, decimal standardWorkingDays);
}

public class CreatePayrollPeriodUseCase : ICreatePayrollPeriodUseCase
{
    private readonly IPayrollRepository _payrollRepository;

    public CreatePayrollPeriodUseCase(IPayrollRepository payrollRepository)
    {
        _payrollRepository = payrollRepository;
    }

    public async Task<int> ExecuteAsync(int month, int year, decimal standardWorkingDays)
    {
        if (month < 1 || month > 12)
            throw new BusinessRuleException("Tháng phải từ 1 đến 12.");

        if (year < 2020 || year > 2100)
            throw new BusinessRuleException("Năm không hợp lệ.");

        if (standardWorkingDays <= 0)
            throw new BusinessRuleException("Số ngày công chuẩn phải lớn hơn 0.");

        var existing = await _payrollRepository.GetPeriodAsync(month, year);
        if (existing != null)
            throw new BusinessRuleException($"Kỳ lương tháng {month}/{year} đã tồn tại trong hệ thống.");

        var period = new PayrollPeriod
        {
            Month = month,
            Year = year,
            StandardWorkingDays = standardWorkingDays,
            Status = PayrollPeriodStatus.Open
        };

        return await _payrollRepository.CreatePeriodAsync(period);
    }
}
