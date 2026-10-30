using QuestPDF.Infrastructure;
using StockFlowWEB.Services;

var builder =
    WebApplication.CreateBuilder(args);


// =========================================================
// QUESTPDF
// =========================================================

QuestPDF.Settings.License =
    LicenseType.Community;


// =========================================================
// MVC
// =========================================================

builder.Services
    .AddControllersWithViews();


// =========================================================
// PDF SERVICES
// =========================================================

builder.Services
    .AddScoped<ReportePdfService>();


// =========================================================
// URL API
// =========================================================

var apiBaseUrl =
    builder.Configuration[
        "ApiSettings:BaseUrl"
    ];


if (string.IsNullOrWhiteSpace(
    apiBaseUrl))
{
    throw new InvalidOperationException(
        "No se encontró ApiSettings:BaseUrl en appsettings.json."
    );
}


// =========================================================
// HTTP CLIENT - STOCKFLOW API
// =========================================================

builder.Services.AddHttpClient(
    "StockFlowAPI",
    client =>
    {
        client.BaseAddress =
            new Uri(
                apiBaseUrl
            );

        client.Timeout =
            TimeSpan.FromSeconds(120);
    }
);


// =========================================================
// SESSION
// =========================================================

builder.Services.AddSession(
    options =>
    {
        options.IdleTimeout =
            TimeSpan.FromMinutes(30);

        options.Cookie.HttpOnly =
            true;

        options.Cookie.IsEssential =
            true;

        options.Cookie.SameSite =
            SameSiteMode.Lax;

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;

        options.Cookie.Name =
            ".StockFlow.Session";
    }
);


// =========================================================
// HTTP CONTEXT
// =========================================================

builder.Services
    .AddHttpContextAccessor();


// =========================================================
// BUILD
// =========================================================

var app =
    builder.Build();


// =========================================================
// ERRORES DE PRODUCCIÓN
// =========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error"
    );

    app.UseHsts();
}


// =========================================================
// STATUS CODES
// =========================================================

app.UseStatusCodePagesWithReExecute(
    "/Home/Estado/{0}"
);


// =========================================================
// HTTPS
// =========================================================

app.UseHttpsRedirection();


// =========================================================
// ARCHIVOS ESTÁTICOS
// =========================================================

app.UseStaticFiles();


// =========================================================
// ROUTING
// =========================================================

app.UseRouting();


// =========================================================
// SESSION
// =========================================================

app.UseSession();


// =========================================================
// AUTHORIZATION
// =========================================================

app.UseAuthorization();


// =========================================================
// MVC
// =========================================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}"
);


// =========================================================
// RUN
// =========================================================

app.Run();