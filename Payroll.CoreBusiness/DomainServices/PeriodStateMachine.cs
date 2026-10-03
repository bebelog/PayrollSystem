using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;

namespace Payroll.CoreBusiness.DomainServices;

public static class PeriodStateMachine
{
    public static void ValidateTransition(PayrollPeriodStatus currentStatus, PayrollPeriodStatus targetStatus)
    {
        if (currentStatus == targetStatus) return;

        bool isValid = (currentStatus, targetStatus) switch
        {
            (PayrollPeriodStatus.Open, PayrollPeriodStatus.Calculated) => true,
            (PayrollPeriodStatus.Calculated, PayrollPeriodStatus.Open) => true, // Cho phép mở lại để tính lại
            (PayrollPeriodStatus.Calculated, PayrollPeriodStatus.Closed) => true,
            _ => false
        };

        if (!isValid)
        {
            throw new BusinessRuleException(
                $"Chuyển trạng thái kỳ lương không hợp lệ: Không thể chuyển từ '{GetStatusName(currentStatus)}' sang '{GetStatusName(targetStatus)}'.");
        }
    }

    public static void EnsurePeriodNotClosed(PayrollPeriod period)
    {
        if (period == null)
            throw new BusinessRuleException("Không tìm thấy thông tin kỳ lương.");

        if (period.Status == PayrollPeriodStatus.Closed)
            throw new BusinessRuleException("Kỳ lương đã chốt. Không được phép chỉnh sửa bảng công hoặc tính lại lương cho kỳ này.");
    }

    private static string GetStatusName(PayrollPeriodStatus status) => status switch
    {
        PayrollPeriodStatus.Open => "Mở",
        PayrollPeriodStatus.Calculated => "Đã tính",
        PayrollPeriodStatus.Closed => "Đã chốt",
        _ => status.ToString()
    };
}
