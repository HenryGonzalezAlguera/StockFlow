using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StockFlowAPI.Models.Response;
using System.Data;

namespace StockFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Empleado,Consulta")]
    public class DashboardController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DashboardController> _logger;


        public DashboardController(
            IConfiguration configuration,
            ILogger<DashboardController> logger)
        {
            _configuration =
                configuration;

            _logger =
                logger;
        }


        private long ObtenerIdEmpresa()
        {
            var claim =
                User.FindFirst(
                    "IdEmpresa"
                )?.Value;


            return long.TryParse(
                claim,
                out var idEmpresa)
                    ? idEmpresa
                    : 0;
        }


        private string ObtenerConnectionString()
        {
            return _configuration[
                "ConnectionStringsApi:DefaultConnection"
            ]
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión."
            );
        }


        // =========================================================
        // DASHBOARD
        // =========================================================

        [HttpGet]
        [Route("ConsultarDashboardAPI")]
        public IActionResult ConsultarDashboardAPI()
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                {
                    return Unauthorized(
                        new
                        {
                            mensaje =
                                "No se pudo identificar la empresa."
                        }
                    );
                }


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                using var resultados =
                    connection.QueryMultiple(
                        "SP_ConsultarDashboard",
                        new
                        {
                            IdEmpresa =
                                idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    );


                // =================================================
                // 1. RESUMEN
                // =================================================

                var resumen =
                    resultados
                        .ReadFirstOrDefault
                        <DashboardResumenResponse>()
                    ??
                    new DashboardResumenResponse();


                // =================================================
                // 2. STOCK BAJO
                // =================================================

                var productosStockBajo =
                    resultados
                        .Read
                        <ProductoStockBajoResponse>()
                        .ToList();


                // =================================================
                // 3. VENTAS ÚLTIMOS MESES
                // =================================================

                var ventasUltimosMeses =
                    resultados
                        .Read
                        <VentaMesResponse>()
                        .ToList();


                // =================================================
                // 4. PRODUCTOS MÁS VENDIDOS
                // =================================================

                var productosMasVendidos =
                    resultados
                        .Read
                        <ProductoMasVendidoResponse>()
                        .ToList();


                var response =
                    new DashboardResponse
                    {
                        Resumen =
                            resumen,

                        ProductosStockBajo =
                            productosStockBajo,

                        VentasUltimosMeses =
                            ventasUltimosMeses,

                        ProductosMasVendidos =
                            productosMasVendidos
                    };


                return Ok(
                    response
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar el dashboard."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al consultar el dashboard."
                    }
                );
            }
        }
    }
}