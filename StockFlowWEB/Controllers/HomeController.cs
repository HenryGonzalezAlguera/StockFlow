using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StockFlowWEB.Models;
using StockFlowWEB.Models.Request;
using StockFlowWEB.Models.Response;
using StockFlowWEB.Models.ViewModels;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

namespace StockFlowWEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;

        private readonly ILogger<HomeController>
            _logger;


        public HomeController(
            IHttpClientFactory httpClientFactory,
            ILogger<HomeController> logger)
        {
            _httpClientFactory =
                httpClientFactory;

            _logger =
                logger;
        }


        // =========================================================
        // VALIDAR SI YA EXISTE SESIÓN
        // =========================================================

        private bool ExisteSesionValida()
        {
            var autenticado =
                HttpContext.Session
                    .GetString(
                        "Autenticado"
                    );


            var token =
                HttpContext.Session
                    .GetString(
                        "Token"
                    );


            return autenticado == "true"
                &&
                !string.IsNullOrWhiteSpace(
                    token
                );
        }


        // =========================================================
        // LIMPIAR SESIÓN INCONSISTENTE
        // =========================================================

        private void LimpiarSesionInconsistente()
        {
            var autenticado =
                HttpContext.Session
                    .GetString(
                        "Autenticado"
                    );


            var token =
                HttpContext.Session
                    .GetString(
                        "Token"
                    );


            if (autenticado == "true" ||
                !string.IsNullOrWhiteSpace(
                    token
                ))
            {
                HttpContext.Session.Clear();
            }
        }


        // =========================================================
        // LOGIN - GET
        // =========================================================

        [HttpGet]
        public IActionResult Index()
        {
            if (ExisteSesionValida())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );
            }


            LimpiarSesionInconsistente();


            return View(
                new LoginRequest()
            );
        }


        // =========================================================
        // LOGIN - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(
            LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(
                    request
                );
            }


            try
            {
                var client =
                    _httpClientFactory
                        .CreateClient(
                            "StockFlowAPI"
                        );


                var response =
                    client.PostAsJsonAsync(
                        "api/Home/LoginAPI",
                        request
                    ).Result;


                // =================================================
                // LOGIN CORRECTO
                // =================================================

                if (response.StatusCode ==
                    HttpStatusCode.OK)
                {
                    var login =
                        response.Content
                            .ReadFromJsonAsync
                            <LoginResponse>()
                            .Result;


                    if (login == null ||
                        login.IdUsuario <= 0 ||
                        login.IdEmpresa <= 0 ||
                        string.IsNullOrWhiteSpace(
                            login.Token
                        ))
                    {
                        HttpContext.Session.Clear();


                        ViewBag.Mensaje =
                            "La API no devolvió una sesión válida.";


                        return View(
                            request
                        );
                    }


                    // =============================================
                    // LIMPIAR SESIÓN ANTERIOR
                    // =============================================

                    HttpContext.Session.Clear();


                    // =============================================
                    // GUARDAR SESIÓN
                    // =============================================

                    HttpContext.Session
                        .SetString(
                            "Autenticado",
                            "true"
                        );


                    HttpContext.Session
                        .SetString(
                            "Token",
                            login.Token
                        );


                    HttpContext.Session
                        .SetString(
                            "IdUsuario",
                            login.IdUsuario
                                .ToString()
                        );


                    HttpContext.Session
                        .SetString(
                            "IdEmpresa",
                            login.IdEmpresa
                                .ToString()
                        );


                    HttpContext.Session
                        .SetString(
                            "Nombre",
                            string.IsNullOrWhiteSpace(
                                login.NombreUsuario
                            )
                                ? "Usuario"
                                : login.NombreUsuario
                        );


                    HttpContext.Session
                        .SetString(
                            "NombreEmpresa",
                            string.IsNullOrWhiteSpace(
                                login.NombreEmpresa
                            )
                                ? "StockFlow"
                                : login.NombreEmpresa
                        );


                    HttpContext.Session
                        .SetString(
                            "Rol",
                            string.IsNullOrWhiteSpace(
                                login.Rol
                            )
                                ? "Consulta"
                                : login.Rol
                        );


                    return RedirectToAction(
                        "Index",
                        "Dashboard"
                    );
                }


                // =================================================
                // LOGIN INCORRECTO
                // =================================================

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();


                    var error =
                        response.Content
                            .ReadFromJsonAsync
                            <MensajeResponse>()
                            .Result;


                    ViewBag.Mensaje =
                        error?.Mensaje
                        ??
                        "Empresa, correo o contraseña incorrectos.";


                    return View(
                        request
                    );
                }


                // =================================================
                // BAD REQUEST
                // =================================================

                if (response.StatusCode ==
                    HttpStatusCode.BadRequest)
                {
                    HttpContext.Session.Clear();


                    var error =
                        response.Content
                            .ReadFromJsonAsync
                            <MensajeResponse>()
                            .Result;


                    ViewBag.Mensaje =
                        error?.Mensaje
                        ??
                        "Los datos ingresados no son válidos.";


                    return View(
                        request
                    );
                }


                // =================================================
                // OTROS ERRORES
                // =================================================

                HttpContext.Session.Clear();


                var mensajeError =
                    response.Content
                        .ReadFromJsonAsync
                        <MensajeResponse>()
                        .Result;


                ViewBag.Mensaje =
                    mensajeError?.Mensaje
                    ??
                    "No fue posible iniciar sesión.";


                return View(
                    request
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al comunicarse con StockFlowAPI durante el inicio de sesión."
                );


                HttpContext.Session.Clear();


                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                return View(
                    request
                );
            }
        }


        // =========================================================
        // REGISTRAR EMPRESA - GET
        // =========================================================

        [HttpGet]
        public IActionResult Registrar()
        {
            // =====================================================
            // UN USUARIO YA AUTENTICADO NO NECESITA REGISTRARSE
            // =====================================================

            if (ExisteSesionValida())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );
            }


            LimpiarSesionInconsistente();


            return View(
                new RegistrarEmpresaViewModel()
            );
        }


        // =========================================================
        // REGISTRAR EMPRESA - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(
            RegistrarEmpresaViewModel model)
        {
            // =====================================================
            // SI YA ESTÁ AUTENTICADO
            // =====================================================

            if (ExisteSesionValida())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );
            }


            // =====================================================
            // VALIDAR FORMULARIO
            // =====================================================

            if (!ModelState.IsValid)
            {
                return View(
                    model
                );
            }


            try
            {
                // =================================================
                // REQUEST API
                // =================================================

                var request =
                    new RegistrarEmpresaRequest
                    {
                        IdentificacionEmpresa =
                            model
                                .IdentificacionEmpresa
                                .Trim(),

                        NombreEmpresa =
                            model
                                .NombreEmpresa
                                .Trim(),

                        CorreoEmpresa =
                            string.IsNullOrWhiteSpace(
                                model.CorreoEmpresa
                            )
                                ? null
                                : model
                                    .CorreoEmpresa
                                    .Trim(),

                        TelefonoEmpresa =
                            string.IsNullOrWhiteSpace(
                                model.TelefonoEmpresa
                            )
                                ? null
                                : model
                                    .TelefonoEmpresa
                                    .Trim(),

                        DireccionEmpresa =
                            string.IsNullOrWhiteSpace(
                                model.DireccionEmpresa
                            )
                                ? null
                                : model
                                    .DireccionEmpresa
                                    .Trim(),

                        NombreAdministrador =
                            model
                                .NombreAdministrador
                                .Trim(),

                        CorreoAdministrador =
                            model
                                .CorreoAdministrador
                                .Trim(),

                        Contrasena =
                            model.Contrasena
                    };


                // =================================================
                // CLIENTE HTTP
                // =================================================

                var client =
                    _httpClientFactory
                        .CreateClient(
                            "StockFlowAPI"
                        );


                // =================================================
                // REGISTRAR
                // =================================================

                var response =
                    client.PostAsJsonAsync(
                        "api/Home/RegistrarEmpresaAPI",
                        request
                    ).Result;


                // =================================================
                // 201 CREATED
                // =================================================

                if (response.StatusCode ==
                    HttpStatusCode.Created)
                {
                    var resultado =
                        response.Content
                            .ReadFromJsonAsync
                            <RegistrarEmpresaResponse>()
                            .Result;


                    TempData["MensajeExito"] =
                        resultado?.Mensaje
                        ??
                        "Empresa registrada correctamente. Ya puede iniciar sesión.";


                    return RedirectToAction(
                        "Index",
                        "Home"
                    );
                }


                // =================================================
                // 409 CONFLICT
                //
                // Ejemplo:
                // empresa con identificación repetida.
                // =================================================

                if (response.StatusCode ==
                    HttpStatusCode.Conflict)
                {
                    var error =
                        response.Content
                            .ReadFromJsonAsync
                            <MensajeResponse>()
                            .Result;


                    ViewBag.Mensaje =
                        error?.Mensaje
                        ??
                        "Ya existe una empresa con esos datos.";


                    return View(
                        model
                    );
                }


                // =================================================
                // 400 BAD REQUEST
                // =================================================

                if (response.StatusCode ==
                    HttpStatusCode.BadRequest)
                {
                    var error =
                        response.Content
                            .ReadFromJsonAsync
                            <MensajeResponse>()
                            .Result;


                    ViewBag.Mensaje =
                        error?.Mensaje
                        ??
                        "Los datos ingresados no son válidos.";


                    return View(
                        model
                    );
                }


                // =================================================
                // OTRO ERROR
                // =================================================

                var mensajeError =
                    response.Content
                        .ReadFromJsonAsync
                        <MensajeResponse>()
                        .Result;


                ViewBag.Mensaje =
                    mensajeError?.Mensaje
                    ??
                    "No fue posible registrar la empresa.";


                return View(
                    model
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al comunicarse con StockFlowAPI durante el registro de empresa."
                );


                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                return View(
                    model
                );
            }
        }


        // =========================================================
        // ERROR 500
        // =========================================================

        [HttpGet]
        [ResponseCache(
            Duration = 0,
            Location =
                ResponseCacheLocation.None,
            NoStore = true
        )]
        public IActionResult Error()
        {
            var exceptionFeature =
                HttpContext.Features
                    .Get<IExceptionHandlerPathFeature>();


            if (exceptionFeature?.Error != null)
            {
                _logger.LogError(
                    exceptionFeature.Error,
                    "Error no controlado en StockFlowWEB. Ruta: {Ruta}",
                    exceptionFeature.Path
                );
            }


            Response.StatusCode =
                StatusCodes
                    .Status500InternalServerError;


            var model =
                new ErrorViewModel
                {
                    StatusCode =
                        StatusCodes
                            .Status500InternalServerError,

                    Titulo =
                        "Algo salió mal",

                    Mensaje =
                        "No fue posible completar la operación. Puede volver a StockFlow e intentarlo nuevamente.",

                    RequestId =
                        Activity.Current?.Id
                        ??
                        HttpContext.TraceIdentifier
                };


            return View(
                "Error",
                model
            );
        }


        // =========================================================
        // ESTADOS HTTP
        // =========================================================

        [HttpGet]
        [Route("Home/Estado/{codigo:int}")]
        [ResponseCache(
            Duration = 0,
            Location =
                ResponseCacheLocation.None,
            NoStore = true
        )]
        public IActionResult Estado(
            int codigo)
        {
            var titulo =
                "No fue posible completar la solicitud";


            var mensaje =
                "Ocurrió un problema al procesar la solicitud.";


            switch (codigo)
            {
                case StatusCodes.Status400BadRequest:

                    titulo =
                        "Solicitud no válida";

                    mensaje =
                        "La solicitud enviada no es válida.";

                    break;


                case StatusCodes.Status401Unauthorized:

                    titulo =
                        "Sesión no válida";

                    mensaje =
                        "Debe iniciar sesión para acceder a este recurso.";

                    break;


                case StatusCodes.Status403Forbidden:

                    titulo =
                        "Acceso denegado";

                    mensaje =
                        "No tiene permisos para acceder a este recurso.";

                    break;


                case StatusCodes.Status404NotFound:

                    titulo =
                        "Página no encontrada";

                    mensaje =
                        "La página o recurso que está buscando no existe o ya no está disponible.";

                    break;


                case StatusCodes.Status500InternalServerError:

                    titulo =
                        "Algo salió mal";

                    mensaje =
                        "Ocurrió un error interno al procesar la solicitud.";

                    break;
            }


            Response.StatusCode =
                codigo;


            var model =
                new ErrorViewModel
                {
                    StatusCode =
                        codigo,

                    Titulo =
                        titulo,

                    Mensaje =
                        mensaje,

                    RequestId =
                        Activity.Current?.Id
                        ??
                        HttpContext.TraceIdentifier
                };


            return View(
                "Error",
                model
            );
        }


        // =========================================================
        // CERRAR SESIÓN
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();


            return RedirectToAction(
                "Index",
                "Home"
            );
        }
    }
}