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
    public class ProveedorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProveedorController> _logger;

        public ProveedorController(
            IConfiguration configuration,
            ILogger<ProveedorController> logger)
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
        // CONSULTAR PROVEEDORES
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarProveedoresAPI")]
        public IActionResult ConsultarProveedoresAPI()
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

                var proveedores =
                    connection.Query<ProveedorResponse>(
                        "SP_ConsultarProveedores",
                        new
                        {
                            IdEmpresa = idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    ).ToList();

                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar proveedores."
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
        // CONSULTAR PROVEEDOR
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarProveedorAPI/{idProveedor}")]
        public IActionResult ConsultarProveedorAPI(
            long idProveedor)
        {
            try
            {
                if (idProveedor <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El proveedor no es válido."
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

                var proveedor =
                    connection.QueryFirstOrDefault
                    <ProveedorResponse>(
                        "SP_ConsultarProveedor",
                        new
                        {
                            IdProveedor =
                                idProveedor,

                            IdEmpresa =
                                idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    );

                if (proveedor == null)
                {
                    return NotFound(new
                    {
                        mensaje =
                            "El proveedor no existe."
                    });
                }

                return Ok(proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar proveedor {IdProveedor}.",
                    idProveedor
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
        // REGISTRAR PROVEEDOR
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado")]
        [HttpPost]
        [Route("RegistrarProveedorAPI")]
        public IActionResult RegistrarProveedorAPI(
            RegistrarProveedorRequest request)
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
                    "@Identificacion",
                    NormalizarOpcional(
                        request.Identificacion
                    )
                );

                parametros.Add(
                    "@Nombre",
                    request.Nombre.Trim()
                );

                parametros.Add(
                    "@Correo",
                    NormalizarOpcional(
                        request.Correo
                    )
                );

                parametros.Add(
                    "@Telefono",
                    NormalizarOpcional(
                        request.Telefono
                    )
                );

                parametros.Add(
                    "@Direccion",
                    NormalizarOpcional(
                        request.Direccion
                    )
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
                    "@IdProveedor",
                    dbType: DbType.Int64,
                    direction:
                        ParameterDirection.Output
                );

                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );

                connection.Execute(
                    "SP_RegistrarProveedor",
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

                var idProveedor =
                    parametros.Get<long>(
                        "@IdProveedor"
                    );

                if (resultado == 1)
                {
                    return StatusCode(
                        StatusCodes.Status201Created,
                        new
                        {
                            idProveedor,
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

                if (resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_RegistrarProveedor",
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
                    "Error inesperado al registrar proveedor."
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
        // MODIFICAR PROVEEDOR
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado")]
        [HttpPut]
        [Route("ModificarProveedorAPI")]
        public IActionResult ModificarProveedorAPI(
            ModificarProveedorRequest request)
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
                    "@IdProveedor",
                    request.IdProveedor
                );

                parametros.Add(
                    "@IdEmpresa",
                    idEmpresa
                );

                parametros.Add(
                    "@Identificacion",
                    NormalizarOpcional(
                        request.Identificacion
                    )
                );

                parametros.Add(
                    "@Nombre",
                    request.Nombre.Trim()
                );

                parametros.Add(
                    "@Correo",
                    NormalizarOpcional(
                        request.Correo
                    )
                );

                parametros.Add(
                    "@Telefono",
                    NormalizarOpcional(
                        request.Telefono
                    )
                );

                parametros.Add(
                    "@Direccion",
                    NormalizarOpcional(
                        request.Direccion
                    )
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
                    "SP_ModificarProveedor",
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

                if (resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_ModificarProveedor",
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
                    "Error inesperado al modificar proveedor {IdProveedor}.",
                    request.IdProveedor
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
        [Route("CambiarEstadoProveedorAPI/{idProveedor}")]
        public IActionResult CambiarEstadoProveedorAPI(
            long idProveedor)
        {
            try
            {
                if (idProveedor <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El proveedor no es válido."
                    });
                }

                var idEmpresa =
                    ObtenerIdEmpresa();

                if (idEmpresa <= 0)
                    return Unauthorized();

                var parametros =
                    new DynamicParameters();

                parametros.Add(
                    "@IdProveedor",
                    idProveedor
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
                    "SP_CambiarEstadoProveedor",
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
                        "SP_CambiarEstadoProveedor",
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
                    "Error inesperado al cambiar estado del proveedor {IdProveedor}.",
                    idProveedor
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
        [Route("ConsultarProveedoresPaginadoAPI")]
        public IActionResult ConsultarProveedoresPaginadoAPI(
            ConsultarProveedoresPaginadoRequest request)
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
                        "SP_ConsultarProveedoresPaginado",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            Busqueda =
                                NormalizarOpcional(
                                    request.Busqueda
                                ),

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

                var proveedores =
                    resultados
                        .Read<ProveedorResponse>()
                        .ToList();

                var totalRegistros =
                    resultados.ReadFirst<int>();

                return Ok(
                    new PaginacionResponse<ProveedorResponse>
                    {
                        Registros =
                            proveedores,

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
                    "Error al consultar proveedores paginados."
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