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
    public class CategoriaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CategoriaController> _logger;

        public CategoriaController(
            IConfiguration configuration,
            ILogger<CategoriaController> logger)
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
        // CONSULTAR CATEGORÍAS
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarCategoriasAPI")]
        public IActionResult ConsultarCategoriasAPI()
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

                var categorias =
                    connection.Query<CategoriaResponse>(
                        "SP_ConsultarCategorias",
                        new
                        {
                            IdEmpresa = idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    ).ToList();

                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar las categorías."
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
        // CONSULTAR CATEGORÍA
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado,Consulta")]
        [HttpGet]
        [Route("ConsultarCategoriaAPI/{idCategoria}")]
        public IActionResult ConsultarCategoriaAPI(
            long idCategoria)
        {
            try
            {
                if (idCategoria <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La categoría no es válida."
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

                var categoria =
                    connection.QueryFirstOrDefault
                    <CategoriaResponse>(
                        "SP_ConsultarCategoria",
                        new
                        {
                            IdCategoria =
                                idCategoria,

                            IdEmpresa =
                                idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    );

                if (categoria == null)
                {
                    return NotFound(new
                    {
                        mensaje =
                            "La categoría no existe."
                    });
                }

                return Ok(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar la categoría {IdCategoria}.",
                    idCategoria
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
        // REGISTRAR
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado")]
        [HttpPost]
        [Route("RegistrarCategoriaAPI")]
        public IActionResult RegistrarCategoriaAPI(
            RegistrarCategoriaRequest request)
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
                    "@IdCategoria",
                    dbType: DbType.Int64,
                    direction:
                        ParameterDirection.Output
                );

                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );

                connection.Execute(
                    "SP_RegistrarCategoria",
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

                var idCategoria =
                    parametros.Get<long>(
                        "@IdCategoria"
                    );

                if (resultado == 1)
                {
                    return StatusCode(
                        StatusCodes.Status201Created,
                        new
                        {
                            idCategoria,
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
                        "SP_RegistrarCategoria",
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
                    "Error inesperado al registrar una categoría."
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
        // MODIFICAR
        // =========================================================

        [Authorize(Roles = "Administrador,Empleado")]
        [HttpPut]
        [Route("ModificarCategoriaAPI")]
        public IActionResult ModificarCategoriaAPI(
            ModificarCategoriaRequest request)
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
                    "@IdCategoria",
                    request.IdCategoria
                );

                parametros.Add(
                    "@IdEmpresa",
                    idEmpresa
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
                    "SP_ModificarCategoria",
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
                        "SP_ModificarCategoria",
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
                    "Error inesperado al modificar la categoría {IdCategoria}.",
                    request.IdCategoria
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
        [Route("CambiarEstadoCategoriaAPI/{idCategoria}")]
        public IActionResult CambiarEstadoCategoriaAPI(
            long idCategoria)
        {
            try
            {
                if (idCategoria <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La categoría no es válida."
                    });
                }

                var idEmpresa =
                    ObtenerIdEmpresa();

                if (idEmpresa <= 0)
                    return Unauthorized();

                var parametros =
                    new DynamicParameters();

                parametros.Add(
                    "@IdCategoria",
                    idCategoria
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
                    "SP_CambiarEstadoCategoria",
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
                        "SP_CambiarEstadoCategoria",
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
                    "Error inesperado al cambiar estado de categoría {IdCategoria}.",
                    idCategoria
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
        [Route("ConsultarCategoriasPaginadoAPI")]
        public IActionResult ConsultarCategoriasPaginadoAPI(
            ConsultarCategoriasPaginadoRequest request)
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
                        "SP_ConsultarCategoriasPaginado",
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

                var categorias =
                    resultados
                        .Read<CategoriaResponse>()
                        .ToList();

                var totalRegistros =
                    resultados.ReadFirst<int>();

                return Ok(
                    new PaginacionResponse<CategoriaResponse>
                    {
                        Registros =
                            categorias,

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
                    "Error al consultar categorías paginadas."
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