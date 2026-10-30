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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsuarioController> _logger;


        public UsuarioController(
            IConfiguration configuration,
            ILogger<UsuarioController> logger)
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
        // ERROR INTERNO DE SP
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
        // PERFIL ACTUAL
        // =========================================================

        [HttpGet]
        [Route("PerfilAPI")]
        public IActionResult PerfilAPI()
        {
            try
            {
                var idUsuarioClaim =
                    User.FindFirst(
                        ClaimTypes.NameIdentifier
                    )?.Value;

                var idEmpresaClaim =
                    User.FindFirst(
                        "IdEmpresa"
                    )?.Value;

                var nombre =
                    User.FindFirst(
                        ClaimTypes.Name
                    )?.Value;

                var rol =
                    User.FindFirst(
                        ClaimTypes.Role
                    )?.Value;


                if (!long.TryParse(
                    idUsuarioClaim,
                    out var idUsuario))
                {
                    return Unauthorized(new
                    {
                        mensaje =
                            "La sesión no contiene un usuario válido."
                    });
                }


                if (!long.TryParse(
                    idEmpresaClaim,
                    out var idEmpresa))
                {
                    return Unauthorized(new
                    {
                        mensaje =
                            "La sesión no contiene una empresa válida."
                    });
                }


                if (string.IsNullOrWhiteSpace(nombre) ||
                    string.IsNullOrWhiteSpace(rol))
                {
                    return Unauthorized(new
                    {
                        mensaje =
                            "La información de la sesión no es válida."
                    });
                }


                return Ok(new
                {
                    idUsuario,
                    idEmpresa,
                    nombre,
                    rol
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar el perfil del usuario."
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
        // CONSULTAR MI CUENTA
        // CUALQUIER USUARIO AUTENTICADO
        // =========================================================

        [HttpGet]
        [Route("ConsultarMiCuentaAPI")]
        public IActionResult ConsultarMiCuentaAPI()
        {
            try
            {
                var idUsuario =
                    ObtenerIdUsuario();

                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idUsuario <= 0 ||
                    idEmpresa <= 0)
                {
                    return Unauthorized(new
                    {
                        mensaje =
                            "La sesión no es válida."
                    });
                }


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var cuenta =
                    connection
                        .QueryFirstOrDefault
                        <MiCuentaResponse>(
                            "SP_ConsultarMiCuenta",
                            new
                            {
                                IdUsuario =
                                    idUsuario,

                                IdEmpresa =
                                    idEmpresa
                            },
                            commandType:
                                CommandType
                                    .StoredProcedure
                        );


                if (cuenta == null)
                {
                    return Unauthorized(new
                    {
                        mensaje =
                            "La sesión ya no es válida."
                    });
                }


                return Ok(cuenta);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar Mi Cuenta."
                );


                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al consultar la cuenta."
                    }
                );
            }
        }


        // =========================================================
        // CAMBIAR MI CONTRASEÑA
        // CUALQUIER USUARIO AUTENTICADO
        // =========================================================

        [HttpPost]
        [Route("CambiarContrasenaAPI")]
        public IActionResult CambiarContrasenaAPI(
            CambiarContrasenaRequest request)
        {
            try
            {
                var idUsuario =
                    ObtenerIdUsuario();

                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idUsuario <= 0 ||
                    idEmpresa <= 0)
                {
                    return Unauthorized(new
                    {
                        mensaje =
                            "La sesión no es válida."
                    });
                }


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var hashActual =
                    connection
                        .QueryFirstOrDefault<string>(
                            "SP_ConsultarHashUsuario",
                            new
                            {
                                IdUsuario =
                                    idUsuario,

                                IdEmpresa =
                                    idEmpresa
                            },
                            commandType:
                                CommandType
                                    .StoredProcedure
                        );


                if (string.IsNullOrWhiteSpace(
                    hashActual))
                {
                    return Unauthorized(new
                    {
                        mensaje =
                            "La sesión ya no es válida."
                    });
                }


                bool contrasenaActualCorrecta;


                try
                {
                    contrasenaActualCorrecta =
                        BCrypt.Net.BCrypt.Verify(
                            request.ContrasenaActual,
                            hashActual
                        );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "El hash almacenado para el usuario {IdUsuario} no es válido.",
                        idUsuario
                    );


                    return StatusCode(
                        StatusCodes
                            .Status500InternalServerError,
                        new
                        {
                            mensaje =
                                "Ocurrió un error interno al procesar la solicitud."
                        }
                    );
                }


                if (!contrasenaActualCorrecta)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La contraseña actual no es correcta."
                    });
                }


                if (BCrypt.Net.BCrypt.Verify(
                    request.ContrasenaNueva,
                    hashActual))
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "La nueva contraseña debe ser diferente de la contraseña actual."
                    });
                }


                var contrasenaNuevaHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.ContrasenaNueva
                    );


                var parametros =
                    new DynamicParameters();


                parametros.Add(
                    "@IdUsuario",
                    idUsuario
                );


                parametros.Add(
                    "@IdEmpresa",
                    idEmpresa
                );


                parametros.Add(
                    "@ContrasenaNueva",
                    contrasenaNuevaHash
                );


                parametros.Add(
                    "@Resultado",
                    dbType:
                        DbType.Int32,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@Mensaje",
                    dbType:
                        DbType.String,
                    size:
                        200,
                    direction:
                        ParameterDirection.Output
                );


                connection.Execute(
                    "SP_CambiarContrasenaPropia",
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
                    return Unauthorized(new
                    {
                        mensaje
                    });
                }


                if (resultado == -2)
                {
                    return BadRequest(new
                    {
                        mensaje
                    });
                }


                if (resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_CambiarContrasenaPropia",
                        mensaje
                    );
                }


                return ErrorInternoSP(
                    "SP_CambiarContrasenaPropia",
                    mensaje
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al cambiar la contraseña del usuario actual."
                );


                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al cambiar la contraseña."
                    }
                );
            }
        }


        // =========================================================
        // CONSULTAR ROLES PARA GESTIÓN
        // SOLO ADMINISTRADOR
        // =========================================================

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        [Route("ConsultarRolesAPI")]
        public IActionResult ConsultarRolesAPI()
        {
            try
            {
                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var roles =
                    connection.Query<RolResponse>(
                        "SP_ConsultarRolesGestionUsuarios",
                        commandType:
                            CommandType.StoredProcedure
                    ).ToList();


                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar los roles para gestión de usuarios."
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al consultar los roles."
                    }
                );
            }
        }


        // =========================================================
        // CONSULTAR USUARIOS PAGINADOS
        // SOLO ADMINISTRADOR
        // =========================================================

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [Route("ConsultarUsuariosPaginadoAPI")]
        public IActionResult ConsultarUsuariosPaginadoAPI(
            ConsultarUsuariosPaginadoRequest request)
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
                            "No fue posible identificar la empresa."
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
                        "SP_ConsultarUsuariosPaginado",
                        new
                        {
                            IdEmpresa =
                                idEmpresa,

                            Busqueda =
                                busqueda,

                            IdRol =
                                request.IdRol,

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


                var usuarios =
                    resultados
                        .Read<UsuarioResponse>()
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
                    new PaginacionResponse<UsuarioResponse>
                    {
                        Registros =
                            usuarios,

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
                    "Error al consultar usuarios paginados."
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al consultar los usuarios."
                    }
                );
            }
        }


        // =========================================================
        // CONSULTAR UN USUARIO
        // SOLO ADMINISTRADOR
        // =========================================================

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        [Route("ConsultarUsuarioAPI/{idUsuario}")]
        public IActionResult ConsultarUsuarioAPI(
            long idUsuario)
        {
            try
            {
                if (idUsuario <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El usuario no es válido."
                    });
                }


                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                {
                    return Unauthorized();
                }


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var usuario =
                    connection.QueryFirstOrDefault<UsuarioResponse>(
                        "SP_ConsultarUsuario",
                        new
                        {
                            IdUsuario =
                                idUsuario,

                            IdEmpresa =
                                idEmpresa
                        },
                        commandType:
                            CommandType.StoredProcedure
                    );


                if (usuario == null)
                {
                    return NotFound(new
                    {
                        mensaje =
                            "El usuario no existe."
                    });
                }


                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar el usuario {IdUsuario}.",
                    idUsuario
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al consultar el usuario."
                    }
                );
            }
        }


        // =========================================================
        // REGISTRAR USUARIO
        // SOLO ADMINISTRADOR
        // =========================================================

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [Route("RegistrarUsuarioAPI")]
        public IActionResult RegistrarUsuarioAPI(
            RegistrarUsuarioRequest request)
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                {
                    return Unauthorized();
                }


                var contrasenaHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Contrasena
                    );


                var parametros =
                    new DynamicParameters();


                parametros.Add(
                    "@IdEmpresa",
                    idEmpresa
                );


                parametros.Add(
                    "@IdRol",
                    request.IdRol
                );


                parametros.Add(
                    "@Nombre",
                    request.Nombre.Trim()
                );


                parametros.Add(
                    "@Correo",
                    request.Correo.Trim()
                );


                parametros.Add(
                    "@Contrasena",
                    contrasenaHash
                );


                parametros.Add(
                    "@Resultado",
                    dbType:
                        DbType.Int32,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@Mensaje",
                    dbType:
                        DbType.String,
                    size:
                        200,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@IdUsuario",
                    dbType:
                        DbType.Int64,
                    direction:
                        ParameterDirection.Output
                );


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                connection.Execute(
                    "SP_RegistrarUsuario",
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
                    return StatusCode(
                        StatusCodes.Status201Created,
                        new
                        {
                            idUsuario =
                                parametros.Get<long>(
                                    "@IdUsuario"
                                ),

                            mensaje
                        }
                    );
                }


                if (resultado == -1)
                {
                    return BadRequest(new
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
                        "SP_RegistrarUsuario",
                        mensaje
                    );
                }


                return ErrorInternoSP(
                    "SP_RegistrarUsuario",
                    mensaje
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al registrar un usuario."
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al registrar el usuario."
                    }
                );
            }
        }


        // =========================================================
        // MODIFICAR USUARIO
        // SOLO ADMINISTRADOR
        // =========================================================

        [Authorize(Roles = "Administrador")]
        [HttpPut]
        [Route("ModificarUsuarioAPI")]
        public IActionResult ModificarUsuarioAPI(
            ModificarUsuarioRequest request)
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                {
                    return Unauthorized();
                }


                var parametros =
                    new DynamicParameters();


                parametros.Add(
                    "@IdUsuario",
                    request.IdUsuario
                );


                parametros.Add(
                    "@IdEmpresa",
                    idEmpresa
                );


                parametros.Add(
                    "@IdRol",
                    request.IdRol
                );


                parametros.Add(
                    "@Nombre",
                    request.Nombre.Trim()
                );


                parametros.Add(
                    "@Correo",
                    request.Correo.Trim()
                );


                parametros.Add(
                    "@Resultado",
                    dbType:
                        DbType.Int32,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@Mensaje",
                    dbType:
                        DbType.String,
                    size:
                        200,
                    direction:
                        ParameterDirection.Output
                );


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                connection.Execute(
                    "SP_ModificarUsuario",
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
                    return BadRequest(new
                    {
                        mensaje
                    });
                }


                if (resultado == -4)
                {
                    return Conflict(new
                    {
                        mensaje
                    });
                }


                if (resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_ModificarUsuario",
                        mensaje
                    );
                }


                return ErrorInternoSP(
                    "SP_ModificarUsuario",
                    mensaje
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al modificar un usuario."
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al modificar el usuario."
                    }
                );
            }
        }


        // =========================================================
        // CAMBIAR ESTADO
        // SOLO ADMINISTRADOR
        // =========================================================

        [Authorize(Roles = "Administrador")]
        [HttpPut]
        [Route("CambiarEstadoUsuarioAPI/{idUsuario}")]
        public IActionResult CambiarEstadoUsuarioAPI(
            long idUsuario)
        {
            try
            {
                if (idUsuario <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El usuario no es válido."
                    });
                }


                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                {
                    return Unauthorized();
                }


                var parametros =
                    new DynamicParameters();


                parametros.Add(
                    "@IdUsuario",
                    idUsuario
                );


                parametros.Add(
                    "@IdEmpresa",
                    idEmpresa
                );


                parametros.Add(
                    "@Resultado",
                    dbType:
                        DbType.Int32,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@Mensaje",
                    dbType:
                        DbType.String,
                    size:
                        200,
                    direction:
                        ParameterDirection.Output
                );


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                connection.Execute(
                    "SP_CambiarEstadoUsuario",
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
                        "SP_CambiarEstadoUsuario",
                        mensaje
                    );
                }


                return ErrorInternoSP(
                    "SP_CambiarEstadoUsuario",
                    mensaje
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al cambiar el estado del usuario {IdUsuario}.",
                    idUsuario
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al actualizar el usuario."
                    }
                );
            }
        }
    }
}
