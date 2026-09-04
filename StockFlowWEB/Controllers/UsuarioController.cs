using Microsoft.AspNetCore.Mvc;
using StockFlowWEB.Attributes;
using StockFlowWEB.Models.Request;
using StockFlowWEB.Models.Response;
using StockFlowWEB.Models.ViewModels;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StockFlowWEB.Controllers
{
    [SessionAuthorize("Administrador")]
    public class UsuarioController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UsuarioController> _logger;


        public UsuarioController(
            IHttpClientFactory httpClientFactory,
            ILogger<UsuarioController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }


        // =========================================================
        // CREAR CLIENTE API
        // =========================================================

        private HttpClient CrearClienteAPI()
        {
            var token =
                HttpContext.Session
                    .GetString("Token");


            var client =
                _httpClientFactory
                    .CreateClient(
                        "StockFlowAPI"
                    );


            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );


            return client;
        }


        // =========================================================
        // MANEJAR 401 / 403
        // =========================================================

        private IActionResult? ManejarAutorizacion(
            HttpResponseMessage response)
        {
            if (response.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();

                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }


            if (response.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return RedirectToAction(
                    "Estado",
                    "Home",
                    new
                    {
                        codigo =
                            StatusCodes.Status403Forbidden
                    }
                );
            }


            return null;
        }


        // =========================================================
        // CONSULTAR ROLES
        // =========================================================

        private List<RolResponse> ConsultarRoles(
            HttpClient client)
        {
            var response =
                client.GetAsync(
                    "api/Usuario/ConsultarRolesAPI"
                ).Result;


            if (!response.IsSuccessStatusCode)
            {
                return new List<RolResponse>();
            }


            var roles =
                response.Content
                    .ReadFromJsonAsync
                    <List<RolResponse>>()
                    .Result;


            return roles?
                .OrderBy(x => x.NombreRol)
                .ToList()
                ??
                new List<RolResponse>();
        }


        // =========================================================
        // LISTADO
        // =========================================================

        [HttpGet]
        public IActionResult Index(
            string? busqueda,
            long? idRol,
            bool? estado,
            int pagina = 1,
            int cantidadPagina = 10)
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var roles =
                    ConsultarRoles(
                        client
                    );


                var request =
                    new ConsultarUsuariosPaginadoRequest
                    {
                        Busqueda =
                            busqueda,

                        IdRol =
                            idRol,

                        Estado =
                            estado,

                        Pagina =
                            pagina,

                        CantidadPagina =
                            cantidadPagina
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Usuario/ConsultarUsuariosPaginadoAPI",
                        request
                    ).Result;


                var autorizacion =
                    ManejarAutorizacion(
                        response
                    );


                if (autorizacion != null)
                {
                    return autorizacion;
                }


                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Mensaje =
                        "No fue posible consultar los usuarios.";


                    return View(
                        new UsuarioIndexViewModel
                        {
                            Roles =
                                roles
                        }
                    );
                }


                var resultado =
                    response.Content
                        .ReadFromJsonAsync
                        <PaginacionResponse<UsuarioResponse>>()
                        .Result;


                resultado ??=
                    new PaginacionResponse<UsuarioResponse>();


                var model =
                    new UsuarioIndexViewModel
                    {
                        Usuarios =
                            resultado.Registros,

                        Roles =
                            roles,

                        Busqueda =
                            busqueda,

                        IdRol =
                            idRol,

                        Estado =
                            estado,

                        PaginaActual =
                            resultado.PaginaActual,

                        CantidadPagina =
                            resultado.CantidadPagina,

                        TotalRegistros =
                            resultado.TotalRegistros,

                        TotalPaginas =
                            resultado.TotalPaginas
                    };


                return View(
                    model
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al cargar la administración de usuarios."
                );


                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                return View(
                    new UsuarioIndexViewModel()
                );
            }
        }


        // =========================================================
        // REGISTRAR - GET
        // =========================================================

        [HttpGet]
        public IActionResult Registrar()
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var model =
                    new RegistrarUsuarioViewModel
                    {
                        Roles =
                            ConsultarRoles(
                                client
                            )
                    };


                return View(
                    model
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al cargar el formulario de registro de usuario."
                );


                TempData["MensajeError"] =
                    "No fue posible cargar el formulario.";


                return RedirectToAction(
                    "Index"
                );
            }
        }


        // =========================================================
        // REGISTRAR - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(
            RegistrarUsuarioViewModel model)
        {
            var client =
                CrearClienteAPI();


            if (!ModelState.IsValid)
            {
                model.Roles =
                    ConsultarRoles(
                        client
                    );


                return View(
                    model
                );
            }


            try
            {
                var request =
                    new RegistrarUsuarioRequest
                    {
                        IdRol =
                            model.IdRol,

                        Nombre =
                            model.Nombre.Trim(),

                        Correo =
                            model.Correo.Trim(),

                        Contrasena =
                            model.Contrasena
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Usuario/RegistrarUsuarioAPI",
                        request
                    ).Result;


                var autorizacion =
                    ManejarAutorizacion(
                        response
                    );


                if (autorizacion != null)
                {
                    return autorizacion;
                }


                if (response.StatusCode ==
                    HttpStatusCode.Created)
                {
                    var resultado =
                        response.Content
                            .ReadFromJsonAsync
                            <MensajeResponse>()
                            .Result;


                    TempData["MensajeExito"] =
                        resultado?.Mensaje
                        ??
                        "Usuario registrado correctamente.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                var error =
                    response.Content
                        .ReadFromJsonAsync
                        <MensajeResponse>()
                        .Result;


                ViewBag.Mensaje =
                    error?.Mensaje
                    ??
                    "No fue posible registrar el usuario.";


                model.Roles =
                    ConsultarRoles(
                        client
                    );


                return View(
                    model
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al registrar un usuario desde StockFlowWEB."
                );


                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                model.Roles =
                    ConsultarRoles(
                        client
                    );


                return View(
                    model
                );
            }
        }


        // =========================================================
        // MODIFICAR - GET
        // =========================================================

        [HttpGet]
        public IActionResult Modificar(
            long id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["MensajeError"] =
                        "El usuario no es válido.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                var client =
                    CrearClienteAPI();


                var response =
                    client.GetAsync(
                        $"api/Usuario/ConsultarUsuarioAPI/{id}"
                    ).Result;


                var autorizacion =
                    ManejarAutorizacion(
                        response
                    );


                if (autorizacion != null)
                {
                    return autorizacion;
                }


                if (response.StatusCode ==
                    HttpStatusCode.NotFound)
                {
                    TempData["MensajeError"] =
                        "El usuario no existe.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar el usuario.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                var usuario =
                    response.Content
                        .ReadFromJsonAsync
                        <UsuarioResponse>()
                        .Result;


                if (usuario == null)
                {
                    TempData["MensajeError"] =
                        "No fue posible cargar el usuario.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                if (usuario.Rol ==
                    "Administrador")
                {
                    TempData["MensajeError"] =
                        "La cuenta Administrador no se modifica desde este módulo.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                var model =
                    new ModificarUsuarioViewModel
                    {
                        IdUsuario =
                            usuario.IdUsuario,

                        IdRol =
                            usuario.IdRol,

                        Nombre =
                            usuario.Nombre,

                        Correo =
                            usuario.Correo,

                        RolActual =
                            usuario.Rol,

                        Roles =
                            ConsultarRoles(
                                client
                            )
                    };


                return View(
                    model
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al cargar el usuario {IdUsuario}.",
                    id
                );


                TempData["MensajeError"] =
                    "No fue posible cargar el usuario.";


                return RedirectToAction(
                    "Index"
                );
            }
        }


        // =========================================================
        // MODIFICAR - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Modificar(
            ModificarUsuarioViewModel model)
        {
            var client =
                CrearClienteAPI();


            if (!ModelState.IsValid)
            {
                model.Roles =
                    ConsultarRoles(
                        client
                    );


                return View(
                    model
                );
            }


            try
            {
                var request =
                    new ModificarUsuarioRequest
                    {
                        IdUsuario =
                            model.IdUsuario,

                        IdRol =
                            model.IdRol,

                        Nombre =
                            model.Nombre.Trim(),

                        Correo =
                            model.Correo.Trim()
                    };


                var response =
                    client.PutAsJsonAsync(
                        "api/Usuario/ModificarUsuarioAPI",
                        request
                    ).Result;


                var autorizacion =
                    ManejarAutorizacion(
                        response
                    );


                if (autorizacion != null)
                {
                    return autorizacion;
                }


                if (response.StatusCode ==
                    HttpStatusCode.OK)
                {
                    var resultado =
                        response.Content
                            .ReadFromJsonAsync
                            <MensajeResponse>()
                            .Result;


                    TempData["MensajeExito"] =
                        resultado?.Mensaje
                        ??
                        "Usuario modificado correctamente.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                var error =
                    response.Content
                        .ReadFromJsonAsync
                        <MensajeResponse>()
                        .Result;


                ViewBag.Mensaje =
                    error?.Mensaje
                    ??
                    "No fue posible modificar el usuario.";


                model.Roles =
                    ConsultarRoles(
                        client
                    );


                return View(
                    model
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al modificar un usuario desde StockFlowWEB."
                );


                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                model.Roles =
                    ConsultarRoles(
                        client
                    );


                return View(
                    model
                );
            }
        }


        // =========================================================
        // CAMBIAR ESTADO
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(
            long id)
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var response =
                    client.PutAsync(
                        $"api/Usuario/CambiarEstadoUsuarioAPI/{id}",
                        null
                    ).Result;


                var autorizacion =
                    ManejarAutorizacion(
                        response
                    );


                if (autorizacion != null)
                {
                    return autorizacion;
                }


                var resultado =
                    response.Content
                        .ReadFromJsonAsync
                        <MensajeResponse>()
                        .Result;


                if (response.IsSuccessStatusCode)
                {
                    TempData["MensajeExito"] =
                        resultado?.Mensaje
                        ??
                        "Estado actualizado correctamente.";
                }
                else
                {
                    TempData["MensajeError"] =
                        resultado?.Mensaje
                        ??
                        "No fue posible actualizar el usuario.";
                }


                return RedirectToAction(
                    "Index"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al cambiar el estado del usuario {IdUsuario}.",
                    id
                );


                TempData["MensajeError"] =
                    "No fue posible comunicarse con StockFlowAPI.";


                return RedirectToAction(
                    "Index"
                );
            }
        }
    }
}