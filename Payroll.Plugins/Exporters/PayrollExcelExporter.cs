using ClosedXML.Excel;
using Payroll.CoreBusiness.Entities;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins.Exporters;

public class PayrollExcelExporter : IPayrollExporter
{
    public Task<byte[]> ExportPayrollToExcelAsync(PayrollPeriod period, IEnumerable<Payslip> payslips)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add($"Luong_T{period.Month}_{period.Year}");

        // Tiêu đề báo cáo
        worksheet.Cell("A1").Value = "BẢNG THANH TOÁN LƯƠNG DOANH NGHIỆP";
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 16;
        worksheet.Cell("A1").Style.Font.FontColor = XLColor.Navy;
        worksheet.Range("A1:N1").Merge();

        worksheet.Cell("A2").Value = $"Kỳ lương: Tháng {period.Month}/{period.Year} | Ngày công chuẩn: {period.StandardWorkingDays} ngày | Trạng thái: {(period.Status == CoreBusiness.Enums.PayrollPeriodStatus.Closed ? "Đã chốt" : "Đã tính")}";
        worksheet.Cell("A2").Style.Font.Italic = true;
        worksheet.Range("A2:N2").Merge();

        // Header các cột
        string[] headers = {
            "STT", "Họ và Tên", "Phòng Ban", "Lương HĐ", "Công Chuẩn", "Công Thực", 
            "Lương Công", "Phụ Cấp", "Tổng Thu Nhập (Gross)", "Bảo Hiểm (10.5%)", 
            "Giảm Trừ Gia Cảnh", "Thu Nhập Tính Thuế", "Thuế TNCN", "Thực Lĩnh (Net)"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(220, 230, 242);
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Đổ dữ liệu từng nhân viên
        int row = 5;
        int stt = 1;
        var payslipList = payslips.ToList();

        foreach (var p in payslipList)
        {
            worksheet.Cell(row, 1).Value = stt++;
            worksheet.Cell(row, 2).Value = p.Employee?.FullName ?? $"NV #{p.EmployeeId}";
            worksheet.Cell(row, 3).Value = p.Employee?.Department?.DepartmentName ?? "---";
            worksheet.Cell(row, 4).Value = p.BaseSalarySnapshot;
            worksheet.Cell(row, 5).Value = p.StandardDaysSnapshot;
            worksheet.Cell(row, 6).Value = p.ActualDaysSnapshot;
            worksheet.Cell(row, 7).Value = p.WorkingSalary;
            worksheet.Cell(row, 8).Value = p.AllowanceSnapshot;
            worksheet.Cell(row, 9).Value = p.GrossSalary;
            worksheet.Cell(row, 10).Value = p.TotalInsuranceDeduction;
            worksheet.Cell(row, 11).Value = p.PersonalDeductionSnapshot + p.DependentDeductionSnapshot;
            worksheet.Cell(row, 12).Value = p.TaxableIncome;
            worksheet.Cell(row, 13).Value = p.PersonalIncomeTax;
            worksheet.Cell(row, 14).Value = p.NetSalary;

            // Định dạng số tiền
            for (int col = 4; col <= 14; col++)
            {
                if (col == 5 || col == 6)
                {
                    worksheet.Cell(row, col).Style.NumberFormat.Format = "0.#";
                }
                else
                {
                    worksheet.Cell(row, col).Style.NumberFormat.Format = "#,##0";
                }
            }

            for (int col = 1; col <= 14; col++)
            {
                worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            row++;
        }

        // Dòng tổng cộng
        worksheet.Cell(row, 1).Value = "TỔNG CỘNG";
        worksheet.Range(row, 1, row, 3).Merge();
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        for (int col = 4; col <= 14; col++)
        {
            if (col == 5 || col == 6) continue;
            var colLetter = worksheet.Column(col).ColumnLetter();
            worksheet.Cell(row, col).FormulaA1 = $"SUM({colLetter}5:{colLetter}{row - 1})";
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.NumberFormat.Format = "#,##0";
        }

        for (int col = 1; col <= 14; col++)
        {
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(242, 242, 242);
            worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return Task.FromResult(memoryStream.ToArray());
    }
}
