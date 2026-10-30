using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;


var builder =
    WebApplication.CreateBuilder(args);


// =========================================================
// JWT CONFIGURATION
// =========================================================

var secretKey =
    builder.Configuration[
        "Jwt:SecretKey"
    ];


var issuer =
    builder.Configuration[
        "Jwt:Issuer"
    ];


var audience =
    builder.Configuration[
        "Jwt:Audience"
    ];


if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException(
        "No se encontró Jwt:SecretKey."
    );
}


if (string.IsNullOrWhiteSpace(issuer))
{
    throw new InvalidOperationException(
        "No se encontró Jwt:Issuer."
    );
}


if (string.IsNullOrWhiteSpace(audience))
{
    throw new InvalidOperationException(
        "No se encontró Jwt:Audience."
    );
}


// =========================================================
// AUTHENTICATION
// =========================================================

builder.Services
    .AddAuthentication(
        options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        }
    )
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    // =============================================
                    // FIRMA
                    // =============================================

                    ValidateIssuerSigningKey =
                        true,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                secretKey
                            )
                        ),


                    // =============================================
                    // ISSUER
                    // =============================================

                    ValidateIssuer =
                        true,

                    ValidIssuer =
                        issuer,


                    // =============================================
                    // AUDIENCE
                    // =============================================

                    ValidateAudience =
                        true,

                    ValidAudience =
                        audience,


                    // =============================================
                    // EXPIRACIÓN
                    // =============================================

                    ValidateLifetime =
                        true,

                    ClockSkew =
                        TimeSpan.Zero,


                    // =============================================
                    // CLAIMS
                    // =============================================

                    NameClaimType =
                        ClaimTypes.Name,

                    RoleClaimType =
                        ClaimTypes.Role
                };


            // =============================================
            // RESPUESTAS 401 / 403 UNIFORMES
            // =============================================

            options.Events =
                new JwtBearerEvents
                {
                    OnChallenge =
                        async context =>
                        {
                            context.HandleResponse();

                            if (!context.Response.HasStarted)
                            {
                                context.Response.StatusCode =
                                    StatusCodes.Status401Unauthorized;

                                context.Response.ContentType =
                                    "application/json";

                                await context.Response
                                    .WriteAsJsonAsync(
                                        new
                                        {
                                            mensaje =
                                                "La sesión no es válida o ha expirado."
                                        }
                                    );
                            }
                        },


                    OnForbidden =
                        async context =>
                        {
                            if (!context.Response.HasStarted)
                            {
                                context.Response.StatusCode =
                                    StatusCodes.Status403Forbidden;

                                context.Response.ContentType =
                                    "application/json";

                                await context.Response
                                    .WriteAsJsonAsync(
                                        new
                                        {
                                            mensaje =
                                                "No tiene permisos para realizar esta operación."
                                        }
                                    );
                            }
                        }
                };
        }
    );


// =========================================================
// AUTHORIZATION
// =========================================================

builder.Services.AddAuthorization();


// =========================================================
// RATE LIMITING - ENDPOINTS PÚBLICOS
// =========================================================

builder.Services.AddRateLimiter(
    options =>
    {
        options.RejectionStatusCode =
            StatusCodes.Status429TooManyRequests;


        options.OnRejected =
            async (context, cancellationToken) =>
            {
                context.HttpContext
                    .Response.ContentType =
                        "application/json";

                await context.HttpContext
                    .Response.WriteAsJsonAsync(
                        new
                        {
                            mensaje =
                                "Se realizaron demasiadas solicitudes. Intente nuevamente más tarde."
                        },
                        cancellationToken
                    );
            };


        options.AddPolicy(
            "LoginPublico",
            httpContext =>
                RateLimitPartition
                    .GetFixedWindowLimiter(
                        partitionKey:
                            httpContext.Connection
                                .RemoteIpAddress?
                                .ToString()
                            ??
                            "sin-ip",

                        factory:
                            _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 30,

                                    Window =
                                        TimeSpan.FromMinutes(
                                            1
                                        ),

                                    QueueLimit = 0,

                                    AutoReplenishment =
                                        true
                                }
                    )
        );


        options.AddPolicy(
            "RegistroEmpresaPublico",
            httpContext =>
                RateLimitPartition
                    .GetFixedWindowLimiter(
                        partitionKey:
                            httpContext.Connection
                                .RemoteIpAddress?
                                .ToString()
                            ??
                            "sin-ip",

                        factory:
                            _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 5,

                                    Window =
                                        TimeSpan.FromHours(
                                            1
                                        ),

                                    QueueLimit = 0,

                                    AutoReplenishment =
                                        true
                                }
                    )
        );
    }
);


// =========================================================
// CONTROLLERS
//
// Personalizamos los errores automáticos de DataAnnotations.
// =========================================================

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(
        options =>
        {
            options.InvalidModelStateResponseFactory =
                context =>
                {
                    var errores =
                        context.ModelState
                            .Where(
                                x =>
                                    x.Value != null &&
                                    x.Value.Errors.Count > 0
                            )
                            .SelectMany(
                                x => x.Value!.Errors
                            )
                            .Select(
                                x =>
                                    string.IsNullOrWhiteSpace(
                                        x.ErrorMessage
                                    )
                                        ? "Uno o más datos enviados no son válidos."
                                        : x.ErrorMessage
                            )
                            .Distinct()
                            .ToList();


                    return new BadRequestObjectResult(
                        new
                        {
                            mensaje =
                                errores.FirstOrDefault()
                                ??
                                "Uno o más datos enviados no son válidos.",

                            errores
                        }
                    );
                };
        }
    );


// =========================================================
// OPEN API
// =========================================================

builder.Services.AddOpenApi();


// =========================================================
// BUILD
// =========================================================

var app =
    builder.Build();


// =========================================================
// OPEN API SOLO DEVELOPMENT
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// =========================================================
// PIPELINE
// =========================================================

app.UseHttpsRedirection();


// Rate limiting antes de ejecutar endpoints.

app.UseRateLimiter();


// JWT:
// siempre Authentication antes de Authorization.

app.UseAuthentication();

app.UseAuthorization();


// =========================================================
// CONTROLLERS
// =========================================================

app.MapControllers();


app.Run();
