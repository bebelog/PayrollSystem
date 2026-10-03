# Ví Dụ Tính Tay Đối Chiếu Bảng Lương (3 Nhân Viên Mẫu)
**Hệ Thống Chấm Công & Tính Lương (SmartPayroll)**  
*Sinh viên thực hiện:* Nguyễn Viết Mẫn (MSSV: 23K4080026) - Trường ĐH Kinh tế, ĐH Huế  

---

## I. BẢNG THÔNG SỐ CẤU HÌNH ÁP DỤNG (KỲ THÁNG 10/2026)

* Số ngày công chuẩn của kỳ: **22 ngày**
* Trần lương đóng bảo hiểm: **50.600.000 đ**
* Mức giảm trừ gia cảnh bản thân: **15.500.000 đ/tháng**
* Mức giảm trừ mỗi người phụ thuộc: **6.200.000 đ/người/tháng**
* Tỷ lệ đóng bảo hiểm người lao động:
  * BHXH: **8.0%**
  * BHYT: **1.5%**
  * BHTN: **1.0%**
  * *Tổng bảo hiểm:* **10.5%**
* Biểu thuế lũy tiến từng phần (5 bậc mẫu):
  * **Bậc 1:** Đến 5.000.000 đ $\to$ **5%**
  * **Bậc 2:** Trên 5.000.000 đ đến 10.000.000 đ $\to$ **10%**
  * **Bậc 3:** Trên 10.000.000 đ đến 18.000.000 đ $\to$ **20%**
  * **Bậc 4:** Trên 18.000.000 đ đến 32.000.000 đ $\to$ **30%**
  * **Bậc 5:** Trên 32.000.000 đ $\to$ **35%**

---

## II. CHI TIẾT TÍNH TAY TỪNG TRƯỜNG HỢP

### 1. Trường Hợp 1: Có Người Phụ Thuộc (Nhân viên: Trần Thị Bình - Kế toán)

* **Dữ liệu đầu vào:**
  * Lương cơ bản hợp đồng: $22.000.000$ đ
  * Phụ cấp: $1.500.000$ đ
  * Ngày công thực tế: $22$ ngày (21 ngày đi làm + 1 ngày phép có lương)
  * Số người phụ thuộc: **2 con** (Trần Gia Hân, Trần Bảo Nam)

* **Các bước tính toán:**
  1. **Lương theo công:** $22.000.000 \times \frac{22}{22} = \mathbf{22.000.000}$ đ
  2. **Tổng thu nhập (Gross):** $22.000.000 + 1.500.000 = \mathbf{23.500.000}$ đ
  3. **Bảo hiểm trích trừ (Mức đóng: $\min(22.000.000, 50.600.000) = 22.000.000$ đ):**
     * BHXH (8%): $22.000.000 \times 8\% = 1.760.000$ đ
     * BHYT (1.5%): $22.000.000 \times 1.5\% = 330.000$ đ
     * BHTN (1%): $22.000.000 \times 1\% = 220.000$ đ
     * $\Rightarrow$ **Tổng bảo hiểm:** $1.760.000 + 330.000 + 220.000 = \mathbf{2.310.000}$ đ
  4. **Giảm trừ gia cảnh:**
     * Bản thân: $15.500.000$ đ
     * Người phụ thuộc: $2 \times 6.200.000 = 12.400.000$ đ
     * $\Rightarrow$ **Tổng giảm trừ gia cảnh:** $15.500.000 + 12.400.000 = \mathbf{27.900.000}$ đ
  5. **Thu nhập tính thuế:**
     $$\text{Gross} - \text{Bảo hiểm} - \text{Giảm trừ} = 23.500.000 - 2.310.000 - 27.900.000 = -6.710.000 \text{ đ} \le 0 \implies \mathbf{0} \text{ đ}$$
  6. **Thuế TNCN:** $\mathbf{0}$ đ
  7. **Thực lĩnh (Net):**
     $$\text{Gross} - \text{Bảo hiểm} - \text{Thuế} = 23.500.000 - 2.310.000 - 0 = \mathbf{21.190.000} \text{ đ}$$

* **Đối chiếu với `PayrollCalculator`:** Kết quả mã nguồn sinh ra chính xác $21.190.000$ đ (Lệch: 0 đ).

---

### 2. Trường Hợp 2: Không Có Người Phụ Thuộc (Nhân viên: Nguyễn Văn An - Kỹ thuật)

* **Dữ liệu đầu vào:**
  * Lương cơ bản hợp đồng: $18.000.000$ đ
  * Phụ cấp: $2.000.000$ đ
  * Ngày công thực tế: $22$ ngày
  * Số người phụ thuộc: **0 người**

* **Các bước tính toán:**
  1. **Lương theo công:** $18.000.000 \times \frac{22}{22} = \mathbf{18.000.000}$ đ
  2. **Tổng thu nhập (Gross):** $18.000.000 + 2.000.000 = \mathbf{20.000.000}$ đ
  3. **Bảo hiểm trích trừ (Mức đóng: $18.000.000$ đ):**
     * BHXH (8%): $18.000.000 \times 8\% = 1.440.000$ đ
     * BHYT (1.5%): $18.000.000 \times 1.5\% = 270.000$ đ
     * BHTN (1%): $18.000.000 \times 1\% = 180.000$ đ
     * $\Rightarrow$ **Tổng bảo hiểm:** $1.440.000 + 270.000 + 180.000 = \mathbf{1.890.000}$ đ
  4. **Giảm trừ gia cảnh:** Bản thân = $\mathbf{15.500.000}$ đ
  5. **Thu nhập tính thuế:**
     $$20.000.000 - 1.890.000 - 15.500.000 = \mathbf{2.610.000} \text{ đ}$$
  6. **Thuế TNCN lũy tiến:**
     * Toàn bộ khoản $2.610.000$ đ rơi vào **Bậc 1** (thuế suất 5%):
     $$\text{Thuế TNCN} = 2.610.000 \times 5\% = \mathbf{130.500} \text{ đ}$$
  7. **Thực lĩnh (Net):**
     $$\text{Gross} - \text{Bảo hiểm} - \text{Thuế} = 20.000.000 - 1.890.000 - 130.500 = \mathbf{17.979.500} \text{ đ}$$

* **Đối chiếu với `PayrollCalculator`:** Kết quả mã nguồn sinh ra chính xác $17.979.500$ đ (Lệch: 0 đ).

---

### 3. Trường Hợp 3: Lương Vượt Trần Bảo Hiểm (Nhân viên: Lê Hoàng Cường - Giám đốc kỹ thuật)

* **Dữ liệu đầu vào:**
  * Lương cơ bản hợp đồng: $65.000.000$ đ **(VƯỢT TRẦN 50.600.000 đ)**
  * Phụ cấp: $5.000.000$ đ
  * Ngày công thực tế: $22$ ngày
  * Số người phụ thuộc: **1 con** (Lê Tuấn Khang)

* **Các bước tính toán:**
  1. **Lương theo công:** $65.000.000 \times \frac{22}{22} = \mathbf{65.000.000}$ đ
  2. **Tổng thu nhập (Gross):** $65.000.000 + 5.000.000 = \mathbf{70.000.000}$ đ
  3. **Bảo hiểm trích trừ (Áp dụng trần $\min(65.000.000, 50.600.000) = \mathbf{50.600.000}$ đ):**
     * BHXH (8%): $50.600.000 \times 8\% = 4.048.000$ đ
     * BHYT (1.5%): $50.600.000 \times 1.5\% = 759.000$ đ
     * BHTN (1%): $50.600.000 \times 1\% = 506.000$ đ
     * $\Rightarrow$ **Tổng bảo hiểm:** $4.048.000 + 759.000 + 506.000 = \mathbf{5.313.000}$ đ
  4. **Giảm trừ gia cảnh:**
     * Bản thân: $15.500.000$ đ
     * Người phụ thuộc (1 người): $6.200.000$ đ
     * $\Rightarrow$ **Tổng giảm trừ:** $15.500.000 + 6.200.000 = \mathbf{21.700.000}$ đ
  5. **Thu nhập tính thuế:**
     $$70.000.000 - 5.313.000 - 21.700.000 = \mathbf{42.987.000} \text{ đ}$$
  6. **Thuế TNCN theo 5 bậc lũy tiến từng phần:**
     * Bậc 1 ($0 \to 5.000.000$ đ, 5%): $5.000.000 \times 5\% = 250.000$ đ
     * Bậc 2 ($5.000.000 \to 10.000.000$ đ, 10%): $5.000.000 \times 10\% = 500.000$ đ
     * Bậc 3 ($10.000.000 \to 18.000.000$ đ, 20%): $8.000.000 \times 20\% = 1.600.000$ đ
     * Bậc 4 ($18.000.000 \to 32.000.000$ đ, 30%): $14.000.000 \times 30\% = 4.200.000$ đ
     * Bậc 5 (Trên $32.000.000$ đ, 35%): $(42.987.000 - 32.000.000) \times 35\% = 10.987.000 \times 35\% = 3.845.450$ đ
     * $\Rightarrow$ **Tổng thuế TNCN:**
     $$250.000 + 500.000 + 1.600.000 + 4.200.000 + 3.845.450 = \mathbf{10.395.450} \text{ đ}$$
  7. **Thực lĩnh (Net):**
     $$\text{Gross} - \text{Bảo hiểm} - \text{Thuế} = 70.000.000 - 5.313.000 - 10.395.450 = \mathbf{54.291.550} \text{ đ}$$

* **Đối chiếu với `PayrollCalculator`:** Kết quả mã nguồn sinh ra chính xác $54.291.550$ đ (Lệch: 0 đ).

---

## III. BẢNG TỔNG HỢP ĐỐI CHIẾU

| Chỉ Tiêu | Trần Thị Bình | Nguyễn Văn An | Lê Hoàng Cường |
| :--- | :--- | :--- | :--- |
| **Đặc điểm** | Có 2 con | Độc thân (0 con) | **Lương vượt trần 50.6tr** |
| **Lương Hợp Đồng** | 22.000.000 đ | 18.000.000 đ | 65.000.000 đ |
| **Phụ Cấp** | 1.500.000 đ | 2.000.000 đ | 5.000.000 đ |
| **Ngày Công Chuẩn / Thực** | 22 / 22 ngày | 22 / 22 ngày | 22 / 22 ngày |
| **Tổng Gross** | **23.500.000 đ** | **20.000.000 đ** | **70.000.000 đ** |
| **Tổng Bảo Hiểm (10.5%)** | 2.310.000 đ | 1.890.000 đ | **5.313.000 đ** *(Tính theo trần)* |
| **Giảm Trừ Gia Cảnh** | 27.900.000 đ | 15.500.000 đ | 21.700.000 đ |
| **Thu Nhập Tính Thuế** | 0 đ | 2.610.000 đ | 42.987.000 đ |
| **Thuế TNCN** | 0 đ | 130.500 đ | 10.395.450 đ |
| **Thực Lĩnh (Net) Tính Tay** | **21.190.000 đ** | **17.979.500 đ** | **54.291.550 đ** |
| **Thực Lĩnh (Net) Chương Trình** | **21.190.000 đ** | **17.979.500 đ** | **54.291.550 đ** |
| **Độ Lệch** | **0 đ** | **0 đ** | **0 đ** |

