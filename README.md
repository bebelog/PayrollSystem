# ĐỒ ÁN CAPSTONE: CHẤM CÔNG & TÍNH LƯƠNG (TIMESHEET & PAYROLL)
**Môn học:** Lập trình ứng dụng Web  
**Sinh viên thực hiện:** Nguyễn Viết Mẫn  
**Mã số sinh viên (MSSV):** 23K4080026  
**Trường:** Đại học Kinh tế - Đại học Huế (HUE)  

---

## 1. GIỚI THIỆU HỆ THỐNG
Hệ thống **Chấm công & Tính lương (SmartPayroll)** được xây dựng chuyên biệt cho doanh nghiệp quy mô 20–50 nhân viên, tuân thủ nghiêm ngặt **Kiến trúc sạch (Clean Architecture)** phỏng theo cấu trúc mẫu dự án eShop của Microsoft/Bebelog.

### Điểm nhấn kỹ thuật & nghiệp vụ:
1. **Kiến trúc 4 dự án tách biệt hoàn toàn**:
   * `Payroll.CoreBusiness`: 0 NuGet, 0 tham chiếu, chứa Entity 3NF, Domain Exceptions và nghiệp vụ thuần (`PayrollCalculator`, `PeriodStateMachine`, `TimesheetStateMachine`).
   * `Payroll.UseCases`: Chứa Use Cases và Plugin Interfaces. Chỉ tham chiếu `Payroll.CoreBusiness`.
   * `Payroll.Plugins`: Cài đặt Dapper SQL Server, băm mật khẩu SHA256, xuất Excel ClosedXML.
   * `Payroll.WebApp`: Blazor Server, chỉ `Program.cs` là composition root biết `Payroll.Plugins`.
2. **LUẬT 1 – Công thức tính lương động**: 100% tham số đọc từ bảng cấu hình (không hardcode), trích trừ bảo hiểm theo trần, giảm trừ gia cảnh và tính thuế TNCN lũy tiến từng phần. Lưu snapshot dòng `PayslipLine`.
3. **LUẬT 2 – Chống sửa kỳ đã chốt & Phân quyền dữ liệu**: Phân quyền dữ liệu tại UseCase qua Claims (nhân viên chỉ xem bảng công và phiếu lương của chính mình, chặn đứng lỗ hổng IDOR). Chỉ tính lương cho nhân viên có bảng công **Đã duyệt**.
4. **Giao dịch toàn vẹn ACID**: Lưu Header–Line (`Payslip` – `PayslipLine`) trong 1 `IDbTransaction`.

---

## 2. HƯỚNG DẪN CÀI ĐẶT & CHẠY DỰ ÁN

### Bước 1: Khởi tạo Cơ sở dữ liệu SQL Server
Mở SQL Server Management Studio (SSMS) hoặc Visual Studio, mở file script `Payroll.SchemaAndData.sql` ở thư mục gốc và thực thi:
```sql
-- File: Payroll.SchemaAndData.sql
-- Tự động tạo CSDL PayrollDB và nạp toàn bộ cấu hình, phòng ban, nhân viên, bảng công mẫu tháng 10/2026
```

### Bước 2: Cấu hình Chuỗi kết nối
Mở file `Payroll.WebApp/appsettings.json` và kiểm tra chuỗi kết nối:
```json
"ConnectionStrings": {
  "PayrollConnection": "Server=localhost;Database=PayrollDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Bước 3: Chạy ứng dụng
Mở file Solution `PayrollSystem.sln` trong Visual Studio 2022 và bấm **F5** (hoặc `Ctrl + F5`).  
Hoặc chạy lệnh từ dòng lệnh:
```bash
dotnet run --project Payroll.WebApp/Payroll.WebApp.csproj
```

---

## 3. DANH SÁCH TÀI KHOẢN MẪU THỬ NGHIỆM

Hệ thống có sẵn các nút bấm **1-Click Tự Động Điền** ngay trên trang Đăng nhập (`/login`):

| Tên Đăng Nhập | Mật Khẩu | Vai Trò | Ý Nghĩa Kịch Bản Nghiệp Vụ |
| :--- | :--- | :--- | :--- |
| **`accountant`** | `admin123` | **Kế toán (Admin)** | Toàn quyền CRUD nhân viên, duyệt bảng công, tính lương và chốt kỳ, xuất Excel. |
| **`emp_an`** | `123` | Nhân viên | Lương 18.000.000 đ, **độc thân** (0 người phụ thuộc) $\to$ Thu nhập rơi vào Bậc 1 thuế. |
| **`emp_binh`** | `123` | Nhân viên | Lương 22.000.000 đ, **có 2 con phụ thuộc** $\to$ Giảm trừ gia cảnh lớn, thuế TNCN = 0. |
| **`emp_cuong`** | `123` | Nhân viên | Lương 65.000.000 đ, **LƯƠNG VƯỢT TRẦN BẢO HIỂM 50.600.000 đ** $\to$ Đóng bảo hiểm kịch trần. |

---

## 4. SỐ LIỆU CẤU HÌNH CẦN XÁC MINH (SOURCE NOTE)

Theo quy định đề tài, các số liệu sau được đánh dấu `SourceNote = "GIÁ TRỊ MẪU - CẦN XÁC MINH VỚI VĂN BẢN GỐC"` và được lưu trong bảng cấu hình:
1. **Trần lương đóng bảo hiểm:** $50.600.000$ đ (20 lần mức lương cơ sở giả định $2.340.000$ đ).
2. **Mức giảm trừ gia cảnh bản thân:** $15.500.000$ đ/tháng.
3. **Mức giảm trừ gia cảnh người phụ thuộc:** $6.200.000$ đ/người/tháng.
4. **Tỷ lệ trích bảo hiểm người lao động:** BHXH 8.0%, BHYT 1.5%, BHTN 1.0%.
5. **Biểu thuế lũy tiến từng phần:** 5 bậc mẫu (5%, 10%, 20%, 30%, 35%).


---

## 5. TÀI LIỆU KÈM THEO & KỊCH BẢN KIỂM THỬ

1. **Kịch bản kiểm thử chi tiết**: [docs/KICH-BAN-KIEM-THU.md](docs/KICH-BAN-KIEM-THU.md) (Ma trận 17 ca kiểm thử từ TC-AUTH-01 đến TC-CFG-01, có bước thực hiện, kết quả mong đợi và bảng đối chiếu tiêu chí chấm điểm).
2. **Ví dụ tính tay đối chiếu**: [docs/vi-du-tinh-tay.md](docs/vi-du-tinh-tay.md) (3 ca tính tay đối soát khớp 100% từng đồng với `PayrollCalculator`).
3. **Mô hình thực thể liên kết (ERD)**: [docs/ERD.md](docs/ERD.md) (13 bảng cơ sở dữ liệu chuẩn hóa 3NF).
4. **Biểu đồ UML (PlantUML)**:
   * Use Case Diagram: [docs/uml/use-case.puml](docs/uml/use-case.puml)
   * Class Diagram: [docs/uml/class-diagram.puml](docs/uml/class-diagram.puml)
   * Sequence Diagram (Tính lương): [docs/uml/sequence-calculate-payroll.puml](docs/uml/sequence-calculate-payroll.puml)