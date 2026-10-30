using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StockFlowAPI.Models.Request;
using StockFlowAPI.Models.Response;
using System.Data;
using System.Security.Claims;

namespace StockFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventarioController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<InventarioController> _logger;


        public InventarioController(
            IConfiguration configuration,
            ILogger<InventarioController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


        // =========================================================
        // ID EMPRESA
        // =========================================================

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


        // =========================================================
        // ID USUARIO
        // =========================================================

        private long ObtenerIdUsuario()
        {
            var claim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier
                )?.Value;


            return long.TryParse(
                claim,
                out var idUsuario)
                    ? idUsuario
                    : 0;
        }


        // =========================================================
        // CONNECTION STRING
        // =========================================================

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
        // ERROR INTERNO SP
        // =========================================================

        private IActionResult ErrorInternoSP(
            string procedimiento,
            string mensajeSql)
        {
            _logger.LogError(
                "El procedimiento {Procedimiento} devolvió un error interno: {Mensaje}",
                procedimiento,
                mensajeSql
            );


            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensaje =
                        "Ocurrió un error interno al procesar la solicitud."
                }
            );
        }


        // =========================================================
        // CONSULTAR TIPOS
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarTiposMovimientoAPI")]
        public IActionResult ConsultarTiposMovimientoAPI()
        {
            try
            {
                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var tipos =
                    connection.Query
                    <TipoMovimientoResponse>(
                        "SP_ConsultarTiposMovimiento",
                        commandType:
                            CommandType.StoredProcedure
                    )
                    .ToList();


                return Ok(
                    tipos
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar los tipos de movimiento."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al procesar la solicitud."
                    }
                );
            }
        }


        // =========================================================
        // CONSULTAR MOVIMIENTOS
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpPost]
        [Route("ConsultarMovimientosAPI")]
        public IActionResult ConsultarMovimientosAPI(
            ConsultarMovimientosRequest request)
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                {
                    return Unauthorized(new
                    {
                        mensaje =
                            "No se pudo identificar la empresa."
                    });
                }


                if (request.FechaDesde.HasValue &&
                    request.FechaHasta.HasValue &&
                    request.FechaDesde.Value.Date >
                    request.FechaHasta.Value.Date)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La fecha inicial no puede ser mayor que la fecha final."
                    });
                }


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var movimientos =
                    connection.Query
                    <MovimientoInventarioResponse>(
                        "SP_ConsultarMovimientosInventario",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            IdProducto =
                                request.IdProducto,

                            IdTipoMovimiento =
                                request.IdTipoMovimiento,

                            FechaDesde =
                                request.FechaDesde?.Date,

                            FechaHasta =
                                request.FechaHasta?.Date
                        },
                        commandType:
                            CommandType.StoredProcedure
                    )
                    .ToList();


                return Ok(
                    movimientos
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar movimientos de inventario."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al procesar la solicitud."
                    }
                );
            }
        }


        // =========================================================
        // MOVIMIENTOS PAGINADOS
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpPost]
        [Route("ConsultarMovimientosPaginadoAPI")]
        public IActionResult ConsultarMovimientosPaginadoAPI(
            ConsultarMovimientosPaginadoRequest request)
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                {
                    return Unauthorized(new
                    {
                        mensaje =
                            "No se pudo identificar la empresa."
                    });
                }


                if (request.Pagina < 1)
                {
                    request.Pagina = 1;
                }


                if (request.CantidadPagina != 10 &&
                    request.CantidadPagina != 20 &&
                    request.CantidadPagina != 50)
                {
                    request.CantidadPagina = 10;
                }


                if (request.FechaDesde.HasValue &&
                    request.FechaHasta.HasValue &&
                    request.FechaDesde.Value.Date >
                    request.FechaHasta.Value.Date)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La fecha inicial no puede ser mayor que la fecha final."
                    });
                }


                var busqueda =
                    string.IsNullOrWhiteSpace(
                        request.Busqueda
                    )
                        ? null
                        : request.Busqueda.Trim();


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                using var resultados =
                    connection.QueryMultiple(
                        "SP_ConsultarMovimientosInventarioPaginado",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            Busqueda =
                                busqueda,

                            IdProducto =
                                request.IdProducto,

                            IdTipoMovimiento =
                                request.IdTipoMovimiento,

                            FechaDesde =
                                request.FechaDesde?.Date,

                            FechaHasta =
                                request.FechaHasta?.Date,

                            Pagina =
                                request.Pagina,

                            CantidadPagina =
                                request.CantidadPagina
                        },
                        commandType:
                            CommandType.StoredProcedure
                    );


                var movimientos =
                    resultados
                        .Read<MovimientoInventarioResponse>()
                        .ToList();


                var totalRegistros =
                    resultados
                        .ReadFirst<int>();


                var totalPaginas =
                    (int)Math.Ceiling(
                        totalRegistros /
                        (double)request.CantidadPagina
                    );


                return Ok(
                    new PaginacionResponse
                    <MovimientoInventarioResponse>
                    {
                        Registros =
                            movimientos,

                        PaginaActual =
                            request.Pagina,

                        CantidadPagina =
                            request.CantidadPagina,

                        TotalRegistros =
                            totalRegistros,

                        TotalPaginas =
                            totalPaginas
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar movimientos paginados."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al procesar la solicitud."
                    }
                );
            }
        }


        // =========================================================
        // AJUSTAR INVENTARIO
        // SOLO ADMINISTRADOR
        // =========================================================

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [Route("AjustarInventarioAPI")]
        public IActionResult AjustarInventarioAPI(
            AjustarInventarioRequest request)
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                var idUsuario =
                    ObtenerIdUsuario();


                if (idEmpresa <= 0 ||
                    idUsuario <= 0)
                {
                    return Unauthorized(new
                    {
                        mensaje =
                            "No fue posible identificar la sesión del usuario."
                    });
                }


                // =================================================
                // NORMALIZAR TIPO
                // =================================================

                var tipoAjuste =
                    request.TipoAjuste?
                        .Trim()
                        .ToUpperInvariant()
                    ?? string.Empty;


                if (tipoAjuste != "ENTRADA" &&
                    tipoAjuste != "SALIDA")
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El tipo de ajuste debe ser ENTRADA o SALIDA."
                    });
                }


                if (request.IdProducto <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Debe seleccionar un producto."
                    });
                }


                if (request.Cantidad <= 0 ||
                    request.Cantidad != decimal.Truncate(
                        request.Cantidad
                    ))
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La cantidad debe ser un número entero mayor a cero."
                    });
                }


                if (string.IsNullOrWhiteSpace(
                    request.Motivo))
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Debe indicar el motivo del ajuste."
                    });
                }


                var motivo =
                    request.Motivo.Trim();


                // =================================================
                // PARÁMETROS
                // =================================================

                var parametros =
                    new DynamicParameters();


                parametros.Add(
                    "@IdEmpresa",
                    idEmpresa
                );


                parametros.Add(
                    "@IdProducto",
                    request.IdProducto
                );


                parametros.Add(
                    "@IdUsuario",
                    idUsuario
                );


                parametros.Add(
                    "@TipoAjuste",
                    tipoAjuste
                );


                parametros.Add(
                    "@Cantidad",
                    request.Cantidad
                );


                parametros.Add(
                    "@Motivo",
                    motivo
                );


                // =================================================
                // OUTPUT
                // =================================================

                parametros.Add(
                    "@Resultado",
                    dbType: DbType.Int32,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@Mensaje",
                    dbType: DbType.String,
                    size: 250,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@IdMovimiento",
                    dbType: DbType.Int64,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@StockAnterior",
                    dbType: DbType.Decimal,
                    direction:
                        ParameterDirection.Output,
                    precision: 18,
                    scale: 3
                );


                parametros.Add(
                    "@StockNuevo",
                    dbType: DbType.Decimal,
                    direction:
                        ParameterDirection.Output,
                    precision: 18,
                    scale: 3
                );


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                connection.Execute(
                    "SP_AjustarInventario",
                    parametros,
                    commandType:
                        CommandType.StoredProcedure
                );


                var resultado =
                    parametros.Get<int>(
                        "@Resultado"
                    );


                var mensaje =
                    parametros.Get<string>(
                        "@Mensaje"
                    );


                // =================================================
                // ÉXITO
                // =================================================

                if (resultado == 1)
                {
                    var response =
                        new AjusteInventarioResponse
                        {
                            IdMovimiento =
                                parametros.Get<long>(
                                    "@IdMovimiento"
                                ),

                            IdProducto =
                                request.IdProducto,

                            TipoAjuste =
                                tipoAjuste,

                            Cantidad =
                                request.Cantidad,

                            StockAnterior =
                                parametros.Get<decimal>(
                                    "@StockAnterior"
                                ),

                            StockNuevo =
                                parametros.Get<decimal>(
                                    "@StockNuevo"
                                ),

                            Motivo =
                                motivo,

                            Mensaje =
                                mensaje
                        };


                    return Ok(
                        response
                    );
                }


                // =================================================
                // REQUEST INVÁLIDO
                // =================================================

                if (resultado == -1 ||
                    resultado == -2 ||
                    resultado == -3)
                {
                    return BadRequest(new
                    {
                        mensaje
                    });
                }


                // =================================================
                // USUARIO INVÁLIDO
                // =================================================

                if (resultado == -4)
                {
                    return Unauthorized(new
                    {
                        mensaje
                    });
                }


                // =================================================
                // PRODUCTO INVÁLIDO
                // =================================================

                if (resultado == -5)
                {
                    return Conflict(new
                    {
                        mensaje
                    });
                }


                // =================================================
                // STOCK INSUFICIENTE
                // =================================================

                if (resultado == -6)
                {
                    return Conflict(new
                    {
                        mensaje
                    });
                }


                // =================================================
                // CONFIGURACIÓN INTERNA
                // =================================================

                if (resultado == -7 ||
                    resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_AjustarInventario",
                        mensaje
                    );
                }


                return BadRequest(new
                {
                    mensaje
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al realizar un ajuste de inventario."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al procesar la solicitud."
                    }
                );
            }
        }
    }
}
