# BẢNG MINH CHỨNG ĐÁNH GIÁ ĐỒ ÁN CAPSTONE (GRADING.MD)
**Đề tài:** Chấm công & Tính lương (Timesheet & Payroll)  
**Sinh viên:** Nguyễn Viết Mẫn (MSSV: 23K4080026) - Trường ĐH Kinh tế, ĐH Huế  

---

| Mã Tiêu Chí | Nội Dung Yêu Cầu | Đường Dẫn File Minh Chứng | Vị Trí / Bằng Chứng Cụ Thể |
| :--- | :--- | :--- | :--- |
| **K1.1** | Kiến trúc đúng 4 dự án, không thêm project thứ 5, hướng phụ thuộc 1 chiều Clean Architecture | `PayrollSystem.sln` | Toàn bộ solution gồm: `Payroll.CoreBusiness`, `Payroll.UseCases`, `Payroll.Plugins`, `Payroll.WebApp`. |
| **K1.2** | Mô hình dữ liệu chuẩn 3NF, tiền và tỷ lệ dùng `decimal` | `Payroll.SchemaAndData.sql`<br>`Payroll.CoreBusiness/Entities/` | 13 bảng 3NF: `Department`, `Employee`, `Contract`, `Dependent`, `PayrollPeriod`, `Payslip`, `PayslipLine`,... |
| **K1.3** | Cặp Header–Line bắt buộc (`Payslip` – `PayslipLine`, `Timesheet` – `TimesheetEntry`) | `Payroll.CoreBusiness/Entities/Payslip.cs`<br>`Payroll.CoreBusiness/Entities/PayslipLine.cs` | Dòng 1–45: `Payslip` chứa danh sách `List<PayslipLine> Lines`. Khóa ngoại liên kết 1-N. |
| **K1.4** | Máy trạng thái (Timesheet: Nháp $\to$ Gửi $\to$ Duyệt; Kỳ: Mở $\to$ Tính $\to$ Chốt) | `Payroll.CoreBusiness/DomainServices/TimesheetStateMachine.cs`<br>`Payroll.CoreBusiness/DomainServices/PeriodStateMachine.cs` | Dòng 8–46: `ValidateTransition` kiểm soát hợp lệ, ném `BusinessRuleException` khi chuyển sai. |
| **K2.1** | LUẬT 1: Công thức lương đọc từ cấu hình, không hardcode số liệu | `Payroll.CoreBusiness/DomainServices/PayrollCalculator.cs` | Dòng 26–175: Tính lương theo công, Gross, trừ BH, giảm trừ gia cảnh, Net. |
| **K2.2** | LUẬT 1: Tính bảo hiểm theo trần và thuế TNCN theo biểu lũy tiến từng phần | `Payroll.CoreBusiness/DomainServices/PayrollCalculator.cs` | Dòng 70–82: `Math.Min(BaseSalary, InsuranceCeiling)`<br>Dòng 115–158: Thuế TNCN lũy tiến theo `TaxBracket`. |
| **K2.3** | Snapshot dữ liệu lương tại thời điểm tính | `Payroll.CoreBusiness/Entities/PayslipLine.cs`<br>`Payroll.CoreBusiness/DomainServices/PayrollCalculator.cs` | Dòng 48–170: Tạo các dòng `PayslipLine` lưu tỷ lệ, ngưỡng và số tiền snapshot. |
| **K2.4** | LUẬT 2: Kỳ đã chốt không sửa & Phân quyền dữ liệu theo Claims tại UseCase | `Payroll.CoreBusiness/DomainServices/PeriodStateMachine.cs`<br>`Payroll.UseCases/Timesheets/ViewMyTimesheetUseCase.cs`<br>`Payroll.UseCases/Payrolls/ViewMyPayslipUseCase.cs` | Dòng 30–38: `EnsurePeriodNotClosed`<br>`ViewMyTimesheetUseCase` Dòng 24–28: Chặn nhân viên xem bảng công người khác<br>`ViewMyPayslipUseCase` Dòng 23–27: Chặn xem phiếu lương người khác. |
| **K3.1** | SQL Server Dapper 100% Parameterized queries (`@Param`), không nối chuỗi | `Payroll.Plugins/DataStore.Sql/EmployeeRepository.cs`<br>`Payroll.Plugins/DataStore.Sql/TimesheetRepository.cs` | Toàn bộ các câu lệnh SQL dùng tham số `@SearchTerm`, `@DepartmentId`, `@Month`,... |
| **K3.2** | Lưu Header–Line (`Payslip` – `PayslipLine`) trong 1 DB Transaction ACID | `Payroll.Plugins/DataStore.Sql/PayrollRepository.cs` | Dòng 165–228: `using var tx = conn.BeginTransaction()` lưu đồng thời `Payslip` và các dòng `PayslipLine`. |
| **K3.3** | Băm mật khẩu `IPasswordHasher` / `PasswordHasher` | `Payroll.Plugins/Security/PasswordHasher.cs` | Dòng 9–30: Băm mật khẩu bằng thuật toán SHA256, không lưu plain text. |
| **K3.4** | Xuất bảng thanh toán lương ra file Excel (.xlsx) | `Payroll.Plugins/Exporters/PayrollExcelExporter.cs` | Dòng 9–110: Dùng thư viện `ClosedXML` tạo file Excel có header, kẻ viền và hàm `SUM()`. |
| **K4.1** | Cookie Authentication & Đăng nhập qua Minimal API Endpoint | `Payroll.WebApp/Program.cs` | Dòng 62–134: `AddAuthentication(Cookie...)`, endpoint `POST /api/account/login` gọi `SignInAsync`. |
| **K4.2** | Ít nhất 3 component tái sử dụng có `[Parameter]` / `EventCallback` | `Payroll.WebApp/Components/Controls/PayslipCard.razor`<br>`Payroll.WebApp/Components/Controls/TimesheetCalendar.razor`<br>`Payroll.WebApp/Components/Controls/MoneyText.razor`<br>`Payroll.WebApp/Components/Controls/ConfirmDialog.razor` | 4 component tái sử dụng độc lập, hỗ trợ tham số và callback. |
| **K4.3** | Scoped State Store quản lý bảng công nháp (như ShoppingCart eShop) | `Payroll.WebApp/Services/DraftTimesheetStateStore.cs` | Dòng 15–60: Lưu trữ tạm thời trạng thái ngày công nháp trước khi lưu CSDL. |
| **K5.1** | Hồ sơ PlantUML (Use Case, Class, Sequence) | `docs/uml/use-case.puml`<br>`docs/uml/class-diagram.puml`<br>`docs/uml/sequence-calculate-payroll.puml` | Khớp 100% tên lớp, hàm và quan hệ trong mã nguồn thực tế. |
| **K5.2** | Sơ đồ ERD chuẩn hóa 3NF | `docs/ERD.md` | Bản vẽ thực thể quan hệ Mermaid với 13 bảng dữ liệu. |
| **K5.3** | Bảng tính tay đối chiếu 3 nhân viên mẫu | `docs/vi-du-tinh-tay.md` | Bảng tính tay chi tiết từng đồng (Độc thân, có con, lương vượt trần) đối chiếu với `PayrollCalculator`. |
