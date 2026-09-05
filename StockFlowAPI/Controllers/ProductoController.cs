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
        private readonly IConfiguration
            _configuration;

        private readonly ILogger<ProductoController>
            _logger;


        public ProductoController(
            IConfiguration configuration,
            ILogger<ProductoController> logger)
        {
            _configuration =
                configuration;

            _logger =
                logger;
        }


        // =========================================================
        // ID EMPRESA DESDE JWT
        // =========================================================

        private long ObtenerIdEmpresa()
        {
            var claim =
                User.FindFirst(
                    "IdEmpresa"
                )?.Value;


            return long.TryParse(
                claim,
                out var idEmpresa
            )
                ? idEmpresa
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
            ??
            throw new InvalidOperationException(
                "No se encontró la cadena de conexión."
            );
        }


        // =========================================================
        // NORMALIZAR STRING OPCIONAL
        // =========================================================

        private static string?
            NormalizarOpcional(
                string? valor)
        {
            return string.IsNullOrWhiteSpace(
                valor
            )
                ? null
                : valor.Trim();
        }


        // =========================================================
        // ERROR INTERNO DEL SP
        // =========================================================

        private IActionResult ErrorInternoSP(
            string procedimiento,
            string? mensajeSql)
        {
            _logger.LogError(
                "Error interno en {Procedimiento}. Detalle SQL: {MensajeSql}",
                procedimiento,
                mensajeSql
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


        // =========================================================
        // CONSULTAR PRODUCTOS
        // =========================================================

        [Authorize(
            Roles =
                "Administrador,Empleado,Consulta"
        )]
        [HttpGet]
        [Route("ConsultarProductosAPI")]
        public IActionResult
            ConsultarProductosAPI()
        {
            try
            {
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


                var productos =
                    connection
                        .Query<ProductoResponse>(
                            "SP_ConsultarProductos",
                            new
                            {
                                IdEmpresa =
                                    idEmpresa
                            },
                            commandType:
                                CommandType
                                    .StoredProcedure
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
                    "Error al consultar productos."
                );


                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al consultar los productos."
                    }
                );
            }
        }


        // =========================================================
        // CONSULTAR PRODUCTO
        // =========================================================

        [Authorize(
            Roles =
                "Administrador,Empleado,Consulta"
        )]
        [HttpGet]
        [Route(
            "ConsultarProductoAPI/{idProducto}"
        )]
        public IActionResult
            ConsultarProductoAPI(
                long idProducto)
        {
            try
            {
                if (idProducto <= 0)
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                "El producto no es válido."
                        }
                    );
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


                var producto =
                    connection
                        .QueryFirstOrDefault
                        <ProductoResponse>(
                            "SP_ConsultarProducto",
                            new
                            {
                                IdEmpresa =
                                    idEmpresa,

                                IdProducto =
                                    idProducto
                            },
                            commandType:
                                CommandType
                                    .StoredProcedure
                        );


                if (producto == null)
                {
                    return NotFound(
                        new
                        {
                            mensaje =
                                "El producto no existe."
                        }
                    );
                }


                return Ok(
                    producto
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar producto {IdProducto}.",
                    idProducto
                );


                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al consultar el producto."
                    }
                );
            }
        }


        // =========================================================
        // REGISTRAR PRODUCTO
        //
        // EL CÓDIGO YA NO VIENE DEL CLIENTE.
        // SQL SERVER GENERA:
        //
        // TEC001
        // TEC002
        // TEC003
        // ...
        //
        // POR EMPRESA.
        // =========================================================

        [Authorize(
            Roles =
                "Administrador,Empleado"
        )]
        [HttpPost]
        [Route("RegistrarProductoAPI")]
        public IActionResult
            RegistrarProductoAPI(
                RegistrarProductoRequest request)
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                {
                    return Unauthorized();
                }


                // =================================================
                // VALIDACIÓN ADICIONAL
                // =================================================

                if (string.IsNullOrWhiteSpace(
                    request.Nombre))
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                "Debe indicar el nombre del producto."
                        }
                    );
                }


                if (
                    request.StockMinimo
                    !=
                    decimal.Truncate(
                        request.StockMinimo
                    )
                )
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                "El stock mínimo debe ser un número entero."
                        }
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
                    "@IdCategoria",
                    request.IdCategoria
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


                // =================================================
                // OUTPUT RESULTADO
                // =================================================

                parametros.Add(
                    "@Resultado",
                    dbType:
                        DbType.Int32,
                    direction:
                        ParameterDirection.Output
                );


                // =================================================
                // OUTPUT MENSAJE
                // =================================================

                parametros.Add(
                    "@Mensaje",
                    dbType:
                        DbType.String,
                    size:
                        200,
                    direction:
                        ParameterDirection.Output
                );


                // =================================================
                // OUTPUT ID PRODUCTO
                // =================================================

                parametros.Add(
                    "@IdProducto",
                    dbType:
                        DbType.Int64,
                    direction:
                        ParameterDirection.Output
                );


                // =================================================
                // OUTPUT CÓDIGO GENERADO
                // =================================================

                parametros.Add(
                    "@Codigo",
                    dbType:
                        DbType.String,
                    size:
                        50,
                    direction:
                        ParameterDirection.Output
                );


                // =================================================
                // EJECUTAR SP
                // =================================================

                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                connection.Execute(
                    "SP_RegistrarProducto",
                    parametros,
                    commandType:
                        CommandType
                            .StoredProcedure
                );


                // =================================================
                // LEER OUTPUTS
                // =================================================

                var resultado =
                    parametros.Get<int>(
                        "@Resultado"
                    );


                var mensaje =
                    parametros.Get<string?>(
                        "@Mensaje"
                    );


                var idProducto =
                    parametros.Get<long>(
                        "@IdProducto"
                    );


                var codigo =
                    parametros.Get<string?>(
                        "@Codigo"
                    );


                // =================================================
                // RESULTADOS DEL SP
                // =================================================


                // Categoría inválida
                if (resultado == -1)
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                mensaje
                                ??
                                "La categoría no es válida."
                        }
                    );
                }


                // Valores inválidos
                if (resultado == -3)
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                mensaje
                                ??
                                "Los valores del producto no son válidos."
                        }
                    );
                }


                // Error interno controlado por SQL
                if (resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_RegistrarProducto",
                        mensaje
                    );
                }


                if (resultado != 1)
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                mensaje
                                ??
                                "No fue posible registrar el producto."
                        }
                    );
                }


                // =================================================
                // 201 CREATED
                // =================================================

                return StatusCode(
                    StatusCodes
                        .Status201Created,
                    new
                    {
                        IdProducto =
                            idProducto,

                        Codigo =
                            codigo,

                        Mensaje =
                            mensaje
                            ??
                            "Producto registrado correctamente."
                    }
                );
            }
            catch (SqlException ex)
                when (
                    ex.Number == 2601
                    ||
                    ex.Number == 2627
                )
            {
                _logger.LogWarning(
                    ex,
                    "Conflicto de código al registrar producto."
                );


                return Conflict(
                    new
                    {
                        mensaje =
                            "No fue posible generar un código único para el producto. Intente nuevamente."
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al registrar producto."
                );


                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al registrar el producto."
                    }
                );
            }
        }


        // =========================================================
        // MODIFICAR PRODUCTO
        //
        // EL CÓDIGO NO SE RECIBE NI SE MODIFICA.
        // =========================================================

        [Authorize(
            Roles =
                "Administrador,Empleado"
        )]
        [HttpPut]
        [Route("ModificarProductoAPI")]
        public IActionResult
            ModificarProductoAPI(
                ModificarProductoRequest request)
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                {
                    return Unauthorized();
                }


                if (request.IdProducto <= 0)
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                "El producto no es válido."
                        }
                    );
                }


                if (string.IsNullOrWhiteSpace(
                    request.Nombre))
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                "Debe indicar el nombre del producto."
                        }
                    );
                }


                if (
                    request.StockMinimo
                    !=
                    decimal.Truncate(
                        request.StockMinimo
                    )
                )
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                "El stock mínimo debe ser un número entero."
                        }
                    );
                }


                // =================================================
                // PARÁMETROS
                // =================================================

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


                // =================================================
                // EJECUTAR
                // =================================================

                using var connection =
                    new SqlConnection(
                        ObtenerConnectionString()
                    );


                connection.Execute(
                    "SP_ModificarProducto",
                    parametros,
                    commandType:
                        CommandType
                            .StoredProcedure
                );


                // =================================================
                // OUTPUTS
                // =================================================

                var resultado =
                    parametros.Get<int>(
                        "@Resultado"
                    );


                var mensaje =
                    parametros.Get<string?>(
                        "@Mensaje"
                    );


                // Producto no existe o pertenece a otra empresa
                if (resultado == -1)
                {
                    return NotFound(
                        new
                        {
                            mensaje =
                                mensaje
                                ??
                                "El producto no existe."
                        }
                    );
                }


                // Categoría inválida
                if (resultado == -2)
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                mensaje
                                ??
                                "La categoría no es válida."
                        }
                    );
                }


                // Precios / stock inválidos
                if (resultado == -3)
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                mensaje
                                ??
                                "Los valores del producto no son válidos."
                        }
                    );
                }


                if (resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_ModificarProducto",
                        mensaje
                    );
                }


                if (resultado != 1)
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                mensaje
                                ??
                                "No fue posible modificar el producto."
                        }
                    );
                }


                return Ok(
                    new
                    {
                        mensaje =
                            mensaje
                            ??
                            "Producto modificado correctamente."
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al modificar producto {IdProducto}.",
                    request.IdProducto
                );


                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al modificar el producto."
                    }
                );
            }
        }


        // =========================================================
        // CAMBIAR ESTADO
        // =========================================================

        [Authorize(
            Roles =
                "Administrador"
        )]
        [HttpPut]
        [Route(
            "CambiarEstadoProductoAPI/{idProducto}"
        )]
        public IActionResult
            CambiarEstadoProductoAPI(
                long idProducto)
        {
            try
            {
                if (idProducto <= 0)
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                "El producto no es válido."
                        }
                    );
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
                    "@IdProducto",
                    idProducto
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
                    "SP_CambiarEstadoProducto",
                    parametros,
                    commandType:
                        CommandType
                            .StoredProcedure
                );


                var resultado =
                    parametros.Get<int>(
                        "@Resultado"
                    );


                var mensaje =
                    parametros.Get<string?>(
                        "@Mensaje"
                    );


                if (resultado == -1)
                {
                    return NotFound(
                        new
                        {
                            mensaje =
                                mensaje
                                ??
                                "El producto no existe."
                        }
                    );
                }


                if (resultado == -99)
                {
                    return ErrorInternoSP(
                        "SP_CambiarEstadoProducto",
                        mensaje
                    );
                }


                if (resultado != 1)
                {
                    return BadRequest(
                        new
                        {
                            mensaje =
                                mensaje
                                ??
                                "No fue posible cambiar el estado del producto."
                        }
                    );
                }


                return Ok(
                    new
                    {
                        mensaje =
                            mensaje
                            ??
                            "Estado del producto actualizado correctamente."
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al cambiar estado del producto {IdProducto}.",
                    idProducto
                );


                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al cambiar el estado del producto."
                    }
                );
            }
        }


        // =========================================================
        // CONSULTAR PRODUCTOS PAGINADOS
        // =========================================================

        [Authorize(
            Roles =
                "Administrador,Empleado,Consulta"
        )]
        [HttpPost]
        [Route(
            "ConsultarProductosPaginadoAPI"
        )]
        public IActionResult
            ConsultarProductosPaginadoAPI(
                ConsultarProductosPaginadoRequest request)
        {
            try
            {
                var idEmpresa =
                    ObtenerIdEmpresa();


                if (idEmpresa <= 0)
                {
                    return Unauthorized();
                }


                // =================================================
                // PAGINACIÓN
                // =================================================

                if (request.Pagina < 1)
                {
                    request.Pagina =
                        1;
                }


                if (
                    request.CantidadPagina != 10
                    &&
                    request.CantidadPagina != 20
                    &&
                    request.CantidadPagina != 50
                )
                {
                    request.CantidadPagina =
                        10;
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
                            CommandType
                                .StoredProcedure
                    );


                var productos =
                    resultados
                        .Read<ProductoResponse>()
                        .ToList();


                var totalRegistros =
                    resultados
                        .ReadFirst<int>();


                var totalPaginas =
                    (int)Math.Ceiling(
                        totalRegistros
                        /
                        (double)
                        request.CantidadPagina
                    );


                return Ok(
                    new PaginacionResponse
                    <ProductoResponse>
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
                            totalPaginas
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
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno al consultar los productos."
                    }
                );
            }
        }
    }
}