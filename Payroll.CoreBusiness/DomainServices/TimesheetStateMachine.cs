using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;

namespace Payroll.CoreBusiness.DomainServices;

public static class TimesheetStateMachine
{
    public static void ValidateTransition(TimesheetStatus currentStatus, TimesheetStatus targetStatus)
    {
        if (currentStatus == targetStatus) return;

        bool isValid = (currentStatus, targetStatus) switch
        {
            (TimesheetStatus.Draft, TimesheetStatus.Submitted) => true,
            (TimesheetStatus.Draft, TimesheetStatus.Approved) => true, // Quản trị viên duyệt trực tiếp
            (TimesheetStatus.Submitted, TimesheetStatus.Approved) => true,
            (TimesheetStatus.Submitted, TimesheetStatus.Draft) => true, // Từ chối hoặc trả lại để sửa
            (TimesheetStatus.Approved, TimesheetStatus.Draft) => true,  // Hủy duyệt để chỉnh sửa nếu kỳ chưa chốt
            _ => false
        };

        if (!isValid)
        {
            throw new BusinessRuleException(
                $"Chuyển trạng thái bảng công không hợp lệ: Không thể chuyển từ '{GetStatusName(currentStatus)}' sang '{GetStatusName(targetStatus)}'.");
        }
    }

    public static void EnsureCanCalculateSalary(Timesheet timesheet)
    {
        if (timesheet == null)
            throw new BusinessRuleException("Không tìm thấy bảng công của nhân viên.");

        if (timesheet.Status != TimesheetStatus.Approved)
            throw new BusinessRuleException($"Chỉ tính lương cho nhân viên có bảng công Đã duyệt. Hiện tại: '{GetStatusName(timesheet.Status)}'.");
    }

    private static string GetStatusName(TimesheetStatus status) => status switch
    {
        TimesheetStatus.Draft => "Nháp",
        TimesheetStatus.Submitted => "Đã gửi",
        TimesheetStatus.Approved => "Đã duyệt",
        _ => status.ToString()
    };
}
