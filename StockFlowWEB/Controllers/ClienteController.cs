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
    public class ClienteController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public ClienteController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
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
                    new ConsultarClientesPaginadoRequest
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
                        "api/Cliente/ConsultarClientesPaginadoAPI",
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
                        "No fue posible consultar los clientes.";

                    return View(
                        new ClienteIndexViewModel()
                    );
                }


                var resultado =
                    response.Content
                        .ReadFromJsonAsync
                        <PaginacionResponse<ClienteResponse>>()
                        .Result;


                resultado ??=
                    new PaginacionResponse<ClienteResponse>();


                var model =
                    new ClienteIndexViewModel
                    {
                        Clientes =
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
                    new ClienteIndexViewModel()
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
                new ClienteFormViewModel()
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
            ClienteFormViewModel model)
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
                    new RegistrarClienteRequest
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
                        "api/Cliente/RegistrarClienteAPI",
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
                        "No tiene permisos para registrar clientes.";

                    return View(model);
                }


                if (response.StatusCode ==
                    HttpStatusCode.Created)
                {
                    TempData["MensajeExito"] =
                        "Cliente registrado correctamente.";

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
                    "No fue posible registrar el cliente.";


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
                        $"api/Cliente/ConsultarClienteAPI/{id}"
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
                        "El cliente no existe.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar el cliente.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                var cliente =
                    response.Content
                        .ReadFromJsonAsync
                        <ClienteResponse>()
                        .Result;


                if (cliente == null)
                {
                    return RedirectToAction(
                        "Index"
                    );
                }


                var model =
                    new ClienteFormViewModel
                    {
                        IdCliente =
                            cliente.IdCliente,

                        Identificacion =
                            cliente.Identificacion,

                        Nombre =
                            cliente.Nombre,

                        Correo =
                            cliente.Correo,

                        Telefono =
                            cliente.Telefono,

                        Direccion =
                            cliente.Direccion
                    };


                return View(model);
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar el cliente.";

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
            ClienteFormViewModel model)
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
                    new ModificarClienteRequest
                    {
                        IdCliente =
                            model.IdCliente,

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
                        "api/Cliente/ModificarClienteAPI",
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
                        "Cliente modificado correctamente.";

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
                    "No fue posible modificar el cliente.";


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
                        $"api/Cliente/ConsultarClienteAPI/{id}"
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
                        "El cliente no existe.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar el cliente.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                var cliente =
                    response.Content
                        .ReadFromJsonAsync
                        <ClienteResponse>()
                        .Result;


                if (cliente == null)
                {
                    return RedirectToAction(
                        "Index"
                    );
                }


                return View(cliente);
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar el cliente.";

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
                        $"api/Cliente/CambiarEstadoClienteAPI/{id}",
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
                        "No tiene permisos para cambiar el estado del cliente.";

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
                    "No fue posible actualizar el cliente.";


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
