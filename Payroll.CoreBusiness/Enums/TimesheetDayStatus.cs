namespace Payroll.CoreBusiness.Enums;

public enum TimesheetDayStatus
{
    Working = 1,      // Đi làm (tính công)
    PaidLeave = 2,    // Nghỉ phép có lương (tính công)
    UnpaidLeave = 3,  // Nghỉ không lương (không tính công)
    Holiday = 4       // Nghỉ lễ (tính công)
}
