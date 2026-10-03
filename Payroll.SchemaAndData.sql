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

-- 1. BẢNG PHÒNG BAN (Department)
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

CREATE TABLE dbo.Department (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- 2. BẢNG NHÂN VIÊN (Employee)
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

-- 3. BẢNG NGƯỜI PHỤ THUỘC (Dependent)
CREATE TABLE dbo.Dependent (
    DependentId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Relationship NVARCHAR(50) NOT NULL,
    DateOfBirth DATE NOT NULL,
    IdentityNumber NVARCHAR(20) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Dependent_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId)
);

-- 4. BẢNG HỢP ĐỒNG LAO ĐỘNG (Contract)
CREATE TABLE dbo.Contract (
    ContractId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    ContractNumber NVARCHAR(50) NOT NULL UNIQUE,
    BaseSalary DECIMAL(18,2) NOT NULL,
    Allowance DECIMAL(18,2) NOT NULL DEFAULT 0,
    EffectiveFrom DATE NOT NULL,
    EffectiveTo DATE NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Contract_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId)
);

-- 5. BẢNG TÀI KHOẢN NGƯỜI DÙNG (AppUser)
-- Role: 1 = Employee, 2 = Accountant
CREATE TABLE dbo.AppUser (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role INT NOT NULL,
    EmployeeId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AppUser_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId)
);

-- 6. BẢNG BẢNG CÔNG HEADER (Timesheet)
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

-- 7. BẢNG CHI TIẾT CHẤM CÔNG LINE (TimesheetEntry)
-- DayStatus: 1 = Working, 2 = PaidLeave, 3 = UnpaidLeave, 4 = Holiday
CREATE TABLE dbo.TimesheetEntry (
    EntryId INT IDENTITY(1,1) PRIMARY KEY,
    TimesheetId INT NOT NULL,
    WorkDate DATE NOT NULL,
    DayStatus INT NOT NULL DEFAULT 1,
    Note NVARCHAR(250) NULL,
    CONSTRAINT FK_TimesheetEntry_Timesheet FOREIGN KEY (TimesheetId) REFERENCES dbo.Timesheet(TimesheetId) ON DELETE CASCADE,
    CONSTRAINT UQ_Timesheet_WorkDate UNIQUE (TimesheetId, WorkDate)
);

-- 8. BẢNG THIẾT LẬP LƯƠNG & GIẢM TRỪ (PayrollSetting)
CREATE TABLE dbo.PayrollSetting (
    SettingId INT IDENTITY(1,1) PRIMARY KEY,
    InsuranceCeiling DECIMAL(18,2) NOT NULL,
    PersonalDeduction DECIMAL(18,2) NOT NULL,
    DependentDeduction DECIMAL(18,2) NOT NULL,
    EffectiveFrom DATE NOT NULL,
    SourceNote NVARCHAR(250) NOT NULL
);

-- 9. BẢNG TỶ LỆ ĐÓNG BẢO HIỂM (DeductionRate)
CREATE TABLE dbo.DeductionRate (
    RateId INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    Rate DECIMAL(8,4) NOT NULL,
    EffectiveFrom DATE NOT NULL,
    SourceNote NVARCHAR(250) NOT NULL
);

-- 10. BẢNG BIỂU THUẾ LŨY TIẾN TỪNG PHẦN (TaxBracket)
CREATE TABLE dbo.TaxBracket (
    BracketId INT IDENTITY(1,1) PRIMARY KEY,
    BracketOrder INT NOT NULL UNIQUE,
    ThresholdMin DECIMAL(18,2) NOT NULL,
    ThresholdMax DECIMAL(18,2) NULL,
    TaxRate DECIMAL(8,4) NOT NULL,
    SourceNote NVARCHAR(250) NOT NULL
);

-- 11. BẢNG KỲ LƯƠNG (PayrollPeriod)
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

-- 12. BẢNG PHIẾU LƯƠNG HEADER (Payslip)
CREATE TABLE dbo.Payslip (
    PayslipId INT IDENTITY(1,1) PRIMARY KEY,
    PeriodId INT NOT NULL,
    EmployeeId INT NOT NULL,
    BaseSalarySnapshot DECIMAL(18,2) NOT NULL,
    StandardDaysSnapshot DECIMAL(5,2) NOT NULL,
    ActualDaysSnapshot DECIMAL(5,2) NOT NULL,
    WorkingSalary DECIMAL(18,2) NOT NULL,
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

-- 13. BẢNG CHI TIẾT PHIẾU LƯƠNG LINE (PayslipLine)
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
-- DỮ LIỆU MẪU KHỞI TẠO (SEED DATA)
-- =========================================================================================

-- Cấu hình trần lương & mức giảm trừ
INSERT INTO dbo.PayrollSetting (InsuranceCeiling, PersonalDeduction, DependentDeduction, EffectiveFrom, SourceNote)
VALUES (50600000, 15500000, 6200000, '2026-01-01', N'GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC');

-- Tỷ lệ đóng bảo hiểm (Người lao động)
INSERT INTO dbo.DeductionRate (Code, Name, Rate, EffectiveFrom, SourceNote) VALUES
('BHXH', N'Bảo hiểm xã hội', 0.0800, '2026-01-01', N'GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC'),
('BHYT', N'Bảo hiểm y tế', 0.0150, '2026-01-01', N'GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC'),
('BHTN', N'Bảo hiểm thất nghiệp', 0.0100, '2026-01-01', N'GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC');

-- Biểu thuế lũy tiến từng phần (5 bậc mẫu)
INSERT INTO dbo.TaxBracket (BracketOrder, ThresholdMin, ThresholdMax, TaxRate, SourceNote) VALUES
(1, 0, 5000000, 0.0500, N'GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC'),
(2, 5000000, 10000000, 0.1000, N'GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC'),
(3, 10000000, 18000000, 0.2000, N'GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC'),
(4, 18000000, 32000000, 0.3000, N'GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC'),
(5, 32000000, NULL, 0.3500, N'GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC');

-- 2 Phòng ban
INSERT INTO dbo.Department (DepartmentName, Description) VALUES
(N'Phòng Kỹ thuật & Công nghệ', N'Phát triển phần mềm, vận hành hệ thống IT'),
(N'Phòng Kế toán & Hành chính Nhân sự', N'Quản lý nhân sự, tiền lương và tài chính');

-- 6 Nhân viên mẫu (Bao quát 3 trường hợp: Lương cao vượt trần bảo hiểm, có người phụ thuộc, độc thân)
INSERT INTO dbo.Employee (FullName, Email, PhoneNumber, IdentityNumber, DepartmentId, HireDate, IsActive) VALUES
(N'Nguyễn Văn An', 'an.nguyen@company.com', '0901234567', '046098001111', 1, '2024-01-15', 1), -- Độc thân, lương 18tr
(N'Trần Thị Bình', 'binh.tran@company.com', '0902345678', '046098002222', 2, '2023-05-10', 1), -- 2 người phụ thuộc, lương 22tr
(N'Lê Hoàng Cường', 'cuong.le@company.com', '0903456789', '046098003333', 1, '2022-03-01', 1), -- LƯƠNG VƯỢT TRẦN 65tr, 1 người phụ thuộc
(N'Phạm Minh Đức', 'duc.pham@company.com', '0904567890', '046098004444', 1, '2024-06-01', 1), -- Độc thân, lương 14tr
(N'Hoàng Lan Hương', 'huong.hoang@company.com', '0905678901', '046098005555', 2, '2023-11-20', 1), -- 1 người phụ thuộc, lương 12tr
(N'Vũ Đức Hùng', 'hung.vu@company.com', '0906789012', '046098006666', 1, '2024-02-15', 1); -- Độc thân, lương 16tr

-- Người phụ thuộc
INSERT INTO dbo.Dependent (EmployeeId, FullName, Relationship, DateOfBirth, IdentityNumber, IsActive) VALUES
(2, N'Trần Gia Hân', N'Con', '2018-06-15', NULL, 1),
(2, N'Trần Bảo Nam', N'Con', '2021-09-20', NULL, 1),
(3, N'Lê Tuấn Khang', N'Con', '2016-12-05', NULL, 1),
(5, N'Nguyễn Thị Mai', N'Mẹ ruột', '1958-04-10', '046158009999', 1);

-- Hợp đồng lao động active
INSERT INTO dbo.Contract (EmployeeId, ContractNumber, BaseSalary, Allowance, EffectiveFrom, EffectiveTo, IsActive) VALUES
(1, 'HDLD-2024-001', 18000000, 2000000, '2024-01-15', NULL, 1),
(2, 'HDLD-2023-012', 22000000, 1500000, '2023-05-10', NULL, 1),
(3, 'HDLD-2022-005', 65000000, 5000000, '2022-03-01', NULL, 1),
(4, 'HDLD-2024-045', 14000000, 1000000, '2024-06-01', NULL, 1),
(5, 'HDLD-2023-088', 12000000, 1000000, '2023-11-20', NULL, 1),
(6, 'HDLD-2024-032', 16000000, 1000000, '2024-02-15', NULL, 1);

-- Tài khoản người dùng (Mật khẩu băm SHA256 chuẩn cho: admin123 và 123)
-- SHA256('admin123') = 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
-- SHA256('123')      = a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3
INSERT INTO dbo.AppUser (Username, PasswordHash, Role, EmployeeId, IsActive) VALUES
('accountant', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 2, NULL, 1), -- Kế toán (Admin)
('emp_an',     'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 1, 1, 1),
('emp_binh',   'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 1, 2, 1),
('emp_cuong',  'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 1, 3, 1),
('emp_duc',    'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 1, 4, 1),
('emp_huong',  'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 1, 5, 1),
('emp_hung',   'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 1, 6, 1);

-- Kỳ lương mẫu: Tháng 10/2026 (22 ngày công chuẩn)
INSERT INTO dbo.PayrollPeriod (Month, Year, StandardWorkingDays, Status, CreatedAt) VALUES
(10, 2026, 22.0, 1, GETDATE());

-- Bảng công mẫu tháng 10/2026 cho 6 nhân viên
-- 1: An (Đã duyệt - 22 ngày công), 2: Bình (Đã duyệt - 21 ngày công, 1 ngày phép có lương = 22 công), 3: Cường (Đã duyệt - 22 công)
-- 4: Đức (Đã duyệt - 20 ngày công, 2 ngày không lương = 20 công), 5: Hương (Đã gửi - chờ duyệt), 6: Hùng (Nháp)
INSERT INTO dbo.Timesheet (EmployeeId, Month, Year, Status, SubmittedAt, ApprovedAt, ApprovedByUserId) VALUES
(1, 10, 2026, 3, '2026-10-31 17:00:00', '2026-10-31 18:00:00', 1),
(2, 10, 2026, 3, '2026-10-31 17:05:00', '2026-10-31 18:00:00', 1),
(3, 10, 2026, 3, '2026-10-31 17:10:00', '2026-10-31 18:00:00', 1),
(4, 10, 2026, 3, '2026-10-31 17:15:00', '2026-10-31 18:00:00', 1),
(5, 10, 2026, 2, '2026-10-31 17:20:00', NULL, NULL),
(6, 10, 2026, 1, NULL, NULL, NULL);

-- Chi tiết ngày công mẫu (Ví dụ sinh ngày công cho An - TimesheetId = 1)
DECLARE @d INT = 1;
WHILE @d <= 31
BEGIN
    DECLARE @curDate DATE = DATEFROMPARTS(2026, 10, @d);
    DECLARE @dayOfWeek INT = DATEPART(WEEKDAY, @curDate); -- 1 = Chủ nhật, 7 = Thứ 7 (tuỳ cấu hình)
    
    -- Nếu không phải thứ 7, chủ nhật (ở VN DATEFIRST thường là 7 hoặc theo hệ thống)
    IF DATENAME(WEEKDAY, @curDate) NOT IN ('Saturday', 'Sunday', 'Thứ Bảy', 'Chủ Nhật')
    BEGIN
        INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, Note)
        VALUES (1, @curDate, 1, N'Đi làm đầy đủ 8h');

        -- Nhân viên 2: Bình có 1 ngày nghỉ phép có lương (ngày 15)
        INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, Note)
        VALUES (2, @curDate, CASE WHEN @d = 15 THEN 2 ELSE 1 END, CASE WHEN @d = 15 THEN N'Nghỉ phép thường niên' ELSE N'Đi làm' END);

        -- Nhân viên 3: Cường đi làm đầy đủ
        INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, Note)
        VALUES (3, @curDate, 1, N'Đi làm');

        -- Nhân viên 4: Đức nghỉ không lương 2 ngày (ngày 20, 21)
        INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, Note)
        VALUES (4, @curDate, CASE WHEN @d IN (20, 21) THEN 3 ELSE 1 END, CASE WHEN @d IN (20, 21) THEN N'Nghỉ việc riêng không lương' ELSE N'Đi làm' END);

        -- Nhân viên 5: Hương
        INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, Note)
        VALUES (5, @curDate, 1, N'Đi làm');

        -- Nhân viên 6: Hùng
        INSERT INTO dbo.TimesheetEntry (TimesheetId, WorkDate, DayStatus, Note)
        VALUES (6, @curDate, 1, N'Đi làm');
    END
    SET @d = @d + 1;
END;
GO
