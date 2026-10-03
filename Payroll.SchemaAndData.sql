-- =========================================================================================
-- ĐỒ ÁN CAPSTONE: CHẤM CÔNG & TÍNH LƯƠNG (TIMESHEET & PAYROLL)
-- Sinh viên: Nguyễn Viết Mẫn - MSSV: 23K4080026 - Trường ĐH Kinh tế, ĐH Huế
-- Hệ quản trị CSDL: Microsoft SQL Server 2022 / LocalDB / SQLEXPRESS
-- Chuẩn hóa: 3NF, Khóa ngoại Foreign Keys, Ràng buộc toàn vẹn ACID
-- =========================================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PayrollDB')
BEGIN
    CREATE DATABASE PayrollDB;
END
GO

USE PayrollDB;
GO

-- 1. XÓA BẢNG CŨ THEO THỨ TỰ PHỤ THUỘC KHÓA NGOẠI
IF OBJECT_ID('dbo.PayslipLine', 'U') IS NOT NULL DROP TABLE dbo.PayslipLine;
IF OBJECT_ID('dbo.Payslip', 'U') IS NOT NULL DROP TABLE dbo.Payslip;
IF OBJECT_ID('dbo.PayrollPeriod', 'U') IS NOT NULL DROP TABLE dbo.PayrollPeriod;
IF OBJECT_ID('dbo.TimesheetEntry', 'U') IS NOT NULL DROP TABLE dbo.TimesheetEntry;
IF OBJECT_ID('dbo.Timesheet', 'U') IS NOT NULL DROP TABLE dbo.Timesheet;
IF OBJECT_ID('dbo.AppUser', 'U') IS NOT NULL DROP TABLE dbo.AppUser;
IF OBJECT_ID('dbo.Contract', 'U') IS NOT NULL DROP TABLE dbo.Contract;
IF OBJECT_ID('dbo.Dependent', 'U') IS NOT NULL DROP TABLE dbo.Dependent;
IF OBJECT_ID('dbo.Employee', 'U') IS NOT NULL DROP TABLE dbo.Employee;
IF OBJECT_ID('dbo.Department', 'U') IS NOT NULL DROP TABLE dbo.Department;
IF OBJECT_ID('dbo.PayrollSetting', 'U') IS NOT NULL DROP TABLE dbo.PayrollSetting;
IF OBJECT_ID('dbo.DeductionRate', 'U') IS NOT NULL DROP TABLE dbo.DeductionRate;
IF OBJECT_ID('dbo.TaxBracket', 'U') IS NOT NULL DROP TABLE dbo.TaxBracket;
GO

-- 2. TẠO CÁC BẢNG THEO CHUẨN 3NF

-- BẢNG PHÒNG BAN (Department)
CREATE TABLE dbo.Department (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- BẢNG HỒ SƠ NHÂN VIÊN (Employee)
CREATE TABLE dbo.Employee (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20) NOT NULL,
    IdentityNumber NVARCHAR(20) NOT NULL UNIQUE,
    DepartmentId INT NOT NULL,
    HireDate DATE NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Employee_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.Department(DepartmentId)
);

-- BẢNG NGƯỜI PHỤ THUỘC (Dependent)
CREATE TABLE dbo.Dependent (
    DependentId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Relationship NVARCHAR(50) NOT NULL,
    DateOfBirth DATE NOT NULL,
    TaxCode NVARCHAR(20) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Dependent_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId)
);

-- BẢNG HỢP ĐỒNG LAO ĐỘNG (Contract)
CREATE TABLE dbo.Contract (
    ContractId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    ContractNumber NVARCHAR(50) NOT NULL UNIQUE,
    ContractType NVARCHAR(100) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    BaseSalary DECIMAL(18,2) NOT NULL,
    Allowance DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Contract_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId)
);

-- BẢNG TÀI KHOẢN ĐĂNG NHẬP (AppUser)
-- Role: 1 = Employee, 2 = Accountant
CREATE TABLE dbo.AppUser (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role INT NOT NULL,
    EmployeeId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_AppUser_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId)
);

-- BẢNG BẢNG CÔNG HEADER (Timesheet)
-- Status: 1 = Draft, 2 = Submitted, 3 = Approved
CREATE TABLE dbo.Timesheet (
    TimesheetId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Month INT NOT NULL,
    Year INT NOT NULL,
    Status INT NOT NULL DEFAULT 1,
    SubmittedAt DATETIME NULL,
    ApprovedAt DATETIME NULL,
    ApprovedByUserId INT NULL,
    CONSTRAINT FK_Timesheet_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId),
    CONSTRAINT FK_Timesheet_ApprovedBy FOREIGN KEY (ApprovedByUserId) REFERENCES dbo.AppUser(UserId),
    CONSTRAINT UQ_Employee_Month_Year UNIQUE (EmployeeId, Month, Year)
);

-- BẢNG CHI TIẾT CHẤM CÔNG LINE (TimesheetEntry)
-- DayStatus: 1 = Working, 2 = PaidLeave, 3 = UnpaidLeave, 4 = Holiday
CREATE TABLE dbo.TimesheetEntry (
    EntryId INT IDENTITY(1,1) PRIMARY KEY,
    TimesheetId INT NOT NULL,
    WorkDate DATE NOT NULL,
    DayStatus INT NOT NULL DEFAULT 1,
    CheckInTime DATETIME NULL,
    CheckOutTime DATETIME NULL,
    WorkingHours DECIMAL(4,2) NULL,
    Note NVARCHAR(250) NULL,
    CONSTRAINT FK_TimesheetEntry_Timesheet FOREIGN KEY (TimesheetId) REFERENCES dbo.Timesheet(TimesheetId) ON DELETE CASCADE,
    CONSTRAINT UQ_Timesheet_WorkDate UNIQUE (TimesheetId, WorkDate)
);

-- BẢNG THIẾT LẬP LƯƠNG & GIẢM TRỪ (PayrollSetting)
CREATE TABLE dbo.PayrollSetting (
    SettingId INT IDENTITY(1,1) PRIMARY KEY,
    InsuranceCeiling DECIMAL(18,2) NOT NULL,
    PersonalDeduction DECIMAL(18,2) NOT NULL,
    DependentDeduction DECIMAL(18,2) NOT NULL,
    EffectiveFrom DATE NOT NULL,
    SourceNote NVARCHAR(250) NOT NULL
);

-- BẢNG TỶ LỆ ĐÓNG BẢO HIỂM (DeductionRate)
CREATE TABLE dbo.DeductionRate (
    RateId INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(150) NOT NULL,
    Rate DECIMAL(6,4) NOT NULL,
    EffectiveFrom DATE NOT NULL,
    SourceNote NVARCHAR(250) NOT NULL
);

-- BẢNG BIỂU THUẾ LŨY TIẾN TỪNG PHẦN (TaxBracket)
CREATE TABLE dbo.TaxBracket (
    BracketId INT IDENTITY(1,1) PRIMARY KEY,
    BracketOrder INT NOT NULL,
    ThresholdMin DECIMAL(18,2) NOT NULL,
    ThresholdMax DECIMAL(18,2) NULL,
    TaxRate DECIMAL(6,4) NOT NULL,
    EffectiveFrom DATE NOT NULL,
    SourceNote NVARCHAR(250) NOT NULL
);

-- BẢNG KỲ LƯƠNG (PayrollPeriod)
-- Status: 1 = Open, 2 = Calculated, 3 = Closed
CREATE TABLE dbo.PayrollPeriod (
    PeriodId INT IDENTITY(1,1) PRIMARY KEY,
    Month INT NOT NULL,
    Year INT NOT NULL,
    StandardWorkingDays DECIMAL(5,2) NOT NULL DEFAULT 22,
    Status INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ClosedAt DATETIME NULL,
    CONSTRAINT UQ_Period_Month_Year UNIQUE (Month, Year)
);

-- BẢNG PHIẾU LƯƠNG HEADER (Payslip)
CREATE TABLE dbo.Payslip (
    PayslipId INT IDENTITY(1,1) PRIMARY KEY,
    PeriodId INT NOT NULL,
    EmployeeId INT NOT NULL,
    BaseSalarySnapshot DECIMAL(18,2) NOT NULL,
    StandardDaysSnapshot DECIMAL(5,2) NOT NULL,
    ActualDaysSnapshot DECIMAL(5,2) NOT NULL,
    AllowanceSnapshot DECIMAL(18,2) NOT NULL,
    GrossSalary DECIMAL(18,2) NOT NULL,
    TotalInsuranceDeduction DECIMAL(18,2) NOT NULL,
    PersonalDeductionSnapshot DECIMAL(18,2) NOT NULL,
    DependentDeductionSnapshot DECIMAL(18,2) NOT NULL,
    DependentCountSnapshot INT NOT NULL,
    TaxableIncome DECIMAL(18,2) NOT NULL,
    PersonalIncomeTax DECIMAL(18,2) NOT NULL,
    NetSalary DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Payslip_Period FOREIGN KEY (PeriodId) REFERENCES dbo.PayrollPeriod(PeriodId),
    CONSTRAINT FK_Payslip_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId),
    CONSTRAINT UQ_Period_Employee UNIQUE (PeriodId, EmployeeId)
);

-- BẢNG CHI TIẾT PHIẾU LƯƠNG LINE (PayslipLine)
-- LineType: 1 = Earning, 2 = Insurance, 3 = TaxDeduction, 4 = Tax, 5 = OtherDeduction
CREATE TABLE dbo.PayslipLine (
    LineId INT IDENTITY(1,1) PRIMARY KEY,
    PayslipId INT NOT NULL,
    LineType INT NOT NULL,
    ItemCode NVARCHAR(50) NOT NULL,
    ItemName NVARCHAR(150) NOT NULL,
    RateOrThresholdSnapshot DECIMAL(18,4) NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Note NVARCHAR(250) NULL,
    CONSTRAINT FK_PayslipLine_Payslip FOREIGN KEY (PayslipId) REFERENCES dbo.Payslip(PayslipId) ON DELETE CASCADE
);
GO

-- =========================================================================================
-- 3. NẠP DỮ LIỆU CẤU HÌNH & DỮ LIỆU MẪU
-- =========================================================================================

-- Cấu hình giảm trừ & trần bảo hiểm
-- NGUỒN XÁC MINH RÕ RÀNG:
-- Trần 50.600.000 đ = 20 lần mức tham chiếu 2.530.000 đ/tháng (theo Luật BHXH 2024 số 41/2024/QH15 áp dụng từ 01/7/2026).
-- (Lưu ý: Nếu đối chiếu theo Nghị định 73/2024/NĐ-CP với mức lương cơ sở 2.340.000 đ thì trần là 46.800.000 đ, có thể chỉnh trong giao diện Cấu hình).
INSERT INTO dbo.PayrollSetting (InsuranceCeiling, PersonalDeduction, DependentDeduction, EffectiveFrom, SourceNote) VALUES
(50600000.00, 15500000.00, 6200000.00, '2026-07-01', N'Trần BHXH 20 x 2.530.000 đ = 50.600.000 đ (Luật BHXH 2024). Giảm trừ gia cảnh 15.5tr/6.2tr');

-- Tỷ lệ đóng bảo hiểm người lao động (BHXH: 8%, BHYT: 1.5%, BHTN: 1%)
INSERT INTO dbo.DeductionRate (Code, Name, Rate, EffectiveFrom, SourceNote) VALUES
('BHXH', N'Bảo hiểm xã hội', 0.0800, '2026-01-01', N'Quy định Luật BHXH người lao động đóng 8%'),
('BHYT', N'Bảo hiểm y tế', 0.0150, '2026-01-01', N'Quy định Luật BHYT người lao động đóng 1.5%'),
('BHTN', N'Bảo hiểm thất nghiệp', 0.0100, '2026-01-01', N'Quy định Luật Việc làm người lao động đóng 1%');

-- Biểu thuế lũy tiến từng phần 5 bậc mẫu
INSERT INTO dbo.TaxBracket (BracketOrder, ThresholdMin, ThresholdMax, TaxRate, EffectiveFrom, SourceNote) VALUES
(1, 0.00, 5000000.00, 0.0500, '2026-01-01', N'Bậc 1: Thu nhập tính thuế đến 5 triệu đồng'),
(2, 5000000.00, 10000000.00, 0.1000, '2026-01-01', N'Bậc 2: Trên 5 triệu đến 10 triệu đồng'),
(3, 10000000.00, 18000000.00, 0.2000, '2026-01-01', N'Bậc 3: Trên 10 triệu đến 18 triệu đồng'),
(4, 18000000.00, 32000000.00, 0.3000, '2026-01-01', N'Bậc 4: Trên 18 triệu đến 32 triệu đồng'),
(5, 32000000.00, NULL, 0.3500, '2026-01-01', N'Bậc 5: Trên 32 triệu đồng');

-- Phòng ban mẫu (2 phòng ban chính)
INSERT INTO dbo.Department (DepartmentName, Description) VALUES
(N'Phòng Kế Toán & Nhân Sự', N'Quản lý tài chính, nhân sự, chấm công và chi trả lương doanh nghiệp'),
(N'Phòng Phát Triển Phần Mềm', N'Đội ngũ kỹ sư phần mềm, giải pháp công nghệ số');

-- Nhân viên mẫu (6 nhân sự đại diện đầy đủ các trường hợp tính thuế & BH)
INSERT INTO dbo.Employee (FullName, Email, PhoneNumber, IdentityNumber, DepartmentId, HireDate, IsActive) VALUES
(N'Nguyễn Văn An', 'an.nguyen@smartpayroll.vn', '0905123456', '046098001122', 2, '2024-01-15', 1),
(N'Trần Thị Bình', 'binh.tran@smartpayroll.vn', '0905234567', '046198002233', 1, '2023-05-10', 1),
(N'Lê Hoàng Cường', 'cuong.le@smartpayroll.vn', '0905345678', '046088003344', 2, '2022-03-01', 1),
(N'Phạm Minh Đức', 'duc.pham@smartpayroll.vn', '0905456789', '046092004455', 2, '2024-06-01', 1),
(N'Hoàng Thu Hương', 'huong.hoang@smartpayroll.vn', '0905567890', '046195005566', 1, '2024-02-20', 1),
(N'Vũ Tuấn Hùng', 'hung.vu@smartpayroll.vn', '0905678901', '046099006677', 2, '2024-08-01', 1);

-- Người phụ thuộc (Bình có 2 con, Cường có 1 con, An 0 con)
INSERT INTO dbo.Dependent (EmployeeId, FullName, Relationship, DateOfBirth, TaxCode, IsActive) VALUES
(2, N'Trần Gia Hân', N'Con ruột', '2018-05-12', '8401234567', 1),
(2, N'Trần Bảo Nam', N'Con ruột', '2021-09-20', '8401234568', 1),
(3, N'Lê Tuấn Khang', N'Con ruột', '2019-11-05', '8401234569', 1);

-- Hợp đồng lao động
-- An: 18tr (phụ cấp 2tr) | Bình: 22tr (phụ cấp 1.5tr) | Cường: 65tr (VƯỢT TRẦN BH, phụ cấp 5tr)
-- Đức: 16tr (phụ cấp 1tr) | Hương: 15tr (phụ cấp 1tr) | Hùng: 14tr (phụ cấp 1tr)
INSERT INTO dbo.Contract (EmployeeId, ContractNumber, ContractType, StartDate, EndDate, BaseSalary, Allowance, IsActive) VALUES
(1, 'HDLD-2024-001', N'Hợp đồng không xác định thời hạn', '2024-01-15', NULL, 18000000.00, 2000000.00, 1),
(2, 'HDLD-2023-012', N'Hợp đồng không xác định thời hạn', '2023-05-10', NULL, 22000000.00, 1500000.00, 1),
(3, 'HDLD-2022-005', N'Hợp đồng không xác định thời hạn', '2022-03-01', NULL, 65000000.00, 5000000.00, 1),
(4, 'HDLD-2024-034', N'Hợp đồng 24 tháng', '2024-06-01', '2026-05-31', 16000000.00, 1000000.00, 1),
(5, 'HDLD-2024-018', N'Hợp đồng 12 tháng', '2024-02-20', '2025-02-19', 15000000.00, 1000000.00, 1),
(6, 'HDLD-2024-055', N'Hợp đồng thử việc / 12 tháng', '2024-08-01', '2025-07-31', 14000000.00, 1000000.00, 1);

-- Tài khoản người dùng (Mật khẩu băm chuẩn PBKDF2 100.000 vòng lặp có Cryptographic Salt)
-- Kế toán (admin): accountant / admin123
-- Nhân viên: emp_an, emp_binh, emp_cuong, emp_duc, emp_huong, emp_hung / 123
INSERT INTO dbo.AppUser (Username, PasswordHash, Role, EmployeeId, IsActive) VALUES
('accountant', '07S7ydANBCw0WLwdafR4ecG8d0MaPvle6fWLx7ugaOXTQQ1OeqY0zahNZDUIAVHh', 2, NULL, 1),
('emp_an',     '65rNZ0c4nTxAmrEs9Islb7dJo5BjRVn1KwZKMIXYlz5QQl2/tXlGPGuCcdMuAfOz', 1, 1, 1),
('emp_binh',   'D+Fpczb0YtxiBf9E0Tn1DLDGTP1aKxpQkJMCd5h2/WGFlm//ZDFv9o6v90jBKsXA', 1, 2, 1),
('emp_cuong',  'B3PIthUTHka4IKsNu/5WFW3FNf4jRqBKL2U4tu7jsKVSZDXV1glaUoFw71s+3eyu', 1, 3, 1),
('emp_duc',    '7aKg1tTZ1navj62tOipHVKWoeV0jo/0pb4jbChprxuP+Q3NI+Gu8lOXrI2Rn45o2', 1, 4, 1),
('emp_huong',  'c0KqQo9yz2mAOdGgFw04+qwt8vjoWs9/iedfxSpfV2E+U8nABXqXi/MtgycHpFMu', 1, 5, 1),
('emp_hung',   '3xOY8m5LV0ToS2ZH9/zfk7oZ+GTOjVUSX0ktn8tTzqqje08lRxEfjwiwxXSQZO0S', 1, 6, 1);

-- =========================================================================================
-- KỲ LƯƠNG & CHẤM CÔNG MẪU (CHUẨN HOÁ THỰC TẾ: THÁNG 9 TRỌN VẸN & THÁNG 10 ĐANG DIỄN RA)
-- =========================================================================================

-- Kỳ lương Tháng 9/2026: Kỳ chuẩn đã kết thúc để demo tính lương, chốt kỳ và xuất Excel
INSERT INTO dbo.PayrollPeriod (Month, Year, StandardWorkingDays, Status, CreatedAt) VALUES
(9, 2026, 22.0, 1, '2026-09-01 08:00:00');

-- Kỳ lương Tháng 10/2026: Kỳ hiện tại đang mở
INSERT INTO dbo.PayrollPeriod (Month, Year, StandardWorkingDays, Status, CreatedAt) VALUES
(10, 2026, 22.0, 1, '2026-10-01 08:00:00');

-- Bảng công Tháng 9/2026: Đầy đủ 30 ngày (đã duyệt để tính lương)
INSERT INTO dbo.Timesheet (EmployeeId, Month, Year, Status, SubmittedAt, ApprovedAt, ApprovedByUserId) VALUES
(1, 9, 2026, 3, '2026-09-30 17:00:00', '2026-09-30 17:30:00', 1),
(2, 9, 2026, 3, '2026-09-30 17:05:00', '2026-09-30 17:30:00', 1),
(3, 9, 2026, 3, '2026-09-30 17:10:00', '2026-09-30 17:30:00', 1),
(4, 9, 2026, 3, '2026-09-30 17:15:00', '2026-09-30 17:30:00', 1),
(5, 9, 2026, 2, '2026-09-30 17:20:00', NULL, NULL),
(6, 9, 2026, 1, NULL, NULL, NULL);

DECLARE @ts1_Sep INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 1 AND Month = 9 AND Year = 2026);
DECLARE @ts2_Sep INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 2 AND Month = 9 AND Year = 2026);
DECLARE @ts3_Sep INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 3 AND Month = 9 AND Year = 2026);
DECLARE @ts4_Sep INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 4 AND Month = 9 AND Year = 2026);
DECLARE @ts5_Sep INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 5 AND Month = 9 AND Year = 2026);
DECLARE @ts6_Sep INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 6 AND Month = 9 AND Year = 2026);

DECLARE @dSep INT = 1;
WHILE @dSep <= 30
BEGIN
    DECLARE @dateSep DATE = DATEFROMPARTS(2026, 9, @dSep);
    IF DATENAME(WEEKDAY, @dateSep) NOT IN ('Saturday', 'Sunday', 'Thứ Bảy', 'Chủ Nhật')
    BEGIN
        IF @dSep = 2
        BEGIN
            INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note)
            VALUES 
            (@ts1_Sep, @dateSep, 4, NULL, NULL, 8.0, N'Nghỉ lễ Quốc Khánh 2/9'),
            (@ts2_Sep, @dateSep, 4, NULL, NULL, 8.0, N'Nghỉ lễ Quốc Khánh 2/9'),
            (@ts3_Sep, @dateSep, 4, NULL, NULL, 8.0, N'Nghỉ lễ Quốc Khánh 2/9'),
            (@ts4_Sep, @dateSep, 4, NULL, NULL, 8.0, N'Nghỉ lễ Quốc Khánh 2/9'),
            (@ts5_Sep, @dateSep, 4, NULL, NULL, 8.0, N'Nghỉ lễ Quốc Khánh 2/9'),
            (@ts6_Sep, @dateSep, 4, NULL, NULL, 8.0, N'Nghỉ lễ Quốc Khánh 2/9');
        END
        ELSE
        BEGIN
            INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note)
            VALUES (@ts1_Sep, @dateSep, 1, DATEADD(hour, 8, CAST(@dateSep AS DATETIME)), DATEADD(hour, 17, CAST(@dateSep AS DATETIME)), 8.0, N'Đi làm đủ 8h');

            INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note)
            VALUES (@ts2_Sep, @dateSep, 
                CASE WHEN @dSep = 15 THEN 2 ELSE 1 END,
                CASE WHEN @dSep = 15 THEN NULL ELSE DATEADD(hour, 8, CAST(@dateSep AS DATETIME)) END,
                CASE WHEN @dSep = 15 THEN NULL ELSE DATEADD(hour, 17, CAST(@dateSep AS DATETIME)) END,
                8.0,
                CASE WHEN @dSep = 15 THEN N'Nghỉ phép thường niên có lương' ELSE N'Đi làm đủ 8h' END);

            INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note)
            VALUES (@ts3_Sep, @dateSep, 1, DATEADD(hour, 8, CAST(@dateSep AS DATETIME)), DATEADD(hour, 17, CAST(@dateSep AS DATETIME)), 8.0, N'Đi làm đủ 8h');

            INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note)
            VALUES (@ts4_Sep, @dateSep,
                CASE WHEN @dSep IN (21, 22) THEN 3 ELSE 1 END,
                CASE WHEN @dSep IN (21, 22) THEN NULL ELSE DATEADD(hour, 8, CAST(@dateSep AS DATETIME)) END,
                CASE WHEN @dSep IN (21, 22) THEN NULL ELSE DATEADD(hour, 17, CAST(@dateSep AS DATETIME)) END,
                CASE WHEN @dSep IN (21, 22) THEN 0.0 ELSE 8.0 END,
                CASE WHEN @dSep IN (21, 22) THEN N'Nghỉ việc riêng không hưởng lương' ELSE N'Đi làm đủ 8h' END);

            INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note)
            VALUES 
            (@ts5_Sep, @dateSep, 1, DATEADD(hour, 8, CAST(@dateSep AS DATETIME)), DATEADD(hour, 17, CAST(@dateSep AS DATETIME)), 8.0, N'Đi làm đủ 8h'),
            (@ts6_Sep, @dateSep, 1, DATEADD(hour, 8, CAST(@dateSep AS DATETIME)), DATEADD(hour, 17, CAST(@dateSep AS DATETIME)), 8.0, N'Đi làm đủ 8h');
        END
    END
    SET @dSep = @dSep + 1;
END;

-- Bảng công Tháng 10/2026: Chỉ có ngày 01 và 02/10 (Tháng đang diễn ra)
INSERT INTO dbo.Timesheet (EmployeeId, Month, Year, Status) VALUES
(1, 10, 2026, 1),
(2, 10, 2026, 1),
(3, 10, 2026, 1),
(4, 10, 2026, 1),
(5, 10, 2026, 1),
(6, 10, 2026, 1);

DECLARE @ts1_Oct INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 1 AND Month = 10 AND Year = 2026);
DECLARE @ts2_Oct INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 2 AND Month = 10 AND Year = 2026);
DECLARE @ts3_Oct INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 3 AND Month = 10 AND Year = 2026);
DECLARE @ts4_Oct INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 4 AND Month = 10 AND Year = 2026);
DECLARE @ts5_Oct INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 5 AND Month = 10 AND Year = 2026);
DECLARE @ts6_Oct INT = (SELECT TimesheetId FROM dbo.Timesheet WHERE EmployeeId = 6 AND Month = 10 AND Year = 2026);

-- Ngày 01/10/2026 (Thứ Năm)
INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note) VALUES
(@ts1_Oct, '2026-10-01', 1, '2026-10-01 08:00:00', '2026-10-01 17:05:00', 8.0, N'Đi làm đủ ngày'),
(@ts2_Oct, '2026-10-01', 1, '2026-10-01 08:02:00', '2026-10-01 17:00:00', 8.0, N'Đi làm đủ ngày'),
(@ts3_Oct, '2026-10-01', 1, '2026-10-01 07:55:00', '2026-10-01 17:15:00', 8.0, N'Đi làm đủ ngày'),
(@ts4_Oct, '2026-10-01', 1, '2026-10-01 08:00:00', '2026-10-01 17:00:00', 8.0, N'Đi làm đủ ngày'),
(@ts5_Oct, '2026-10-01', 1, '2026-10-01 08:10:00', '2026-10-01 17:00:00', 8.0, N'Đi làm đủ ngày'),
(@ts6_Oct, '2026-10-01', 1, '2026-10-01 08:00:00', '2026-10-01 17:00:00', 8.0, N'Đi làm đủ ngày');

-- Ngày 02/10/2026 (Thứ Sáu)
INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, CheckInTime, CheckOutTime, WorkingHours, Note) VALUES
(@ts1_Oct, '2026-10-02', 1, '2026-10-02 08:00:00', '2026-10-02 17:00:00', 8.0, N'Đi làm đủ ngày'),
(@ts2_Oct, '2026-10-02', 1, '2026-10-02 08:05:00', '2026-10-02 17:00:00', 8.0, N'Đi làm đủ ngày'),
(@ts3_Oct, '2026-10-02', 1, '2026-10-02 08:00:00', '2026-10-02 17:10:00', 8.0, N'Đi làm đủ ngày'),
(@ts4_Oct, '2026-10-02', 1, '2026-10-02 08:00:00', '2026-10-02 17:00:00', 8.0, N'Đi làm đủ ngày'),
(@ts5_Oct, '2026-10-02', 1, '2026-10-02 08:15:00', '2026-10-02 17:00:00', 8.0, N'Đi làm đủ ngày'),
(@ts6_Oct, '2026-10-02', 1, '2026-10-02 08:00:00', '2026-10-02 17:00:00', 8.0, N'Đi làm đủ ngày');

-- Ngày 03/10/2026 (Hôm nay): KHÔNG SEED TRƯỚC để nhân viên tự bấm Chấm Công Vào / Ra trên giao diện Web!
-- Ngày 04/10/2026 đến 31/10/2026: Chưa phát sinh trong tương lai.
GO