# Sơ Đồ Cơ Sở Dữ Liệu (ERD) - Chuẩn 3NF
**Hệ Thống Chấm Công & Tính Lương (SmartPayroll)**  
*Sinh viên thực hiện:* Nguyễn Viết Mẫn (MSSV: 23K4080026) - Trường ĐH Kinh tế, ĐH Huế  

---

## 1. Sơ Đồ Quan Hệ Thực Thể (Mermaid Diagram)

```mermaid
erDiagram
    DEPARTMENT ||--o{ EMPLOYEE : "thuộc về (1:N)"
    EMPLOYEE ||--o{ DEPENDENT : "có người phụ thuộc (1:N)"
    EMPLOYEE ||--o{ CONTRACT : "ký hợp đồng (1:N)"
    EMPLOYEE ||--o| APP_USER : "liên kết tài khoản (1:1)"
    EMPLOYEE ||--o{ TIMESHEET : "chấm công tháng (1:N)"
    TIMESHEET ||--o{ TIMESHEET_ENTRY : "chi tiết ngày (1:N)"
    
    PAYROLL_PERIOD ||--o{ PAYSLIP : "tính lương kỳ (1:N)"
    EMPLOYEE ||--o{ PAYSLIP : "nhận phiếu lương (1:N)"
    PAYSLIP ||--o{ PAYSLIP_LINE : "chi tiết khoản lương (1:N)"

    APP_USER ||--o{ TIMESHEET : "phê duyệt (1:N)"

    DEPARTMENT {
        int DepartmentId PK
        nvarchar DepartmentName
        nvarchar Description
        datetime CreatedAt
    }

    EMPLOYEE {
        int EmployeeId PK
        nvarchar FullName
        nvarchar Email UK
        nvarchar PhoneNumber
        nvarchar IdentityNumber UK
        int DepartmentId FK
        date HireDate
        bit IsActive
    }

    DEPENDENT {
        int DependentId PK
        int EmployeeId FK
        nvarchar FullName
        nvarchar Relationship
        date DateOfBirth
        nvarchar IdentityNumber
        bit IsActive
    }

    CONTRACT {
        int ContractId PK
        int EmployeeId FK
        nvarchar ContractNumber UK
        decimal BaseSalary
        decimal Allowance
        date EffectiveFrom
        date EffectiveTo
        bit IsActive
    }

    APP_USER {
        int UserId PK
        nvarchar Username UK
        nvarchar PasswordHash
        int Role "1=Employee, 2=Accountant"
        int EmployeeId FK
        bit IsActive
        datetime CreatedAt
    }

    TIMESHEET {
        int TimesheetId PK
        int EmployeeId FK
        int Month
        int Year
        int Status "1=Draft, 2=Submitted, 3=Approved"
        datetime SubmittedAt
        datetime ApprovedAt
        int ApprovedByUserId FK
    }

    TIMESHEET_ENTRY {
        int EntryId PK
        int TimesheetId FK
        date WorkDate
        int DayStatus "1=Work, 2=PaidLeave, 3=Unpaid, 4=Holiday"
        nvarchar Note
    }

    PAYROLL_PERIOD {
        int PeriodId PK
        int Month
        int Year
        decimal StandardWorkingDays
        int Status "1=Open, 2=Calculated, 3=Closed"
        datetime CreatedAt
        datetime ClosedAt
    }

    PAYSLIP {
        int PayslipId PK
        int PeriodId FK
        int EmployeeId FK
        decimal BaseSalarySnapshot
        decimal StandardDaysSnapshot
        decimal ActualDaysSnapshot
        decimal WorkingSalary
        decimal AllowanceSnapshot
        decimal GrossSalary
        decimal TotalInsuranceDeduction
        decimal PersonalDeductionSnapshot
        decimal DependentDeductionSnapshot
        int DependentCountSnapshot
        decimal TaxableIncome
        decimal PersonalIncomeTax
        decimal NetSalary
        datetime CreatedAt
    }

    PAYSLIP_LINE {
        int LineId PK
        int PayslipId FK
        int LineType "1=Earning, 2=Insurance, 3=TaxDed, 4=Tax, 5=Other"
        nvarchar ItemCode
        nvarchar ItemName
        decimal RateOrThresholdSnapshot
        decimal Amount
        nvarchar Note
    }

    PAYROLL_SETTING {
        int SettingId PK
        decimal InsuranceCeiling "50,600,000"
        decimal PersonalDeduction "15,500,000"
        decimal DependentDeduction "6,200,000"
        date EffectiveFrom
        nvarchar SourceNote
    }

    DEDUCTION_RATE {
        int RateId PK
        nvarchar Code UK "BHXH, BHYT, BHTN"
        nvarchar Name
        decimal Rate "0.08, 0.015, 0.01"
        date EffectiveFrom
        nvarchar SourceNote
    }

    TAX_BRACKET {
        int BracketId PK
        int BracketOrder UK "1..5"
        decimal ThresholdMin
        decimal ThresholdMax
        decimal TaxRate "0.05, 0.10, 0.20, 0.30, 0.35"
        nvarchar SourceNote
    }
```

---

## 2. Các Ràng Buộc Khóa & Chỉ Mục Quan Trọng

1. `UQ_Employee_Month_Year`: Ràng buộc `UNIQUE (EmployeeId, Month, Year)` trên bảng `Timesheet` — mỗi nhân viên chỉ có duy nhất 1 bảng công trong 1 tháng.
2. `UQ_Timesheet_WorkDate`: Ràng buộc `UNIQUE (TimesheetId, WorkDate)` trên bảng `TimesheetEntry` — không thể chấm công 2 lần trong cùng một ngày cho 1 bảng công.
3. `UQ_Period_Month_Year`: Ràng buộc `UNIQUE (Month, Year)` trên bảng `PayrollPeriod` — không thể tạo trùng lặp kỳ lương.
4. `UQ_Period_Employee`: Ràng buộc `UNIQUE (PeriodId, EmployeeId)` trên bảng `Payslip` — trong 1 kỳ lương mỗi nhân viên chỉ có đúng 1 phiếu lương.
5. Cặp Header - Line:
   * `Payslip` $\leftrightarrow$ `PayslipLine` (bắt buộc lưu trong **1 DB Transaction ACID**).
   * `Timesheet` $\leftrightarrow$ `TimesheetEntry`.

