namespace Payroll.Plugins.DataStore.InMemory;

/// <summary>
/// Placeholder cho tuỳ chọn lưu trữ In-Memory phục vụ kiểm thử nhanh nếu không có SQL Server
/// </summary>
public class InMemoryDataStoreMarker
{
    public string Description => "In-Memory Plugin Provider for Testing";
}
