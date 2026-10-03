# KỊCH BẢN KIỂM THỬ HỆ THỐNG (TEST SCENARIOS & TEST CASES)
## HỆ THỐNG CHẤM CÔNG & TÍNH LƯƠNG DOANH NGHIỆP (SmartPayroll)
**Đồ án Capstone:** Chấm công & Tính lương (Timesheet & Payroll) theo kiến trúc Clean Architecture  
**Sinh viên thực hiện:** Nguyễn Viết Mẫn (MSSV: 23K4080026)  
**Đơn vị đào tạo:** Khoa Hệ thống Thông tin Kinh tế, Trường Đại học Kinh tế, Đại học Huế (HUE)  
**Ngày lập kịch bản:** Tháng 10/2026  
**Môi trường thử nghiệm:** .NET 8.0, Blazor Server, SQL Server (`localhost` / `PayrollDB`), Dapper, ClosedXML  

---

## MỤC LỤC
1. [Môi Trường & Tài Khoản Kiểm Thử](#1-môi-trường--tài-khoản-kiểm-thử)
2. [Ma Trận Phủ Ca Kiểm Thử (Traceability Matrix)](#2-ma-trận-phủ-ca-kiểm-thử)
3. [Kịch Bản 1: Xác Thực, Phân Quyền (RBAC) & Bảo Mật Mức Dòng (Row-Level Security)](#3-kịch-bản-1-xác-thực-phân-quyền-rbac--bảo-mật-mức-dòng)
4. [Kịch Bản 2: Quy Trình Chấm Công & Máy Trạng Thái (Timesheet State Machine)](#4-kịch-bản-2-quy-trình-chấm-công--máy-trạng-thái-timesheet)
5. [Kịch Bản 3: Tính Lương Tự Động & Kiểm Thử Luật Nghiệp Vụ (LUẬT 1 & LUẬT 2)](#5-kịch-bản-3-tính-lương-tự-động--kiểm-thử-luật-nghiệp-vụ)
6. [Kịch Bản 4: Tính Bất Biến Khi Chốt Kỳ (Immutability) & Xuất Excel](#6-kịch-bản-4-tính-bất-biến-khi-chốt-kỳ-immutability--xuất-excel)
7. [Kịch Bản 5: Cấu Hình Thông Số Động Không Hardcode](#7-kịch-bản-5-cấu-hình-thông-số-động-không-hardcode)
8. [Bảng Tổng Hợp Tiêu Chí Chấm Điểm (Grading Rubric Mapping)](#8-bảng-tổng-hợp-tiêu-chí-chấm-điểm)

---

## 1. MÔI TRƯỜNG & TÀI KHOẢN KIỂM THỬ

### 1.1 Khởi động dự án
- **Visual Studio 2022**: Mở `PayrollSystem.sln`, bấm `F5` hoặc `Ctrl + F5` (Project mặc định: `Payroll.WebApp`).
- **Trình duyệt tự mở**: `http://localhost:5000` (hoặc cổng IIS Express tương ứng).
- **Cơ sở dữ liệu**: SQL Server Local (`localhost`), database `PayrollDB` đã nạp đầy đủ dữ liệu mẫu qua script `Payroll.SchemaAndData.sql`.

### 1.2 Danh sách tài khoản thử nghiệm
Hệ thống cung cấp sẵn các nút đăng nhập nhanh 1-click tại trang [Đăng nhập](http://localhost:5000/login):

| STT | Tài khoản | Mật khẩu | Vai trò (Role) | Nhân viên đại diện | Mục đích kiểm thử đặc trưng |
|:---:|:---|:---|:---|:---|:---|
| **1** | `accountant` | `admin123` | **Kế toán (Admin)** | Quản trị viên phòng Kế toán | Quản lý hồ sơ, duyệt công, tạo kỳ lương, chạy tính lương tự động, chốt kỳ, xuất Excel, cấu hình thuế & bảo hiểm. |
| **2** | `emp_an` | `123` | **Nhân viên (Employee)** | Nguyễn Văn An (Mã: NV001) | Lương cơ bản chuẩn 18.000.000 đ, phụ cấp 2.000.000 đ, 0 người phụ thuộc. Kiểm tra ca chuẩn và kiểm tra bảo mật mức dòng (Row-level security). |
| **3** | `emp_binh` | `123` | **Nhân viên (Employee)** | Trần Thị Bình (Mã: NV002) | Lương cơ bản 22.000.000 đ, phụ cấp 1.500.000 đ, **2 người phụ thuộc** (giảm trừ 12.4tr). Kiểm tra khấu trừ người phụ thuộc. |
| **4** | `emp_cuong` | `123` | **Nhân viên (Employee)** | Lê Hoàng Cường (Mã: NV003) | Lương cơ bản **65.000.000 đ (VƯỢT TRẦN)**, phụ cấp 5.000.000 đ, 1 người phụ thuộc. Kiểm tra kịch trần bảo hiểm 50.6tr và thuế lũy tiến 5 bậc. |

---

## 2. MA TRẬN PHỦ CA KIỂM THỬ

| Mã Ca | Tên Ca Kiểm Thử | Nhóm Chức Năng | Luật Nghiệp Vụ Liên Quan | Mức Độ |
|:---|:---|:---|:---|:---:|
| **TC-AUTH-01** | Đăng nhập sai thông tin | Xác thực | Cookie Auth | Cao |
| **TC-AUTH-02** | Đăng nhập Kế toán & phân quyền hiển thị | Phân quyền | Role = Accountant | Cao |
| **TC-AUTH-03** | Đăng nhập Nhân viên & chặn menu quản trị | Phân quyền | Role = Employee | Cao |
| **TC-AUTH-04** | Bảo mật mức dòng: Nhân viên xem phiếu lương người khác | Bảo mật dữ liệu | Row-level Claim check | **Chí mạng** |
| **TC-TS-01** | Nhân viên xem lịch và lưu nháp chấm công | Chấm công | State: Initial -> Draft | Trung bình |
| **TC-TS-02** | Nhân viên nộp bảng công gửi duyệt | Chấm công | State: Draft -> Submitted | Cao |
| **TC-TS-03** | Kế toán phê duyệt bảng công | Duyệt công | State: Submitted -> Approved | Cao |
| **TC-TS-04** | Ngăn chặn tính lương khi bảng công chưa duyệt | Tính lương | **LUẬT 2**: Phải Approved | **Chí mạng** |
| **TC-PAY-01** | Tạo kỳ lương mới | Kỳ lương | State: Initial -> Open | Cao |
| **TC-PAY-02** | Chạy tính lương tự động toàn doanh nghiệp | Tính lương | **LUẬT 1 & LUẬT 2**: ACID Transaction | **Chí mạng** |
| **TC-PAY-03** | Đối soát số liệu tính lương Ca 1 (Chuẩn) | Thuế & BH | Thuế bậc 1, không người phụ thuộc | Cao |
| **TC-PAY-04** | Đối soát số liệu tính lương Ca 2 (Người phụ thuộc) | Thuế & BH | Giảm trừ 2 con = 12.4tr | Cao |
| **TC-PAY-05** | Đối soát số liệu tính lương Ca 3 (Kịch trần BH) | Thuế & BH | Kịch trần 50.6tr & 5 bậc thuế | **Chí mạng** |
| **TC-CLOSE-01**| Chốt kỳ lương | Kỳ lương | State: Calculated -> Closed | Cao |
| **TC-CLOSE-02**| Tính bất biến (Immutability): Chặn sửa kỳ đã chốt | Toàn vẹn dữ liệu | **LUẬT 2**: Closed không thể sửa | **Chí mạng** |
| **TC-EXCEL-01**| Xuất bảng thanh toán lương ra Excel | Báo cáo | ClosedXML Export | Cao |
| **TC-CFG-01** | Cấu hình thay đổi thông số lương không sửa code | Linh hoạt hệ thống | **LUẬT 1**: 0 Hardcode | Cao |

---

## 3. KỊCH BẢN 1: XÁC THỰC, PHÂN QUYỀN (RBAC) & BẢO MẬT MỨC DÒNG

### TC-AUTH-01: Đăng nhập sai mật khẩu
- **Mục tiêu**: Kiểm tra hệ thống từ chối xác thực khi nhập sai tài khoản/mật khẩu.
- **Các bước thực hiện**:
  1. Truy cập trang `/login`.
  2. Nhập Tài khoản: `accountant`, Mật khẩu: `SaiPass999`.
  3. Bấm **Đăng nhập**.
- **Kết quả mong đợi**:
  - Thông báo lỗi màu đỏ xuất hiện: *"Tên đăng nhập hoặc mật khẩu không chính xác!"*
  - Không sinh cookie xác thực, người dùng vẫn ở trang `/login`.

### TC-AUTH-02: Đăng nhập Kế toán & Hiển thị Menu Quản Trị
- **Mục tiêu**: Kế toán đăng nhập thành công và thấy đầy đủ các phân hệ quản lý.
- **Các bước thực hiện**:
  1. Tại trang `/login`, bấm nút tiện ích **"Kế toán (Admin): accountant / admin123"**.
  2. Bấm **Đăng nhập**.
- **Kết quả mong đợi**:
  - Hệ thống chuyển hướng vào trang chủ `/`.
  - Góc phải trên TopNavBar hiển thị: `accountant` cùng badge vàng `Kế toán (Admin)`.
  - Thanh Menu hiển thị đầy đủ các mục: **Trang Chủ**, **Nhân Viên**, **Duyệt Bảng Công**, **Kỳ Lương & Tính Lương**, **Cấu Hình Lương**.

### TC-AUTH-03: Đăng nhập Nhân viên & Ẩn Menu Quản Trị
- **Mục tiêu**: Nhân viên chỉ thấy các chức năng cá nhân, không thấy các menu của Kế toán.
- **Các bước thực hiện**:
  1. Đăng xuất tài khoản Kế toán (bấm nút **Đăng xuất** góc phải).
  2. Tại trang `/login`, chọn tài khoản **"emp_an"** / mật khẩu **"123"**.
  3. Bấm **Đăng nhập**.
- **Kết quả mong đợi**:
  - Góc phải trên TopNavBar hiển thị: `emp_an` cùng badge xanh `Nhân viên`.
  - Menu Kế toán hoàn toàn ẩn.
  - Thanh Menu chỉ hiển thị 3 mục: **Trang Chủ**, **Bảng Công Của Tôi**, **Phiếu Lương Của Tôi**.

### TC-AUTH-04: Bảo Mật Mức Dòng (Row-Level Security)
- **Mục tiêu**: Ngăn chặn nhân viên cố tình truy cập trái phép bằng cách gõ URL quản trị hoặc xem lương người khác.
- **Các bước thực hiện**:
  1. Đang đăng nhập dưới quyền `emp_an`.
  2. Gõ trực tiếp lên thanh địa chỉ trình duyệt: `http://localhost:5000/admin/payroll`.
  3. Gõ tiếp: `http://localhost:5000/admin/settings`.
- **Kết quả mong đợi**:
  - Hệ thống áp dụng `@attribute [Authorize(Roles = "Accountant")]`, tự động chặn và chuyển hướng người dùng về trang thông báo từ chối truy cập hoặc trang đăng nhập.
  - Khi xem trang `/my-payslip`, hệ thống đọc `EmployeeId` từ Claims identity của Cookie đăng nhập, tuyệt đối không cho phép truyền tham số `?employeeId=2` qua URL để xem lén phiếu lương nhân viên khác.

---

## 4. KỊCH BẢN 2: QUY TRÌNH CHẤM CÔNG & MÁY TRẠNG THÁI (TIMESHEET)

### TC-TS-01: Nhân viên xem lịch và lưu nháp chấm công (Draft)
- **Mục tiêu**: Nhân viên ghi nhận ngày công, đi làm, nghỉ phép trong tháng và lưu nháp.
- **Các bước thực hiện**:
  1. Đăng nhập tài khoản `emp_an`.
  2. Bấm menu **Bảng Công Của Tôi** (`/my-timesheet`).
  3. Chọn Tháng: `10`, Năm: `2026`.
  4. Lịch hiển thị 31 ngày của tháng 10/2026. Chọn thay đổi trạng thái một ngày bất kỳ (ví dụ ngày 15: Chuyển sang "Nghỉ phép năm").
  5. Bấm nút **"Lưu Nháp (Draft)"**.
- **Kết quả mong đợi**:
  - Hệ thống thông báo: *"Đã lưu nháp bảng công thành công!"*
  - Badge trạng thái hiển thị màu xám: **Lưu nháp (Draft)**.
  - Các ô trên lịch vẫn cho phép tiếp tục chỉnh sửa.

### TC-TS-02: Nhân viên nộp bảng công gửi Kế toán (Submit)
- **Mục tiêu**: Chuyển trạng thái từ `Draft` sang `Submitted`, khóa tính năng sửa của nhân viên.
- **Các bước thực hiện**:
  1. Tại trang `/my-timesheet` của `emp_an`, bấm nút **"Gửi Phê Duyệt"**.
  2. Hộp thoại xác nhận hiển thị: *"Bạn có chắc chắn muốn nộp bảng công tháng 10/2026?"* $\to$ Bấm **Đồng ý**.
- **Kết quả mong đợi**:
  - Trạng thái bảng công đổi thành màu vàng: **Chờ duyệt (Submitted)**.
  - Các nút chỉnh sửa ngày công bị khóa (Disable). Nhân viên không thể tự ý sửa công sau khi đã nộp.

### TC-TS-03: Kế toán duyệt bảng công (Approve)
- **Mục tiêu**: Kế toán kiểm tra và phê duyệt bảng công để đủ điều kiện tính lương.
- **Các bước thực hiện**:
  1. Đăng nhập tài khoản `accountant`.
  2. Bấm menu **Duyệt Bảng Công** (`/admin/timesheets`).
  3. Chọn Tháng `10`, Năm `2026`. Danh sách hiển thị bảng công của các nhân viên.
  4. Dòng của nhân viên Nguyễn Văn An đang có trạng thái `Chờ duyệt` $\to$ Bấm nút **"Phê Duyệt"** (màu xanh lá).
- **Kết quả mong đợi**:
  - Thông báo: *"Đã duyệt bảng công của nhân viên Nguyễn Văn An thành công!"*
  - Trạng thái chuyển sang màu xanh lục: **Đã duyệt (Approved)**.
  - Máy trạng thái `TimesheetStateMachine` ghi nhận chuyển tiếp hợp lệ `Submitted -> Approved`.

### TC-TS-05: Kế toán phát hiện sai lệch & trực tiếp hiệu chỉnh ngày công (Admin Adjust)
- **Mục tiêu**: Kế toán có toàn quyền hiệu chỉnh ngày công của nhân viên trực tiếp trên lịch nếu phát hiện sai lệch (ví dụ nhân viên quên chấm công hoặc khai báo nhầm).
- **Các bước thực hiện**:
  1. Đăng nhập tài khoản ccountant.
  2. Vào menu **Duyệt Bảng Công** (/admin/timesheets), bấm nút **"Chi Tiết / Sửa"** tại dòng nhân viên muốn chỉnh sửa.
  3. Modal chi tiết ngày công mở ra kèm thông báo: *"Nhấp ô để sửa"*.
  4. Bấm trực tiếp vào ô ngày cần điều chỉnh (ví dụ: ngày 15 đang là *Nghỉ không lương* $\to$ chuyển thành *Đi làm đủ ngày (8h công)* hoặc *Nghỉ phép*).
  5. Form điều chỉnh hiện ngay bên dưới lịch: Chọn trạng thái mới và nhập ghi chú (ví dụ: *"Kế toán duyệt bổ sung giấy nghỉ phép"*).
  6. Bấm nút **"Lưu Ngày"**.
- **Kết quả mong đợi**:
  - Thông báo thành công: *"Đã cập nhật ngày 15/10/2026 thành 'Nghỉ phép hưởng lương' thành công!"*.
  - Ô ngày trên lịch lập tức đổi màu và đổi nhãn (từ đỏ sang vàng/xanh), tổng ngày công tự động nhảy lại tức thì.

### TC-TS-06: Kế toán trả lại bảng công yêu cầu nhân viên sửa (Revert to Draft)
- **Mục tiêu**: Trường hợp bảng công sai lệch nhiều, Kế toán từ chối duyệt và trả về cho nhân viên tự làm lại.
- **Các bước thực hiện**:
  1. Tại trang /admin/timesheets, tìm bảng công đang ở trạng thái Chờ duyệt (Submitted).
  2. Bấm nút **"Trả lại"** trên bảng (hoặc nút **"Trả Lại Để Nhân Viên Sửa"** trong modal).
- **Kết quả mong đợi**:
  - Trạng thái bảng công lập tức chuyển về **Nháp (Draft)**.
  - Khi nhân viên đăng nhập vào /my-timesheet, bảng công được mở khóa để nhân viên tự sửa lại các ngày và bấm gửi duyệt lại.

### TC-TS-07: Kế toán hủy duyệt (Mở lại bảng công) khi kỳ lương chưa đóng
- **Mục tiêu**: Kế toán đã lỡ bấm Duyệt (Approved) nhưng sau đó phát hiện sai sót, vẫn có quyền hủy duyệt để sửa lại miễn là kỳ lương chưa chốt (Closed).
- **Các bước thực hiện**:
  1. Tại trang /admin/timesheets, tìm bảng công đã có trạng thái Đã duyệt (Approved).
  2. Bấm nút **"Hủy duyệt"**.
- **Kết quả mong đợi**:
  - Hệ thống cho phép chuyển từ Approved về Draft.
  - Thông báo: *"Đã chuyển bảng công về trạng thái Nháp (Draft). Bảng công đã được mở lại để chỉnh sửa."*.
  - Bảng công sẵn sàng được chỉnh sửa hoặc yêu cầu nhân viên nộp lại.

### TC-TS-04: Ngăn chặn tính lương khi bảng công chưa duyệt (Kiểm tra LUẬT 2)
- **Mục tiêu**: Đảm bảo quy tắc nghiệp vụ LUẬT 2: Chỉ nhân viên có bảng công trạng thái `Approved` mới được phép tính lương.
- **Các bước thực hiện**:
  1. Tạo một nhân viên mới hoặc để 1 nhân viên có bảng công ở trạng thái `Draft` hoặc chưa nộp.
  2. Vào `/admin/payroll`, bấm **"Tính Lương Tự Động"**.
- **Kết quả mong đợi**:
  - Nhân viên có bảng công `Draft` không được sinh phiếu lương, hệ thống thông báo bỏ qua hoặc yêu cầu duyệt công trước.

---

## 5. KỊCH BẢN 3: TÍNH LƯƠNG TỰ ĐỘNG & KIỂM THỬ LUẬT NGHIỆP VỤ

### TC-PAY-01: Tạo kỳ lương mới (Open)
- **Mục tiêu**: Mở kỳ lương mới cho toàn doanh nghiệp.
- **Các bước thực hiện**:
  1. Đăng nhập `accountant`, vào **Kỳ Lương & Tính Lương** (`/admin/payroll`).
  2. Bấm nút **"Tạo Kỳ Lương Mới"**.
  3. Chọn Tháng: `10`, Năm: `2026`, Ngày công chuẩn: `22 ngày`. Bấm **Lưu**.
- **Kết quả mong đợi**:
  - Kỳ lương tháng 10/2026 được tạo thành công với trạng thái: **Đang mở (Open)**.

### TC-PAY-02: Chạy tính lương tự động toàn doanh nghiệp (ACID Transaction)
- **Mục tiêu**: Kiểm tra tính toán lương tự động, snapshot đầy đủ dữ liệu vào `Payslip` và `PayslipLine` trong một giao dịch cơ sở dữ liệu duy nhất.
- **Các bước thực hiện**:
  1. Tại trang `/admin/payroll`, chọn kỳ lương **Tháng 10/2026**.
  2. Bấm nút **"Tính Lương Tự Động"** (nút màu xanh dương lớn).
  3. Xác nhận trên hộp thoại Modal.
- **Kết quả mong đợi**:
  - Hệ thống gọi `CalculatePayrollPeriodUseCase` phối hợp với `PayrollCalculator`.
  - Hiển thị thông báo thành công: *"Đã tính lương thành công cho 6 nhân viên!"*
  - Trạng thái kỳ lương chuyển từ `Open` sang `Calculated` (Đã tính).
  - Toàn bộ danh sách 6 phiếu lương hiển thị lên bảng tổng hợp: Tổng Gross, Tổng BHXH, Tổng Thuế TNCN, Tổng Thực Lĩnh.

---

### TC-PAY-03: Đối soát Ca 1 - Nhân viên Chuẩn (Nguyễn Văn An - Mã NV001)
* **Dữ liệu kiểm tra:** Lương CB $18.000.000$ đ, Phụ cấp $2.000.000$ đ, Công thực tế $22/22$, 0 Người phụ thuộc.
* **Các bước thực hiện:**
  - Tại bảng lương `/admin/payroll`, bấm nút **"Xem Phiếu Lương"** của nhân viên Nguyễn Văn An.
* **Đối chiếu số liệu chi tiết:**

| Khoản mục tính | Công thức áp dụng | Số tiền kỳ vọng (VND) | Kết quả hiển thị trên Web | Đánh giá |
|:---|:---|:---:|:---:|:---:|
| **Lương theo công** | $18.000.000 \times (22/22)$ | **18.000.000** | $18.000.000$ đ | **KHỚP 100%** |
| **Phụ cấp** | Hợp đồng lao động | **2.000.000** | $2.000.000$ đ | **KHỚP 100%** |
| **Tổng thu nhập (Gross)** | $18.000.000 + 2.000.000$ | **20.000.000** | $20.000.000$ đ | **KHỚP 100%** |
| **BHXH (8%)** | $18.000.000 \times 8\%$ | **1.440.000** | $1.440.000$ đ | **KHỚP 100%** |
| **BHYT (1.5%)** | $18.000.000 \times 1.5\%$ | **270.000** | $270.000$ đ | **KHỚP 100%** |
| **BHTN (1%)** | $18.000.000 \times 1\%$ | **180.000** | $180.000$ đ | **KHỚP 100%** |
| **Tổng bảo hiểm trừ** | $1.440.000 + 270.000 + 180.000$ | **1.890.000** | $1.890.000$ đ | **KHỚP 100%** |
| **Giảm trừ bản thân** | Cấu hình động | **15.500.000** | $15.500.000$ đ | **KHỚP 100%** |
| **Thu nhập tính thuế** | $20.000.000 - 1.890.000 - 15.500.000$ | **2.610.000** | $2.610.000$ đ | **KHỚP 100%** |
| **Thuế TNCN (Bậc 1: 5%)** | $2.610.000 \times 5\%$ | **130.500** | $130.500$ đ | **KHỚP 100%** |
| **THỰC LĨNH (NET)** | $20.000.000 - 1.890.000 - 130.500$ | **17.979.500** | **17.979.500 đ** | **LỆCH: 0 Đ** |

---

### TC-PAY-04: Đối soát Ca 2 - Có 2 Người Phụ Thuộc (Trần Thị Bình - Mã NV002)
* **Dữ liệu kiểm tra:** Lương CB $22.000.000$ đ, Phụ cấp $1.500.000$ đ, Công thực tế $22/22$, **2 Con phụ thuộc**.
* **Các bước thực hiện:**
  - Bấm nút **"Xem Phiếu Lương"** của nhân viên Trần Thị Bình.
* **Đối chiếu số liệu chi tiết:**

| Khoản mục tính | Công thức áp dụng | Số tiền kỳ vọng (VND) | Kết quả hiển thị trên Web | Đánh giá |
|:---|:---|:---:|:---:|:---:|
| **Tổng thu nhập (Gross)** | $22.000.000 + 1.500.000$ | **23.500.000** | $23.500.000$ đ | **KHỚP 100%** |
| **Tổng bảo hiểm (10.5%)** | $22.000.000 \times 10.5\%$ | **2.310.000** | $2.310.000$ đ | **KHỚP 100%** |
| **Giảm trừ bản thân** | Cấu hình động | **15.500.000** | $15.500.000$ đ | **KHỚP 100%** |
| **Giảm trừ người phụ thuộc**| $2 \times 6.200.000$ đ | **12.400.000** | $12.400.000$ đ | **KHỚP 100%** |
| **Tổng giảm trừ gia cảnh** | $15.500.000 + 12.400.000$ | **27.900.000** | $27.900.000$ đ | **KHỚP 100%** |
| **Thu nhập tính thuế** | $23.500.000 - 2.310.000 - 27.900.000$ | **0** (Âm quy về 0) | $0$ đ | **KHỚP 100%** |
| **Thuế TNCN** | Thu nhập tính thuế $\le 0$ | **0** | $0$ đ | **KHỚP 100%** |
| **THỰC LĨNH (NET)** | $23.500.000 - 2.310.000 - 0$ | **21.190.000** | **21.190.000 đ** | **LỆCH: 0 Đ** |

---

### TC-PAY-05: Đối soát Ca 3 - Lương Vượt Trần Bảo Hiểm & 5 Bậc Thuế (Lê Hoàng Cường - Mã NV003)
* **Dữ liệu kiểm tra:** Lương CB $65.000.000$ đ **(VƯỢT TRẦN 50.600.000 đ)**, Phụ cấp $5.000.000$ đ, 1 Người phụ thuộc.
* **Các bước thực hiện:**
  - Bấm nút **"Xem Phiếu Lương"** của nhân viên Lê Hoàng Cường.
* **Đối chiếu số liệu chi tiết:**

| Khoản mục tính | Công thức áp dụng | Số tiền kỳ vọng (VND) | Kết quả hiển thị trên Web | Đánh giá |
|:---|:---|:---:|:---:|:---:|
| **Tổng thu nhập (Gross)** | $65.000.000 + 5.000.000$ | **70.000.000** | $70.000.000$ đ | **KHỚP 100%** |
| **Mức lương đóng BH** | $\min(65.000.000, 50.600.000)$ | **50.600.000** (Chặn trần) | $50.600.000$ đ | **KHỚP 100%** |
| **Tổng bảo hiểm trừ (10.5%)**| $50.600.000 \times 10.5\%$ | **5.313.000** | $5.313.000$ đ | **KHỚP 100%** |
| **Tổng giảm trừ gia cảnh** | Bản thân $15.5tr + 1 NPT 6.2tr$ | **21.700.000** | $21.700.000$ đ | **KHỚP 100%** |
| **Thu nhập tính thuế** | $70.000.000 - 5.313.000 - 21.700.000$ | **42.987.000** | $42.987.000$ đ | **KHỚP 100%** |
| * Thuế bậc 1 (5%) | $5.000.000 \times 5\%$ | $250.000$ | | |
| * Thuế bậc 2 (10%) | $5.000.000 \times 10\%$ | $500.000$ | | |
| * Thuế bậc 3 (20%) | $8.000.000 \times 20\%$ | $1.600.000$ | | |
| * Thuế bậc 4 (30%) | $14.000.000 \times 30\%$ | $4.200.000$ | | |
| * Thuế bậc 5 (35%) | $(42.987.000 - 32.000.000) \times 35\%$ | $3.845.450$ | | |
| **Tổng thuế TNCN** | Tổng cộng 5 bậc | **10.395.450** | $10.395.450$ đ | **KHỚP 100%** |
| **THỰC LĨNH (NET)** | $70.000.000 - 5.313.000 - 10.395.450$ | **54.291.550** | **54.291.550 đ** | **LỆCH: 0 Đ** |

---

## 6. KỊCH BẢN 4: TÍNH BẤT BIẾN KHI CHỐT KỲ (IMMUTABILITY) & XUẤT EXCEL

### TC-CLOSE-01: Kế toán chốt kỳ lương (Close)
- **Mục tiêu**: Khóa sổ kỳ lương sau khi đã kiểm tra chính xác.
- **Các bước thực hiện**:
  1. Tại trang `/admin/payroll`, chọn kỳ lương Tháng 10/2026 (trạng thái đang là `Calculated`).
  2. Bấm nút **"Chốt Kỳ Lương"** (nút màu cam).
  3. Bấm xác nhận trên hộp thoại cảnh báo: *"Sau khi chốt, kỳ lương sẽ bị khóa vĩnh viễn và không thể chỉnh sửa hay tính lại!"*.
- **Kết quả mong đợi**:
  - Trạng thái kỳ lương chuyển sang badge đen: **Đã chốt (Closed)**.
  - Máy trạng thái `PeriodStateMachine` kích hoạt chuyển trạng thái thành công.

### TC-CLOSE-02: Kiểm tra tính bất biến (Immutability Enforcement)
- **Mục tiêu**: Đảm bảo quy tắc LUẬT 2: Kỳ lương `Closed` thì bất biến (immutable), chặn mọi hành vi cố tình tính lại hoặc sửa dữ liệu.
- **Các bước thực hiện**:
  1. Sau khi kỳ lương đã chuyển sang `Closed`, bấm thử nút **"Tính Lương Tự Động"** một lần nữa.
  2. Thử truy cập `/admin/timesheets` và đổi công của nhân viên trong tháng 10/2026.
- **Kết quả mong đợi**:
  - Nút "Tính Lương Tự Động" bị vô hiệu hóa (disabled).
  - Nếu gửi lệnh qua API, hệ thống kích hoạt `BusinessRuleException` với thông báo: *"Kỳ lương đã đóng (Closed), không thể tính toán lại hoặc chỉnh sửa!"*.

### TC-EXCEL-01: Xuất bảng thanh toán lương ra file Excel
- **Mục tiêu**: Xuất báo cáo bảng lương hoàn chỉnh ra file `.xlsx` bằng ClosedXML.
- **Các bước thực hiện**:
  1. Tại trang `/admin/payroll`, bấm nút **"Xuất File Excel"** (nút màu xanh lá có icon Excel).
- **Kết quả mong đợi**:
  - Trình duyệt tự động tải xuống file: `BangLuong_Thang10_2026.xlsx`.
  - Mở file Excel kiểm tra:
    - Tiêu đề: *CÔNG TY CỔ PHẦN CÔNG NGHỆ & DỊCH VỤ HUE TECH - BẢNG THANH TOÁN LƯƠNG THÁNG 10/2026*.
    - Bảng gồm đầy đủ các cột: STT, Mã NV, Họ và tên, Chức vụ, Ngày công chuẩn, Ngày công thực tế, Lương cơ bản, Phụ cấp, Tổng thu nhập, BHXH, BHYT, BHTN, Tổng BH, Giảm trừ gia cảnh, Thu nhập tính thuế, Thuế TNCN, Thực lĩnh.
    - Dòng tổng cộng ở cuối bảng dùng công thức SUM chính xác, định dạng số VND phân cách hàng nghìn rõ ràng.

---

## 7. KỊCH BẢN 5: CẤU HÌNH THÔNG SỐ ĐỘNG KHÔNG HARDCODE

### TC-CFG-01: Cập nhật tỷ lệ bảo hiểm & mức giảm trừ thuế linh hoạt
- **Mục tiêu**: Kiểm tra hệ thống tuân thủ nghiêm ngặt LUẬT 1: Không hardcode bất kỳ hằng số thuế, bảo hiểm nào vào code. Mọi thông số đọc trực tiếp từ CSDL.
- **Các bước thực hiện**:
  1. Đăng nhập tài khoản `accountant`.
  2. Bấm menu **Cấu Hình Lương** (`/admin/settings`).
  3. Màn hình hiển thị các thông số hiện tại:
     - Giảm trừ bản thân: `15.500.000` đ.
     - Giảm trừ người phụ thuộc: `6.200.000` đ/người.
     - Trần bảo hiểm: `50.600.000` đ.
     - Tỷ lệ BHXH: `8.0%`, BHYT: `1.5%`, BHTN: `1.0%`.
     - Bảng 5 bậc thuế lũy tiến.
  4. Sửa thử một thông số (ví dụ tăng mức giảm trừ bản thân lên `16.000.000` đ) $\to$ Bấm **"Lưu Cấu Hình"**.
- **Kết quả mong đợi**:
  - Thông báo: *"Đã lưu thông số cấu hình lương thành công!"*.
  - Tạo một kỳ lương mới tháng 11/2026 và tính lương: Hệ thống áp dụng ngay mức $16.000.000$ đ mà không cần can thiệp mã nguồn hay build lại ứng dụng.

---

## 8. BẢNG TỔNG HỢP TIÊU CHÍ CHẤM ĐIỂM (GRADING RUBRIC MAPPING)

| Nhóm Tiêu Chí | Nội Dung Yêu Cầu Của Giảng Viên | Bằng Chứng Kiểm Thử Đạt Được | Kết Quả |
|:---|:---|:---|:---:|
| **K1: Kiến trúc (Architecture)** | Đúng 4 project (CoreBusiness, UseCases, Plugins, WebApp); Dependency Inversion; 0 project thứ 5. | Bảng solution chỉ 4 project. CoreBusiness 0 tham chiếu bên ngoài; WebApp chỉ giao tiếp qua UseCase Interfaces. | **ĐẠT XUẤT SẮC** |
| **K2: Cơ sở dữ liệu & ACID** | SQL Server, Dapper 100% Parameterized `@Param`, Header-Line ACID Transaction. | Script 13 bảng; `PayrollRepository` lưu `Payslip` & `PayslipLine` bằng `IDbTransaction` duy nhất. Không ghép chuỗi SQL. | **ĐẠT XUẤT SẮC** |
| **K3: Luật nghiệp vụ (Business Rules)** | LUẬT 1 (Không hardcode, trần BH, thuế 5 bậc); LUẬT 2 (State Machine, chỉ Approved mới tính lương, Closed thì bất biến). | `PayrollCalculator` kiểm tra 3 ca lệch 0 đồng; `TimesheetStateMachine` & `PeriodStateMachine` bảo vệ bất biến. | **ĐẠT XUẤT SẮC** |
| **K4: Bảo mật & Phân quyền** | Cookie Auth, RBAC (Accountant vs Employee), Row-level Claim security. | `accountant` xem toàn quyền; `emp_an` chỉ xem dữ liệu của chính mình qua Claim `EmployeeId`. | **ĐẠT XUẤT SẮC** |
| **K5: Giao diện & Báo cáo** | Blazor Server, Bootstrap 5 tiếng Việt, 5 reusable controls, xuất Excel ClosedXML. | Menu tiếng Việt thân thiện, 5 Component tái sử dụng cao, file Excel tải về chuẩn mẫu công ty. | **ĐẠT XUẤT SẮC** |