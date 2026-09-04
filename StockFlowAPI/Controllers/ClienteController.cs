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
    public class ClienteController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClienteController> _logger;

        public ClienteController(
            IConfiguration configuration,
            ILogger<ClienteController> logger)
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
        // CONSULTAR CLIENTES
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarClientesAPI")]
        public IActionResult ConsultarClientesAPI()
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

                var clientes =
                    connection.Query<ClienteResponse>(
                        "SP_ConsultarClientes",
                        new
                        {
                            IdEmpresa = idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    ).ToList();

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar clientes."
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
        // CONSULTAR CLIENTE
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarClienteAPI/{idCliente}")]
        public IActionResult ConsultarClienteAPI(
            long idCliente)
        {
            try
            {
                if (idCliente <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El cliente no es válido."
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

                var cliente =
                    connection.QueryFirstOrDefault
                    <ClienteResponse>(
                        "SP_ConsultarCliente",
                        new
                        {
                            IdCliente =
                                idCliente,

                            IdEmpresa =
                                idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    );

                if (cliente == null)
                {
                    return NotFound(new
                    {
                        mensaje =
                            "El cliente no existe."
                    });
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar cliente {IdCliente}.",
                    idCliente
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
        // REGISTRAR CLIENTE
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado")]
        [HttpPost]
        [Route("RegistrarClienteAPI")]
        public IActionResult RegistrarClienteAPI(
            RegistrarClienteRequest request)
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
                    "@IdCliente",
                    dbType: DbType.Int64,
                    direction:
                        ParameterDirection.Output
                );

                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );

                connection.Execute(
                    "SP_RegistrarCliente",
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

                var idCliente =
                    parametros.Get<long>(
                        "@IdCliente"
                    );

                if (resultado == 1)
                {
                    return StatusCode(
                        StatusCodes.Status201Created,
                        new
                        {
                            idCliente,
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
                        "SP_RegistrarCliente",
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
                    "Error inesperado al registrar cliente."
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
        // MODIFICAR CLIENTE
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado")]
        [HttpPut]
        [Route("ModificarClienteAPI")]
        public IActionResult ModificarClienteAPI(
            ModificarClienteRequest request)
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
                    "@IdCliente",
                    request.IdCliente
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
                    "SP_ModificarCliente",
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
                        "SP_ModificarCliente",
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
                    "Error inesperado al modificar cliente {IdCliente}.",
                    request.IdCliente
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
        [Route("CambiarEstadoClienteAPI/{idCliente}")]
        public IActionResult CambiarEstadoClienteAPI(
            long idCliente)
        {
            try
            {
                if (idCliente <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El cliente no es válido."
                    });
                }

                var idEmpresa =
                    ObtenerIdEmpresa();

                if (idEmpresa <= 0)
                    return Unauthorized();

                var parametros =
                    new DynamicParameters();

                parametros.Add(
                    "@IdCliente",
                    idCliente
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
                    "SP_CambiarEstadoCliente",
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
                        "SP_CambiarEstadoCliente",
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
                    "Error inesperado al cambiar estado de cliente {IdCliente}.",
                    idCliente
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
        [Route("ConsultarClientesPaginadoAPI")]
        public IActionResult ConsultarClientesPaginadoAPI(
            ConsultarClientesPaginadoRequest request)
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
                        "SP_ConsultarClientesPaginado",
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

                var clientes =
                    resultados
                        .Read<ClienteResponse>()
                        .ToList();

                var totalRegistros =
                    resultados.ReadFirst<int>();

                return Ok(
                    new PaginacionResponse<ClienteResponse>
                    {
                        Registros =
                            clientes,

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
                    "Error al consultar clientes paginados."
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