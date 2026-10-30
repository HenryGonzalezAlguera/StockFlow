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
    public class VentaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VentaController> _logger;


        public VentaController(
            IConfiguration configuration,
            ILogger<VentaController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


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


        private string ObtenerConnectionString()
        {
            return _configuration[
                "ConnectionStringsApi:DefaultConnection"
            ]
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión."
            );
        }


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
        // REGISTRAR VENTA
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado")]
        [HttpPost]
        [Route("RegistrarVentaAPI")]
        public IActionResult RegistrarVentaAPI(
            RegistrarVentaRequest request)
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


                if (request.IdCliente.HasValue &&
                    request.IdCliente.Value <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El cliente seleccionado no es válido."
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
                        )))
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Los productos y cantidades enteras de la venta no son válidos."
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
                // TVP
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


                foreach (var detalle in request.Detalles)
                {
                    detalles.Rows.Add(
                        detalle.IdProducto,
                        detalle.Cantidad
                    );
                }


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
                    "@IdCliente",
                    request.IdCliente
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
                        "dbo.VentaDetalleType"
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
                    "@IdVenta",
                    dbType: DbType.Int64,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@NumeroVenta",
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


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                connection.Execute(
                    "SP_RegistrarVenta",
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
                        new RegistrarVentaResponse
                        {
                            IdVenta =
                                parametros.Get<long>(
                                    "@IdVenta"
                                ),

                            NumeroVenta =
                                parametros.Get<string>(
                                    "@NumeroVenta"
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


                // Usuario inválido.
                if (resultado == -1)
                {
                    return Unauthorized(new
                    {
                        mensaje
                    });
                }


                // Cliente no válido.
                if (resultado == -2)
                {
                    return Conflict(new
                    {
                        mensaje
                    });
                }


                // Detalles, cantidades o impuesto.
                if (resultado == -3 ||
                    resultado == -4 ||
                    resultado == -5)
                {
                    return BadRequest(new
                    {
                        mensaje
                    });
                }


                // Producto inválido.
                if (resultado == -6)
                {
                    return Conflict(new
                    {
                        mensaje
                    });
                }


                // Stock insuficiente.
                if (resultado == -7)
                {
                    return Conflict(new
                    {
                        mensaje
                    });
                }


                // Configuración interna.
                if (resultado == -8 ||
                    resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_RegistrarVenta",
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
                    "Error inesperado al registrar una venta."
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
        // CONSULTAR VENTAS
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarVentasAPI")]
        public IActionResult ConsultarVentasAPI()
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


                var ventas =
                    connection.Query<VentaResponse>(
                        "SP_ConsultarVentas",
                        new
                        {
                            IdEmpresa =
                                idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    ).ToList();


                return Ok(
                    ventas
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar las ventas."
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
        // CONSULTAR VENTA
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarVentaAPI/{idVenta}")]
        public IActionResult ConsultarVentaAPI(
            long idVenta)
        {
            try
            {
                if (idVenta <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La venta no es válida."
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


                var venta =
                    connection.QueryFirstOrDefault
                    <VentaResponse>(
                        "SP_ConsultarVenta",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            IdVenta =
                                idVenta
                        },
                        commandType:
                            CommandType.StoredProcedure
                    );


                if (venta == null)
                {
                    return NotFound(new
                    {
                        mensaje =
                            "La venta no existe."
                    });
                }


                var detalles =
                    connection.Query<VentaDetalleResponse>(
                        "SP_ConsultarDetalleVenta",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            IdVenta =
                                idVenta
                        },
                        commandType:
                            CommandType.StoredProcedure
                    ).ToList();


                return Ok(
                    new VentaCompletaResponse
                    {
                        Venta =
                            venta,

                        Detalles =
                            detalles
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar la venta {IdVenta}.",
                    idVenta
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
        [Route("ConsultarVentasPaginadoAPI")]
        public IActionResult ConsultarVentasPaginadoAPI(
            ConsultarVentasPaginadoRequest request)
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
                        "SP_ConsultarVentasPaginado",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            Busqueda =
                                busqueda,

                            IdCliente =
                                request.IdCliente,

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


                var ventas =
                    resultados
                        .Read<VentaResponse>()
                        .ToList();


                var totalRegistros =
                    resultados
                        .ReadFirst<int>();


                return Ok(
                    new PaginacionResponse<VentaResponse>
                    {
                        Registros =
                            ventas,

                        PaginaActual =
                            request.Pagina,

                        CantidadPagina =
                            request.CantidadPagina,

                        TotalRegistros =
                            totalRegistros,

                        TotalPaginas =
                            (int)Math.Ceiling(
                                totalRegistros /
                                (double)request.CantidadPagina
                            )
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar ventas paginadas."
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
