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
    [Authorize]
    public class ProductoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductoController> _logger;

        public ProductoController(
            IConfiguration configuration,
            ILogger<ProductoController> logger)
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

        private string ObtenerConnectionString()
        {
            return _configuration[
                "ConnectionStringsApi:DefaultConnection"
            ] ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión."
            );
        }

        private static string? NormalizarOpcional(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
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
        // CONSULTAR PRODUCTOS
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarProductosAPI")]
        public IActionResult ConsultarProductosAPI()
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
                    connection.Query<ProductoResponse>(
                        "SP_ConsultarProductos",
                        new
                        {
                            IdEmpresa = idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    ).ToList();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar productos."
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
        // CONSULTAR PRODUCTO
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarProductoAPI/{idProducto}")]
        public IActionResult ConsultarProductoAPI(
            long idProducto)
        {
            try
            {
                if (idProducto <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El producto no es válido."
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

                var producto =
                    connection.QueryFirstOrDefault
                    <ProductoResponse>(
                        "SP_ConsultarProducto",
                        new
                        {
                            IdProducto =
                                idProducto,

                            IdEmpresa =
                                idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    );

                if (producto == null)
                {
                    return NotFound(new
                    {
                        mensaje =
                            "El producto no existe."
                    });
                }

                return Ok(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar producto {IdProducto}.",
                    idProducto
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
        // REGISTRAR PRODUCTO
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado")]
        [HttpPost]
        [Route("RegistrarProductoAPI")]
        public IActionResult RegistrarProductoAPI(
            RegistrarProductoRequest request)
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();

                if (idEmpresa <= 0)
                    return Unauthorized();

                var parametros =
                    new DynamicParameters();

                parametros.Add(
                    "@IdEmpresa",
                    idEmpresa
                );

                parametros.Add(
                    "@IdCategoria",
                    request.IdCategoria
                );

                parametros.Add(
                    "@Codigo",
                    request.Codigo.Trim()
                );

                parametros.Add(
                    "@Nombre",
                    request.Nombre.Trim()
                );

                parametros.Add(
                    "@Descripcion",
                    NormalizarOpcional(
                        request.Descripcion
                    )
                );

                parametros.Add(
                    "@PrecioCompra",
                    request.PrecioCompra
                );

                parametros.Add(
                    "@PrecioVenta",
                    request.PrecioVenta
                );

                parametros.Add(
                    "@StockMinimo",
                    request.StockMinimo
                );

                parametros.Add(
                    "@Resultado",
                    dbType: DbType.Int32,
                    direction:
                        ParameterDirection.Output
                );

                parametros.Add(
                    "@Mensaje",
                    dbType: DbType.String,
                    size: 200,
                    direction:
                        ParameterDirection.Output
                );

                parametros.Add(
                    "@IdProducto",
                    dbType: DbType.Int64,
                    direction:
                        ParameterDirection.Output
                );

                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );

                connection.Execute(
                    "SP_RegistrarProducto",
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

                var idProducto =
                    parametros.Get<long>(
                        "@IdProducto"
                    );

                if (resultado == 1)
                {
                    return StatusCode(
                        StatusCodes.Status201Created,
                        new
                        {
                            idProducto,
                            mensaje
                        }
                    );
                }

                if (resultado == -1)
                {
                    return Conflict(new
                    {
                        mensaje
                    });
                }

                if (resultado == -2)
                {
                    return Conflict(new
                    {
                        mensaje
                    });
                }

                if (resultado == -3)
                {
                    return BadRequest(new
                    {
                        mensaje
                    });
                }

                if (resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_RegistrarProducto",
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
                    "Error inesperado al registrar producto."
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
        // MODIFICAR PRODUCTO
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado")]
        [HttpPut]
        [Route("ModificarProductoAPI")]
        public IActionResult ModificarProductoAPI(
            ModificarProductoRequest request)
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();

                if (idEmpresa <= 0)
                    return Unauthorized();

                var parametros =
                    new DynamicParameters();

                parametros.Add(
                    "@IdProducto",
                    request.IdProducto
                );

                parametros.Add(
                    "@IdEmpresa",
                    idEmpresa
                );

                parametros.Add(
                    "@IdCategoria",
                    request.IdCategoria
                );

                parametros.Add(
                    "@Codigo",
                    request.Codigo.Trim()
                );

                parametros.Add(
                    "@Nombre",
                    request.Nombre.Trim()
                );

                parametros.Add(
                    "@Descripcion",
                    NormalizarOpcional(
                        request.Descripcion
                    )
                );

                parametros.Add(
                    "@PrecioCompra",
                    request.PrecioCompra
                );

                parametros.Add(
                    "@PrecioVenta",
                    request.PrecioVenta
                );

                parametros.Add(
                    "@StockMinimo",
                    request.StockMinimo
                );

                parametros.Add(
                    "@Resultado",
                    dbType: DbType.Int32,
                    direction:
                        ParameterDirection.Output
                );

                parametros.Add(
                    "@Mensaje",
                    dbType: DbType.String,
                    size: 200,
                    direction:
                        ParameterDirection.Output
                );

                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );

                connection.Execute(
                    "SP_ModificarProducto",
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

                if (resultado == 1)
                {
                    return Ok(new
                    {
                        mensaje
                    });
                }

                if (resultado == -1)
                {
                    return NotFound(new
                    {
                        mensaje
                    });
                }

                if (resultado == -2)
                {
                    return Conflict(new
                    {
                        mensaje
                    });
                }

                if (resultado == -3)
                {
                    return Conflict(new
                    {
                        mensaje
                    });
                }

                if (resultado == -4)
                {
                    return BadRequest(new
                    {
                        mensaje
                    });
                }

                if (resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_ModificarProducto",
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
                    "Error inesperado al modificar producto {IdProducto}.",
                    request.IdProducto
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
        // CAMBIAR ESTADO
        // =========================================================

        [Authorize(Roles = "Administrador")]
        [HttpPut]
        [Route("CambiarEstadoProductoAPI/{idProducto}")]
        public IActionResult CambiarEstadoProductoAPI(
            long idProducto)
        {
            try
            {
                if (idProducto <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El producto no es válido."
                    });
                }

                var idEmpresa =
                    ObtenerIdEmpresa();

                if (idEmpresa <= 0)
                    return Unauthorized();

                var parametros =
                    new DynamicParameters();

                parametros.Add(
                    "@IdProducto",
                    idProducto
                );

                parametros.Add(
                    "@IdEmpresa",
                    idEmpresa
                );

                parametros.Add(
                    "@Resultado",
                    dbType: DbType.Int32,
                    direction:
                        ParameterDirection.Output
                );

                parametros.Add(
                    "@Mensaje",
                    dbType: DbType.String,
                    size: 200,
                    direction:
                        ParameterDirection.Output
                );

                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );

                connection.Execute(
                    "SP_CambiarEstadoProducto",
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

                if (resultado == 1)
                {
                    return Ok(new
                    {
                        mensaje
                    });
                }

                if (resultado == -1)
                {
                    return NotFound(new
                    {
                        mensaje
                    });
                }

                if (resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_CambiarEstadoProducto",
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
                    "Error inesperado al cambiar estado de producto {IdProducto}.",
                    idProducto
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
        [Route("ConsultarProductosPaginadoAPI")]
        public IActionResult ConsultarProductosPaginadoAPI(
            ConsultarProductosPaginadoRequest request)
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

                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );

                using var resultados =
                    connection.QueryMultiple(
                        "SP_ConsultarProductosPaginado",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            Busqueda =
                                NormalizarOpcional(
                                    request.Busqueda
                                ),

                            IdCategoria =
                                request.IdCategoria,

                            Estado =
                                request.Estado,

                            Pagina =
                                request.Pagina,

                            CantidadPagina =
                                request.CantidadPagina
                        },
                        commandType:
                            CommandType.StoredProcedure
                    );

                var productos =
                    resultados
                        .Read<ProductoResponse>()
                        .ToList();

                var totalRegistros =
                    resultados.ReadFirst<int>();

                return Ok(
                    new PaginacionResponse<ProductoResponse>
                    {
                        Registros =
                            productos,

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
                    "Error al consultar productos paginados."
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