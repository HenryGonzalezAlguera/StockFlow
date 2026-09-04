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
    [SessionAuthorize]
    public class CuentaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CuentaController> _logger;


        public CuentaController(
            IHttpClientFactory httpClientFactory,
            ILogger<CuentaController> logger)
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
        // MI CUENTA
        // =========================================================

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var response =
                    client.GetAsync(
                        "api/Usuario/ConsultarMiCuentaAPI"
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
                        "No fue posible consultar la información de la cuenta.";


                    return View(
                        new MiCuentaResponse()
                    );
                }


                var cuenta =
                    response.Content
                        .ReadFromJsonAsync
                        <MiCuentaResponse>()
                        .Result;


                if (cuenta == null)
                {
                    ViewBag.Mensaje =
                        "No fue posible consultar la información de la cuenta.";


                    return View(
                        new MiCuentaResponse()
                    );
                }


                // =================================================
                // SINCRONIZAR DATOS VISUALES DE SESIÓN
                // =================================================

                HttpContext.Session.SetString(
                    "Nombre",
                    cuenta.Nombre
                );


                HttpContext.Session.SetString(
                    "NombreEmpresa",
                    cuenta.NombreEmpresa
                );


                HttpContext.Session.SetString(
                    "Rol",
                    cuenta.Rol
                );


                return View(cuenta);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar Mi Cuenta desde StockFlowWEB."
                );


                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                return View(
                    new MiCuentaResponse()
                );
            }
        }


        // =========================================================
        // CAMBIAR CONTRASEÑA - GET
        // =========================================================

        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            return View(
                new CambiarContrasenaViewModel()
            );
        }


        // =========================================================
        // CAMBIAR CONTRASEÑA - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarContrasena(
            CambiarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            try
            {
                var client =
                    CrearClienteAPI();


                var request =
                    new CambiarContrasenaRequest
                    {
                        ContrasenaActual =
                            model.ContrasenaActual,

                        ContrasenaNueva =
                            model.ContrasenaNueva
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Usuario/CambiarContrasenaAPI",
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
                    // =============================================
                    // OBLIGAR NUEVO LOGIN EN ESTE NAVEGADOR
                    // =============================================

                    HttpContext.Session.Clear();


                    TempData["MensajeExito"] =
                        "Contraseña actualizada correctamente. Inicie sesión nuevamente.";


                    return RedirectToAction(
                        "Index",
                        "Home"
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
                    "No fue posible cambiar la contraseña.";


                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al cambiar la contraseña desde StockFlowWEB."
                );


                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                return View(model);
            }
        }
    }
}
