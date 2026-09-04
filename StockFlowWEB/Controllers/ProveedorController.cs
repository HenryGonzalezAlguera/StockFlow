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
    public class ProveedorController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public ProveedorController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =========================================================
        // CLIENTE API
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
        // INDEX
        // =========================================================

        [HttpGet]
        public IActionResult Index(
            string? busqueda,
            bool? estado,
            int pagina = 1,
            int cantidadPagina = 10)
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var request =
                    new ConsultarProveedoresPaginadoRequest
                    {
                        Busqueda =
                            busqueda,

                        Estado =
                            estado,

                        Pagina =
                            pagina,

                        CantidadPagina =
                            cantidadPagina
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Proveedor/ConsultarProveedoresPaginadoAPI",
                        request
                    ).Result;


                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction(
                        "Index",
                        "Home"
                    );
                }


                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Mensaje =
                        "No fue posible consultar los proveedores.";


                    return View(
                        new ProveedorIndexViewModel()
                    );
                }


                var resultado =
                    response.Content
                        .ReadFromJsonAsync
                        <PaginacionResponse<ProveedorResponse>>()
                        .Result;


                resultado ??=
                    new PaginacionResponse<ProveedorResponse>();


                var model =
                    new ProveedorIndexViewModel
                    {
                        Proveedores =
                            resultado.Registros,

                        Busqueda =
                            busqueda,

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


                return View(model);
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                return View(
                    new ProveedorIndexViewModel()
                );
            }
        }


        // =========================================================
        // REGISTRAR GET
        // =========================================================

        [HttpGet]
        [SessionAuthorize(
            "Administrador",
            "Empleado"
        )]
        public IActionResult Registrar()
        {
            return View(
                new ProveedorFormViewModel()
            );
        }


        // =========================================================
        // REGISTRAR POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(
            "Administrador",
            "Empleado"
        )]
        public IActionResult Registrar(
            ProveedorFormViewModel model)
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
                    new RegistrarProveedorRequest
                    {
                        Identificacion =
                            model.Identificacion?.Trim(),

                        Nombre =
                            model.Nombre.Trim(),

                        Correo =
                            model.Correo?.Trim(),

                        Telefono =
                            model.Telefono?.Trim(),

                        Direccion =
                            model.Direccion?.Trim()
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Proveedor/RegistrarProveedorAPI",
                        request
                    ).Result;


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
                    ViewBag.Mensaje =
                        "No tiene permisos para registrar proveedores.";

                    return View(model);
                }


                if (response.StatusCode ==
                    HttpStatusCode.Created)
                {
                    TempData["MensajeExito"] =
                        "Proveedor registrado correctamente.";


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
                    "No fue posible registrar el proveedor.";


                return View(model);
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                return View(model);
            }
        }


        // =========================================================
        // MODIFICAR GET
        // =========================================================

        [HttpGet]
        [SessionAuthorize(
            "Administrador",
            "Empleado"
        )]
        public IActionResult Modificar(
            long id)
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var response =
                    client.GetAsync(
                        $"api/Proveedor/ConsultarProveedorAPI/{id}"
                    ).Result;


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
                    HttpStatusCode.NotFound)
                {
                    TempData["MensajeError"] =
                        "El proveedor no existe.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar el proveedor.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                var proveedor =
                    response.Content
                        .ReadFromJsonAsync
                        <ProveedorResponse>()
                        .Result;


                if (proveedor == null)
                {
                    return RedirectToAction(
                        "Index"
                    );
                }


                var model =
                    new ProveedorFormViewModel
                    {
                        IdProveedor =
                            proveedor.IdProveedor,

                        Identificacion =
                            proveedor.Identificacion,

                        Nombre =
                            proveedor.Nombre,

                        Correo =
                            proveedor.Correo,

                        Telefono =
                            proveedor.Telefono,

                        Direccion =
                            proveedor.Direccion
                    };


                return View(model);
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar el proveedor.";


                return RedirectToAction(
                    "Index"
                );
            }
        }


        // =========================================================
        // MODIFICAR POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(
            "Administrador",
            "Empleado"
        )]
        public IActionResult Modificar(
            ProveedorFormViewModel model)
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
                    new ModificarProveedorRequest
                    {
                        IdProveedor =
                            model.IdProveedor,

                        Identificacion =
                            model.Identificacion?.Trim(),

                        Nombre =
                            model.Nombre.Trim(),

                        Correo =
                            model.Correo?.Trim(),

                        Telefono =
                            model.Telefono?.Trim(),

                        Direccion =
                            model.Direccion?.Trim()
                    };


                var response =
                    client.PutAsJsonAsync(
                        "api/Proveedor/ModificarProveedorAPI",
                        request
                    ).Result;


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
                    HttpStatusCode.OK)
                {
                    TempData["MensajeExito"] =
                        "Proveedor modificado correctamente.";


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
                    "No fue posible modificar el proveedor.";


                return View(model);
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                return View(model);
            }
        }


        // =========================================================
        // DETALLE
        // =========================================================

        [HttpGet]
        public IActionResult Detalle(
            long id)
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var response =
                    client.GetAsync(
                        $"api/Proveedor/ConsultarProveedorAPI/{id}"
                    ).Result;


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
                    HttpStatusCode.NotFound)
                {
                    TempData["MensajeError"] =
                        "El proveedor no existe.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar el proveedor.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                var proveedor =
                    response.Content
                        .ReadFromJsonAsync
                        <ProveedorResponse>()
                        .Result;


                if (proveedor == null)
                {
                    return RedirectToAction(
                        "Index"
                    );
                }


                return View(
                    proveedor
                );
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar el proveedor.";


                return RedirectToAction(
                    "Index"
                );
            }
        }


        // =========================================================
        // CAMBIAR ESTADO
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(
            "Administrador"
        )]
        public IActionResult CambiarEstado(
            long id)
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var response =
                    client.PutAsync(
                        $"api/Proveedor/CambiarEstadoProveedorAPI/{id}",
                        null
                    ).Result;


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
                    TempData["MensajeError"] =
                        "No tiene permisos para cambiar el estado del proveedor.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                if (response.IsSuccessStatusCode)
                {
                    var resultado =
                        response.Content
                            .ReadFromJsonAsync
                            <MensajeResponse>()
                            .Result;


                    TempData["MensajeExito"] =
                        resultado?.Mensaje
                        ??
                        "Estado actualizado correctamente.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                var error =
                    response.Content
                        .ReadFromJsonAsync
                        <MensajeResponse>()
                        .Result;


                TempData["MensajeError"] =
                    error?.Mensaje
                    ??
                    "No fue posible actualizar el proveedor.";


                return RedirectToAction(
                    "Index"
                );
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible comunicarse con StockFlowAPI.";


                return RedirectToAction(
                    "Index"
                );
            }
        }
    }
}
