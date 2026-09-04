using BCrypt.Net;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using StockFlowAPI.Models;
using StockFlowAPI.Models.Request;
using StockFlowAPI.Models.Response;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StockFlowAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;


        public HomeController(
            IConfiguration configuration,
            ILogger<HomeController> logger)
        {
            _configuration =
                configuration;

            _logger =
                logger;
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
        // NORMALIZAR CAMPO OPCIONAL
        // =========================================================

        private static string? NormalizarOpcional(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
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
        // GENERAR TOKEN
        // =========================================================

        private string GenerarToken(
            UsuarioLoginModel usuario)
        {
            var secretKey =
                _configuration[
                    "Jwt:SecretKey"
                ];


            var issuer =
                _configuration[
                    "Jwt:Issuer"
                ];


            var audience =
                _configuration[
                    "Jwt:Audience"
                ];


            var expirationMinutes =
                _configuration.GetValue<int>(
                    "Jwt:ExpirationMinutes"
                );


            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException(
                    "No se encontró Jwt:SecretKey."
                );
            }


            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new InvalidOperationException(
                    "No se encontró Jwt:Issuer."
                );
            }


            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new InvalidOperationException(
                    "No se encontró Jwt:Audience."
                );
            }


            if (expirationMinutes <= 0)
            {
                throw new InvalidOperationException(
                    "Jwt:ExpirationMinutes no es válido."
                );
            }


            // =====================================================
            // CLAIMS
            // =====================================================

            var claims =
                new List<Claim>
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        usuario.IdUsuario.ToString()
                    ),

                    new Claim(
                        "IdEmpresa",
                        usuario.IdEmpresa.ToString()
                    ),

                    new Claim(
                        ClaimTypes.Name,
                        usuario.NombreUsuario
                    ),

                    new Claim(
                        ClaimTypes.Role,
                        usuario.Rol
                    )
                };


            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        secretKey
                    )
                );


            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );


            var token =
                new JwtSecurityToken(
                    issuer:
                        issuer,

                    audience:
                        audience,

                    claims:
                        claims,

                    expires:
                        DateTime.UtcNow
                            .AddMinutes(
                                expirationMinutes
                            ),

                    signingCredentials:
                        credentials
                );


            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }


        // =========================================================
        // LOGIN
        // =========================================================

        [HttpPost]
        [Route("LoginAPI")]
        public IActionResult LoginAPI(
            LoginRequest request)
        {
            try
            {
                var parametros =
                    new DynamicParameters();


                parametros.Add(
                    "@IdentificacionEmpresa",
                    request
                        .IdentificacionEmpresa
                        .Trim()
                );


                parametros.Add(
                    "@Correo",
                    request
                        .Correo
                        .Trim()
                );


                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                var usuario =
                    connection
                        .QueryFirstOrDefault<UsuarioLoginModel>(
                            "SP_ConsultarUsuarioLogin",
                            parametros,
                            commandType:
                                CommandType.StoredProcedure
                        );


                // =================================================
                // CREDENCIALES INCORRECTAS
                // =================================================

                if (usuario == null)
                {
                    return Unauthorized(
                        new
                        {
                            mensaje =
                                "Empresa, correo o contraseña incorrectos."
                        }
                    );
                }


                var contrasenaValida =
                    BCrypt.Net.BCrypt.Verify(
                        request.Contrasena,
                        usuario.Contrasena
                    );


                if (!contrasenaValida)
                {
                    return Unauthorized(
                        new
                        {
                            mensaje =
                                "Empresa, correo o contraseña incorrectos."
                        }
                    );
                }


                // =================================================
                // JWT
                // =================================================

                var token =
                    GenerarToken(usuario);


                var response =
                    new LoginResponse
                    {
                        IdUsuario =
                            usuario.IdUsuario,

                        IdEmpresa =
                            usuario.IdEmpresa,

                        NombreUsuario =
                            usuario.NombreUsuario,

                        NombreEmpresa =
                            usuario.NombreEmpresa,

                        Rol =
                            usuario.Rol,

                        Token =
                            token
                    };


                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error durante el inicio de sesión."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error al iniciar sesión."
                    }
                );
            }
        }


        // =========================================================
        // REGISTRAR EMPRESA
        // =========================================================

        [HttpPost]
        [Route("RegistrarEmpresaAPI")]
        public IActionResult RegistrarEmpresaAPI(
            RegistrarEmpresaRequest request)
        {
            try
            {
                // =================================================
                // HASH
                // =================================================

                var contrasenaHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Contrasena
                    );


                // =================================================
                // PARÁMETROS
                // =================================================

                var parametros =
                    new DynamicParameters();


                parametros.Add(
                    "@IdentificacionEmpresa",
                    request
                        .IdentificacionEmpresa
                        .Trim()
                );


                parametros.Add(
                    "@NombreEmpresa",
                    request
                        .NombreEmpresa
                        .Trim()
                );


                parametros.Add(
                    "@CorreoEmpresa",
                    NormalizarOpcional(
                        request.CorreoEmpresa
                    )
                );


                parametros.Add(
                    "@TelefonoEmpresa",
                    NormalizarOpcional(
                        request.TelefonoEmpresa
                    )
                );


                parametros.Add(
                    "@DireccionEmpresa",
                    NormalizarOpcional(
                        request.DireccionEmpresa
                    )
                );


                parametros.Add(
                    "@NombreAdministrador",
                    request
                        .NombreAdministrador
                        .Trim()
                );


                parametros.Add(
                    "@CorreoAdministrador",
                    request
                        .CorreoAdministrador
                        .Trim()
                );


                parametros.Add(
                    "@Contrasena",
                    contrasenaHash
                );


                // =================================================
                // OUTPUT
                // =================================================

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
                    size: 200,
                    direction:
                        ParameterDirection.Output
                );


                parametros.Add(
                    "@IdEmpresa",
                    dbType:
                        DbType.Int64,
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
                    "SP_RegistrarEmpresaAdministrador",
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
                // DUPLICADO
                // =================================================

                if (resultado == -1)
                {
                    return Conflict(
                        new
                        {
                            mensaje
                        }
                    );
                }


                // =================================================
                // ERROR DE CONFIGURACIÓN INTERNA
                // =================================================

                if (resultado == -2 ||
                    resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_RegistrarEmpresaAdministrador",
                        mensaje
                    );
                }


                // =================================================
                // RESULTADO INESPERADO
                // =================================================

                if (resultado != 1)
                {
                    return ErrorInternoSP(
                        "SP_RegistrarEmpresaAdministrador",
                        mensaje
                    );
                }


                // =================================================
                // SUCCESS
                // =================================================

                var idEmpresa =
                    parametros.Get<long>(
                        "@IdEmpresa"
                    );


                var idUsuario =
                    parametros.Get<long>(
                        "@IdUsuario"
                    );


                var response =
                    new RegistrarEmpresaResponse
                    {
                        IdEmpresa =
                            idEmpresa,

                        IdUsuario =
                            idUsuario,

                        NombreEmpresa =
                            request
                                .NombreEmpresa
                                .Trim(),

                        NombreAdministrador =
                            request
                                .NombreAdministrador
                                .Trim(),

                        CorreoAdministrador =
                            request
                                .CorreoAdministrador
                                .Trim(),

                        Mensaje =
                            mensaje
                    };


                return StatusCode(
                    StatusCodes.Status201Created,
                    response
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al registrar una empresa."
                );


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error al registrar la empresa."
                    }
                );
            }
        }
    }
}