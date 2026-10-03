using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Payroll.CoreBusiness.Enums;
using Payroll.CoreBusiness.Exceptions;
using Payroll.Plugins; // DUY NHẤT Program.cs được using Payroll.Plugins theo luật Clean Architecture
using Payroll.UseCases.Accounts;
using Payroll.UseCases.Configurations;
using Payroll.UseCases.Employees;
using Payroll.UseCases.Payrolls;
using Payroll.UseCases.Timesheets;
using Payroll.WebApp.Components;
using Payroll.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Chuỗi kết nối CSDL (Đổi 1 dòng duy nhất để đổi plugin CSDL)
string connectionString = builder.Configuration.GetConnectionString("PayrollConnection") 
    ?? "Server=localhost;Database=PayrollDB;Trusted_Connection=True;TrustServerCertificate=True;";

// 2. Composition Root: Đăng ký Plugin CSDL (Dapper SQL Server)
builder.Services.AddPayrollSqlPlugins(connectionString);

// 3. Đăng ký Use Cases
builder.Services.AddScoped<IViewEmployeesUseCase, ViewEmployeesUseCase>();
builder.Services.AddScoped<IGetEmployeeByIdUseCase, GetEmployeeByIdUseCase>();
builder.Services.AddScoped<ICreateEmployeeUseCase, CreateEmployeeUseCase>();
builder.Services.AddScoped<IUpdateEmployeeUseCase, UpdateEmployeeUseCase>();
builder.Services.AddScoped<IViewDepartmentsUseCase, ViewDepartmentsUseCase>();

builder.Services.AddScoped<IRecordAttendanceTodayUseCase, RecordAttendanceTodayUseCase>();
builder.Services.AddScoped<IViewMyTimesheetUseCase, ViewMyTimesheetUseCase>();
builder.Services.AddScoped<ISubmitMyTimesheetUseCase, SubmitMyTimesheetUseCase>();
builder.Services.AddScoped<IApproveTimesheetUseCase, ApproveTimesheetUseCase>();
builder.Services.AddScoped<IViewAllTimesheetsUseCase, ViewAllTimesheetsUseCase>();
builder.Services.AddScoped<IRevertTimesheetUseCase, RevertTimesheetUseCase>();
builder.Services.AddScoped<IAdjustTimesheetEntryUseCase, AdjustTimesheetEntryUseCase>();

builder.Services.AddScoped<ICreatePayrollPeriodUseCase, CreatePayrollPeriodUseCase>();
builder.Services.AddScoped<ICalculatePayrollPeriodUseCase, CalculatePayrollPeriodUseCase>();
builder.Services.AddScoped<IClosePayrollPeriodUseCase, ClosePayrollPeriodUseCase>();
builder.Services.AddScoped<IViewPayrollPeriodsUseCase, ViewPayrollPeriodsUseCase>();
builder.Services.AddScoped<IViewMyPayslipUseCase, ViewMyPayslipUseCase>();
builder.Services.AddScoped<IViewPayslipsByPeriodUseCase, ViewPayslipsByPeriodUseCase>();
builder.Services.AddScoped<IExportPayrollToExcelUseCase, ExportPayrollToExcelUseCase>();

builder.Services.AddScoped<IViewPayrollSettingsUseCase, ViewPayrollSettingsUseCase>();
builder.Services.AddScoped<IUpdatePayrollSettingsUseCase, UpdatePayrollSettingsUseCase>();

builder.Services.AddScoped<ILoginUserUseCase, LoginUserUseCase>();
builder.Services.AddScoped<IGetCurrentUserUseCase, GetCurrentUserUseCase>();

// 4. Scoped State Store (Quản lý bảng công nháp như ShoppingCart trong eShop)
builder.Services.AddScoped<DraftTimesheetStateStore>();

// 5. Cấu hình Cookie Authentication (Xác thực Cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Payroll.AuthCookie";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// 6. Blazor Server Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// =========================================================================================
// MINIMAL API ENDPOINTS CHO ĐĂNG NHẬP / ĐĂNG XUẤT (Giải quyết hạn chế WebSocket circuit)
// =========================================================================================
app.MapPost("/api/account/login", async (
    HttpContext httpContext,
    [FromForm] string username,
    [FromForm] string password,
    [FromServices] ILoginUserUseCase loginUserUseCase) =>
{
    try
    {
        var user = await loginUserUseCase.ExecuteAsync(username, password);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role == UserRole.Accountant ? "Accountant" : "Employee"),
            new("RoleName", user.Role == UserRole.Accountant ? "Kế toán" : "Nhân viên")
        };

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));
            if (user.Employee != null)
            {
                claims.Add(new Claim("FullName", user.Employee.FullName));
            }
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return Results.Redirect("/");
    }
    catch (BusinessRuleException brex)
    {
        return Results.Redirect($"/login?error={Uri.EscapeDataString(brex.Message)}");
    }
    catch (Exception ex)
    {
        return Results.Redirect($"/login?error={Uri.EscapeDataString("Lỗi đăng nhập: " + ex.Message)}");
    }
}).DisableAntiforgery();

app.MapGet("/api/account/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
