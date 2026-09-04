var builder =
    WebApplication.CreateBuilder(args);


// =========================================================
// MVC
// =========================================================

builder.Services
    .AddControllersWithViews();


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
            TimeSpan.FromSeconds(30);
    }
);


// =========================================================
// SESSION
// =========================================================

builder.Services.AddSession(
    options =>
    {
        // =================================================
        // TIEMPO DE INACTIVIDAD
        // =================================================

        options.IdleTimeout =
            TimeSpan.FromMinutes(30);


        // =================================================
        // JAVASCRIPT NO PUEDE LEER COOKIE
        // =================================================

        options.Cookie.HttpOnly =
            true;


        // =================================================
        // COOKIE NECESARIA
        // =================================================

        options.Cookie.IsEssential =
            true;


        // =================================================
        // CSRF
        // =================================================

        options.Cookie.SameSite =
            SameSiteMode.Lax;


        // =================================================
        // HTTPS
        //
        // SameAsRequest nos permite desarrollo localhost.
        // En deployment revisaremos esto nuevamente.
        // =================================================

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;


        // =================================================
        // NOMBRE COOKIE
        // =================================================

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
    // =====================================================
    // EXCEPCIONES NO CONTROLADAS
    // =====================================================

    app.UseExceptionHandler(
        "/Home/Error"
    );


    // =====================================================
    // HSTS
    // =====================================================

    app.UseHsts();
}


// =========================================================
// STATUS CODES
//
// Ejemplo:
//
// /pagina-que-no-existe
//
// 404
//   ↓
// /Home/Estado/404
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
//
// Debe ejecutarse antes de los controladores.
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