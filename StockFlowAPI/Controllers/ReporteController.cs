using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StockFlowAPI.Models.Request;
using StockFlowAPI.Models.Response;
using System.Data;

namespace StockFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Empleado,Consulta")]
    public class ReporteController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReporteController> _logger;


        public ReporteController(
            IConfiguration configuration,
            ILogger<ReporteController> logger)
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
        // VALIDAR FECHAS
        // =========================================================

        private IActionResult? ValidarFechas(
            ReporteFechasRequest request)
        {
            if (!request.FechaDesde.HasValue ||
                !request.FechaHasta.HasValue)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Debe indicar el rango de fechas."
                    }
                );
            }


            if (request.FechaDesde.Value.Date >
                request.FechaHasta.Value.Date)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "La fecha inicial no puede ser mayor que la fecha final."
                    }
                );
            }


            return null;
        }


        // =========================================================
        // REPORTE VENTAS
        // =========================================================

        [HttpPost]
        [Route("VentasAPI")]
        public IActionResult VentasAPI(
            ReporteFechasRequest request)
        {
            try
            {
                var validacion =
                    ValidarFechas(request);


                if (validacion != null)
                    return validacion;


                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                    return Unauthorized();


                var fechaDesde =
                    request.FechaDesde!.Value.Date;


                var fechaHasta =
                    request.FechaHasta!.Value.Date;


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var ventas =
                    connection.Query
                    <ReporteVentaResponse>(
                        "SP_ReporteVentas",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            FechaDesde =
                                fechaDesde,

                            FechaHasta =
                                fechaHasta
                        },
                        commandType:
                            CommandType.StoredProcedure
                    )
                    .ToList();


                return Ok(
                    ventas
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el reporte de ventas."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al generar el reporte de ventas."
                    }
                );
            }
        }


        // =========================================================
        // REPORTE COMPRAS
        // =========================================================

        [HttpPost]
        [Route("ComprasAPI")]
        public IActionResult ComprasAPI(
            ReporteFechasRequest request)
        {
            try
            {
                var validacion =
                    ValidarFechas(request);


                if (validacion != null)
                    return validacion;


                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                    return Unauthorized();


                var fechaDesde =
                    request.FechaDesde!.Value.Date;


                var fechaHasta =
                    request.FechaHasta!.Value.Date;


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var compras =
                    connection.Query
                    <ReporteCompraResponse>(
                        "SP_ReporteCompras",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            FechaDesde =
                                fechaDesde,

                            FechaHasta =
                                fechaHasta
                        },
                        commandType:
                            CommandType.StoredProcedure
                    )
                    .ToList();


                return Ok(
                    compras
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el reporte de compras."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al generar el reporte de compras."
                    }
                );
            }
        }


        // =========================================================
        // INVENTARIO ACTUAL
        // =========================================================

        [HttpGet]
        [Route("InventarioActualAPI")]
        public IActionResult InventarioActualAPI()
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                    return Unauthorized();


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var inventario =
                    connection.Query
                    <ReporteInventarioResponse>(
                        "SP_ReporteInventarioActual",
                        new
                        {
                            IdEmpresa =
                                idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    )
                    .ToList();


                return Ok(
                    inventario
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el reporte de inventario actual."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al consultar el inventario."
                    }
                );
            }
        }


        // =========================================================
        // STOCK BAJO
        // =========================================================

        [HttpGet]
        [Route("StockBajoAPI")]
        public IActionResult StockBajoAPI()
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                    return Unauthorized();


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var productos =
                    connection.Query
                    <ReporteStockBajoResponse>(
                        "SP_ReporteStockBajo",
                        new
                        {
                            IdEmpresa =
                                idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    )
                    .ToList();


                return Ok(
                    productos
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar productos con stock bajo."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al consultar el stock bajo."
                    }
                );
            }
        }


        // =========================================================
        // PRODUCTOS MÁS VENDIDOS
        // =========================================================

        [HttpPost]
        [Route("ProductosMasVendidosAPI")]
        public IActionResult ProductosMasVendidosAPI(
            ReporteFechasRequest request)
        {
            try
            {
                var validacion =
                    ValidarFechas(request);


                if (validacion != null)
                    return validacion;


                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                    return Unauthorized();


                var fechaDesde =
                    request.FechaDesde!.Value.Date;


                var fechaHasta =
                    request.FechaHasta!.Value.Date;


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var productos =
                    connection.Query
                    <ReporteProductoVendidoResponse>(
                        "SP_ReporteProductosMasVendidos",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            FechaDesde =
                                fechaDesde,

                            FechaHasta =
                                fechaHasta
                        },
                        commandType:
                            CommandType.StoredProcedure
                    )
                    .ToList();


                return Ok(
                    productos
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el reporte de productos más vendidos."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al generar el reporte."
                    }
                );
            }
        }
    }
}