using Payroll.CoreBusiness.Entities;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;

namespace Payroll.CoreBusiness.DomainServices;

public class PayrollCalculationResult
{
    public decimal BaseSalarySnapshot { get; set; }
    public decimal StandardDaysSnapshot { get; set; }
    public decimal ActualDaysSnapshot { get; set; }
    public decimal WorkingSalary { get; set; }
    public decimal AllowanceSnapshot { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalInsuranceDeduction { get; set; }
    public decimal PersonalDeductionSnapshot { get; set; }
    public decimal DependentDeductionSnapshot { get; set; }
    public int DependentCountSnapshot { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal PersonalIncomeTax { get; set; }
    public decimal NetSalary { get; set; }
    public List<PayslipLine> Lines { get; set; } = new();
}

public static class PayrollCalculator
{
    /// <summary>
    /// LUẬT 1: Tính toán lương đầy đủ không hardcode, đọc hoàn toàn từ cấu hình
    /// </summary>
    public static PayrollCalculationResult Calculate(
        Contract contract,
        decimal actualWorkingDays,
        decimal standardWorkingDays,
        int activeDependentsCount,
        PayrollSetting setting,
        IEnumerable<DeductionRate> deductionRates,
        IEnumerable<TaxBracket> taxBrackets)
    {
        if (standardWorkingDays <= 0)
            throw new BusinessRuleException("Số ngày công chuẩn của kỳ lương phải lớn hơn 0.");

        if (contract == null || !contract.IsActive)
            throw new BusinessRuleException("Nhân viên không có hợp đồng lao động đang hiệu lực để tính lương.");

        var result = new PayrollCalculationResult
        {
            BaseSalarySnapshot = contract.BaseSalary,
            StandardDaysSnapshot = standardWorkingDays,
            ActualDaysSnapshot = actualWorkingDays,
            AllowanceSnapshot = contract.Allowance,
            DependentCountSnapshot = activeDependentsCount,
            PersonalDeductionSnapshot = setting.PersonalDeduction,
            DependentDeductionSnapshot = setting.DependentDeduction
        };

        // a) Lương theo công = lương cơ bản * ngày công thực tế / ngày công chuẩn
        result.WorkingSalary = Math.Round(contract.BaseSalary * actualWorkingDays / standardWorkingDays, MidpointRounding.AwayFromZero);
        result.Lines.Add(new PayslipLine
        {
            LineType = PayslipLineType.Earning,
            ItemCode = "WORKING_SALARY",
            ItemName = "Lương theo ngày công thực tế",
            RateOrThresholdSnapshot = actualWorkingDays,
            Amount = result.WorkingSalary,
            Note = $"{actualWorkingDays}/{standardWorkingDays} ngày công"
        });

        // b) Gross = Lương theo công + phụ cấp
        if (contract.Allowance > 0)
        {
            result.Lines.Add(new PayslipLine
            {
                LineType = PayslipLineType.Earning,
                ItemCode = "ALLOWANCE",
                ItemName = "Phụ cấp theo hợp đồng",
                RateOrThresholdSnapshot = null,
                Amount = contract.Allowance
            });
        }
        result.GrossSalary = result.WorkingSalary + contract.Allowance;

        // c) Bảo hiểm = Rate * min(lương đóng bảo hiểm, trần)
        decimal salaryForInsurance = Math.Min(contract.BaseSalary, setting.InsuranceCeiling);
        decimal totalInsurance = 0m;

        foreach (var rate in deductionRates.OrderBy(r => r.Code))
        {
            decimal insuranceAmount = Math.Round(salaryForInsurance * rate.Rate, MidpointRounding.AwayFromZero);
            totalInsurance += insuranceAmount;

            result.Lines.Add(new PayslipLine
            {
                LineType = PayslipLineType.Insurance,
                ItemCode = rate.Code,
                ItemName = $"Khấu trừ {rate.Name}",
                RateOrThresholdSnapshot = rate.Rate,
                Amount = insuranceAmount,
                Note = $"Áp dụng tỷ lệ {rate.Rate * 100:0.##}% trên mức đóng {salaryForInsurance:N0} đ (Trần: {setting.InsuranceCeiling:N0} đ)"
            });
        }
        result.TotalInsuranceDeduction = totalInsurance;

        // d) Thu nhập tính thuế = Gross - tổng bảo hiểm - giảm trừ bản thân - (số người phụ thuộc * mức giảm trừ), không âm
        decimal totalDependentDeduction = activeDependentsCount * setting.DependentDeduction;
        decimal totalDeductions = totalInsurance + setting.PersonalDeduction + totalDependentDeduction;

        decimal rawTaxable = result.GrossSalary - totalDeductions;
        result.TaxableIncome = Math.Max(0m, Math.Round(rawTaxable, MidpointRounding.AwayFromZero));

        result.Lines.Add(new PayslipLine
        {
            LineType = PayslipLineType.TaxDeduction,
            ItemCode = "PERSONAL_DED",
            ItemName = "Giảm trừ gia cảnh bản thân",
            RateOrThresholdSnapshot = setting.PersonalDeduction,
            Amount = setting.PersonalDeduction
        });

        if (activeDependentsCount > 0)
        {
            result.Lines.Add(new PayslipLine
            {
                LineType = PayslipLineType.TaxDeduction,
                ItemCode = "DEPENDENT_DED",
                ItemName = $"Giảm trừ gia cảnh ({activeDependentsCount} người phụ thuộc)",
                RateOrThresholdSnapshot = setting.DependentDeduction,
                Amount = totalDependentDeduction,
                Note = $"{activeDependentsCount} người x {setting.DependentDeduction:N0} đ"
            });
        }

        // e) Thuế TNCN theo biểu lũy tiến từng phần dựa trên TaxBracket
        decimal personalIncomeTax = 0m;
        if (result.TaxableIncome > 0)
        {
            var orderedBrackets = taxBrackets.OrderBy(b => b.BracketOrder).ToList();
            foreach (var bracket in orderedBrackets)
            {
                if (result.TaxableIncome > bracket.ThresholdMin)
                {
                    decimal taxableInBracket;
                    if (bracket.ThresholdMax.HasValue)
                    {
                        taxableInBracket = Math.Min(result.TaxableIncome, bracket.ThresholdMax.Value) - bracket.ThresholdMin;
                    }
                    else
                    {
                        taxableInBracket = result.TaxableIncome - bracket.ThresholdMin;
                    }

                    if (taxableInBracket > 0)
                    {
                        decimal taxForBracket = Math.Round(taxableInBracket * bracket.TaxRate, MidpointRounding.AwayFromZero);
                        personalIncomeTax += taxForBracket;

                        result.Lines.Add(new PayslipLine
                        {
                            LineType = PayslipLineType.Tax,
                            ItemCode = $"TAX_B{bracket.BracketOrder}",
                            ItemName = $"Thuế TNCN Bậc {bracket.BracketOrder} ({bracket.TaxRate * 100:0.#}%)",
                            RateOrThresholdSnapshot = bracket.TaxRate,
                            Amount = taxForBracket,
                            Note = $"Tính trên {taxableInBracket:N0} đ"
                        });
                    }
                }
            }
        }
        result.PersonalIncomeTax = personalIncomeTax;

        // f) Net = Gross - bảo hiểm - thuế. Làm tròn đến đồng
        result.NetSalary = Math.Round(result.GrossSalary - result.TotalInsuranceDeduction - result.PersonalIncomeTax, MidpointRounding.AwayFromZero);

        return result;
    }
}
