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
    public class CompraController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CompraController> _logger;


        public CompraController(
            IConfiguration configuration,
            ILogger<CompraController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


        // =========================================================
        // ID EMPRESA DESDE JWT
        // =========================================================

        private long ObtenerIdEmpresa()
        {
            var claim =
                User.FindFirst("IdEmpresa")?.Value;

            return long.TryParse(
                claim,
                out var idEmpresa)
                ? idEmpresa
                : 0;
        }


        // =========================================================
        // ID USUARIO DESDE JWT
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
        // ERROR INTERNO DE STORED PROCEDURE
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
        // REGISTRAR COMPRA
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado")]
        [HttpPost]
        [Route("RegistrarCompraAPI")]
        public IActionResult RegistrarCompraAPI(
            RegistrarCompraRequest request)
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
                // VALIDACIONES
                // =================================================

                if (request.IdProveedor <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Debe seleccionar un proveedor."
                    });
                }


                if (request.PorcentajeImpuesto < 0 ||
                    request.PorcentajeImpuesto > 100)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El impuesto debe estar entre 0 y 100."
                    });
                }


                if (request.Detalles == null ||
                    request.Detalles.Count == 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Debe incluir al menos un producto."
                    });
                }


                if (request.Detalles.Any(
                    x =>
                        x.IdProducto <= 0 ||
                        x.Cantidad <= 0 ||
                        x.Cantidad != decimal.Truncate(
                            x.Cantidad
                        ) ||
                        x.PrecioUnitario <= 0))
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Los productos, cantidades enteras y costos de la compra no son válidos."
                    });
                }


                var tieneDuplicados =
                    request.Detalles
                        .GroupBy(x => x.IdProducto)
                        .Any(x => x.Count() > 1);


                if (tieneDuplicados)
                {
                    return Conflict(new
                    {
                        mensaje =
                            "No puede agregar el mismo producto más de una vez."
                    });
                }


                // =================================================
                // TVP - CompraDetalleType
                // =================================================

                var detalles =
                    new DataTable();


                detalles.Columns.Add(
                    "IdProducto",
                    typeof(long)
                );


                detalles.Columns.Add(
                    "Cantidad",
                    typeof(decimal)
                );


                detalles.Columns.Add(
                    "PrecioUnitario",
                    typeof(decimal)
                );


                foreach (var detalle in request.Detalles)
                {
                    detalles.Rows.Add(
                        detalle.IdProducto,
                        detalle.Cantidad,
                        detalle.PrecioUnitario
                    );
                }


                // =================================================
                // PARÁMETROS DEL SP
                // =================================================

                var parametros =
                    new DynamicParameters();


                parametros.Add(
                    "@IdEmpresa",
                    idEmpresa
                );


                parametros.Add(
                    "@IdProveedor",
                    request.IdProveedor
                );


                parametros.Add(
                    "@IdUsuario",
                    idUsuario
                );


                parametros.Add(
                    "@PorcentajeImpuesto",
                    request.PorcentajeImpuesto
                );


                parametros.Add(
                    "@Detalles",
                    detalles.AsTableValuedParameter(
                        "dbo.CompraDetalleType"
                    )
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
                    "@IdCompra",
                    dbType: DbType.Int64,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@NumeroCompra",
                    dbType: DbType.String,
                    size: 50,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@Subtotal",
                    dbType: DbType.Decimal,
                    direction:
                        ParameterDirection.Output,
                    precision: 18,
                    scale: 2
                );


                parametros.Add(
                    "@Impuesto",
                    dbType: DbType.Decimal,
                    direction:
                        ParameterDirection.Output,
                    precision: 18,
                    scale: 2
                );


                parametros.Add(
                    "@Total",
                    dbType: DbType.Decimal,
                    direction:
                        ParameterDirection.Output,
                    precision: 18,
                    scale: 2
                );


                // =================================================
                // EJECUTAR
                // =================================================

                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                connection.Execute(
                    "SP_RegistrarCompra",
                    parametros,
                    commandType:
                        CommandType.StoredProcedure
                );


                // =================================================
                // LEER OUTPUT
                // =================================================

                var resultado =
                    parametros.Get<int>(
                        "@Resultado"
                    );


                var mensaje =
                    parametros.Get<string>(
                        "@Mensaje"
                    );


                // =================================================
                // RESULTADO EXITOSO
                // =================================================

                if (resultado == 1)
                {
                    var response =
                        new RegistrarCompraResponse
                        {
                            IdCompra =
                                parametros.Get<long>(
                                    "@IdCompra"
                                ),

                            NumeroCompra =
                                parametros.Get<string>(
                                    "@NumeroCompra"
                                ),

                            Subtotal =
                                parametros.Get<decimal>(
                                    "@Subtotal"
                                ),

                            PorcentajeImpuesto =
                                request.PorcentajeImpuesto,

                            Impuesto =
                                parametros.Get<decimal>(
                                    "@Impuesto"
                                ),

                            Total =
                                parametros.Get<decimal>(
                                    "@Total"
                                ),

                            Mensaje =
                                mensaje
                        };


                    return StatusCode(
                        StatusCodes.Status201Created,
                        response
                    );
                }


                // =================================================
                // PROVEEDOR NO VÁLIDO
                // =================================================

                if (resultado == -1)
                {
                    return Conflict(new
                    {
                        mensaje
                    });
                }


                // =================================================
                // USUARIO / JWT INVÁLIDO
                // =================================================

                if (resultado == -2)
                {
                    return Unauthorized(new
                    {
                        mensaje
                    });
                }


                // =================================================
                // DATOS INVÁLIDOS
                // =================================================

                if (resultado == -3 ||
                    resultado == -4 ||
                    resultado == -6)
                {
                    return BadRequest(new
                    {
                        mensaje
                    });
                }


                // =================================================
                // PRODUCTO INVÁLIDO / INACTIVO / OTRA EMPRESA
                // =================================================

                if (resultado == -5)
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
                        "SP_RegistrarCompra",
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
                    "Error inesperado al registrar una compra."
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
        // CONSULTAR COMPRAS
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarComprasAPI")]
        public IActionResult ConsultarComprasAPI()
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


                var compras =
                    connection.Query<CompraResponse>(
                        "SP_ConsultarCompras",
                        new
                        {
                            IdEmpresa =
                                idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    ).ToList();


                return Ok(
                    compras
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar las compras."
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
        // CONSULTAR COMPRA
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarCompraAPI/{idCompra}")]
        public IActionResult ConsultarCompraAPI(
            long idCompra)
        {
            try
            {
                if (idCompra <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La compra no es válida."
                    });
                }


                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                    return Unauthorized();


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var compra =
                    connection.QueryFirstOrDefault
                    <CompraResponse>(
                        "SP_ConsultarCompra",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            IdCompra =
                                idCompra
                        },
                        commandType:
                            CommandType.StoredProcedure
                    );


                if (compra == null)
                {
                    return NotFound(new
                    {
                        mensaje =
                            "La compra no existe."
                    });
                }


                var detalles =
                    connection.Query<CompraDetalleResponse>(
                        "SP_ConsultarDetalleCompra",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            IdCompra =
                                idCompra
                        },
                        commandType:
                            CommandType.StoredProcedure
                    ).ToList();


                return Ok(
                    new CompraCompletaResponse
                    {
                        Compra =
                            compra,

                        Detalles =
                            detalles
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar la compra {IdCompra}.",
                    idCompra
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
        // PAGINACIÓN
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpPost]
        [Route("ConsultarComprasPaginadoAPI")]
        public IActionResult ConsultarComprasPaginadoAPI(
            ConsultarComprasPaginadoRequest request)
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                    return Unauthorized();


                if (request.Pagina < 1)
                    request.Pagina = 1;


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
                        "SP_ConsultarComprasPaginado",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            Busqueda =
                                busqueda,

                            IdProveedor =
                                request.IdProveedor,

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


                var compras =
                    resultados
                        .Read<CompraResponse>()
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
                    new PaginacionResponse<CompraResponse>
                    {
                        Registros =
                            compras,

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
                    "Error al consultar compras paginadas."
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
