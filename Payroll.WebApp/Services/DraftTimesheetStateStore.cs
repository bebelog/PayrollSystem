using Payroll.CoreBusiness.Enums;

namespace Payroll.WebApp.Services;

public class DraftDayItem
{
    public DateTime WorkDate { get; set; }
    public TimesheetDayStatus DayStatus { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Scoped State Store: Quản lý các ngày công nháp/chỉnh sửa tạm thời trước khi lưu (mô phỏng ShoppingCart trong eShop)
/// </summary>
public class DraftTimesheetStateStore
{
    private readonly Dictionary<DateTime, DraftDayItem> _draftDays = new();

    public event Action? OnChange;

    public bool HasDrafts => _draftDays.Count > 0;
    public int DraftCount => _draftDays.Count;

    public void SetDraftDay(DateTime date, TimesheetDayStatus status, string? note = null)
    {
        var key = date.Date;
        _draftDays[key] = new DraftDayItem
        {
            WorkDate = key,
            DayStatus = status,
            Note = note
        };
        NotifyStateChanged();
    }

    public DraftDayItem? GetDraftDay(DateTime date)
    {
        return _draftDays.TryGetValue(date.Date, out var item) ? item : null;
    }

    public IReadOnlyCollection<DraftDayItem> GetAllDraftDays()
    {
        return _draftDays.Values.ToList();
    }

    public void RemoveDraftDay(DateTime date)
    {
        if (_draftDays.Remove(date.Date))
        {
            NotifyStateChanged();
        }
    }

    public void ClearDraft()
    {
        _draftDays.Clear();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
