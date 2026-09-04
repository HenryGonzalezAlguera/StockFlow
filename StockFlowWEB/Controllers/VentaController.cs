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
    public class VentaController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public VentaController(
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
        // CLIENTES ACTIVOS
        // =========================================================

        private List<ClienteResponse> ConsultarClientes(
            HttpClient client)
        {
            var response =
                client.GetAsync(
                    "api/Cliente/ConsultarClientesAPI"
                ).Result;


            if (!response.IsSuccessStatusCode)
            {
                return new List<ClienteResponse>();
            }


            return response.Content
                .ReadFromJsonAsync
                <List<ClienteResponse>>()
                .Result?
                .Where(x => x.Estado)
                .OrderBy(x => x.Nombre)
                .ToList()
                ??
                new List<ClienteResponse>();
        }


        // =========================================================
        // PRODUCTOS ACTIVOS
        // =========================================================

        private List<ProductoResponse> ConsultarProductos(
            HttpClient client)
        {
            var response =
                client.GetAsync(
                    "api/Producto/ConsultarProductosAPI"
                ).Result;


            if (!response.IsSuccessStatusCode)
            {
                return new List<ProductoResponse>();
            }


            return response.Content
                .ReadFromJsonAsync
                <List<ProductoResponse>>()
                .Result?
                .Where(x => x.Estado)
                .OrderBy(x => x.Nombre)
                .ToList()
                ??
                new List<ProductoResponse>();
        }


        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public IActionResult Index(
            string? busqueda,
            long? idCliente,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int pagina = 1,
            int cantidadPagina = 10)
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var clientes =
                    ConsultarClientes(
                        client
                    );


                var request =
                    new ConsultarVentasPaginadoRequest
                    {
                        Busqueda =
                            busqueda,

                        IdCliente =
                            idCliente,

                        FechaDesde =
                            fechaDesde,

                        FechaHasta =
                            fechaHasta,

                        Pagina =
                            pagina,

                        CantidadPagina =
                            cantidadPagina
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Venta/ConsultarVentasPaginadoAPI",
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
                        "No fue posible consultar las ventas.";


                    return View(
                        new VentaIndexViewModel
                        {
                            Clientes =
                                clientes
                        }
                    );
                }


                var resultado =
                    response.Content
                        .ReadFromJsonAsync
                        <PaginacionResponse<VentaResponse>>()
                        .Result;


                resultado ??=
                    new PaginacionResponse<VentaResponse>();


                var model =
                    new VentaIndexViewModel
                    {
                        Ventas =
                            resultado.Registros,

                        Clientes =
                            clientes,

                        Busqueda =
                            busqueda,

                        IdCliente =
                            idCliente,

                        FechaDesde =
                            fechaDesde,

                        FechaHasta =
                            fechaHasta,

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
                    new VentaIndexViewModel()
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
            try
            {
                var client =
                    CrearClienteAPI();


                var model =
                    new VentaFormViewModel
                    {
                        PorcentajeImpuesto =
                            13,

                        Clientes =
                            ConsultarClientes(
                                client
                            ),

                        Productos =
                            ConsultarProductos(
                                client
                            )
                    };


                return View(model);
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar el formulario de venta.";


                return RedirectToAction(
                    "Index"
                );
            }
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
            VentaFormViewModel model)
        {
            var client =
                CrearClienteAPI();


            if (model.Detalles == null ||
                model.Detalles.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Debe agregar al menos un producto."
                );
            }


            if (!ModelState.IsValid)
            {
                model.Clientes =
                    ConsultarClientes(
                        client
                    );


                model.Productos =
                    ConsultarProductos(
                        client
                    );


                return View(model);
            }


            try
            {
                var request =
                    new RegistrarVentaRequest
                    {
                        IdCliente =
                            model.IdCliente,

                        PorcentajeImpuesto =
                            model.PorcentajeImpuesto,

                        Detalles =
                            model.Detalles
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Venta/RegistrarVentaAPI",
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
                        "No tiene permisos para registrar ventas.";


                    model.Clientes =
                        ConsultarClientes(client);

                    model.Productos =
                        ConsultarProductos(client);


                    return View(model);
                }


                // STOCK INSUFICIENTE

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
                        "Uno o más productos no tienen stock suficiente.";


                    model.Clientes =
                        ConsultarClientes(client);

                    model.Productos =
                        ConsultarProductos(client);


                    return View(model);
                }


                if (response.StatusCode ==
                    HttpStatusCode.Created)
                {
                    var resultado =
                        response.Content
                            .ReadFromJsonAsync
                            <RegistrarVentaResponse>()
                            .Result;


                    TempData["MensajeExito"] =
                        resultado == null
                        ? "Venta registrada correctamente."
                        : $"Venta {resultado.NumeroVenta} registrada correctamente.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                var mensajeError =
                    response.Content
                        .ReadFromJsonAsync
                        <MensajeResponse>()
                        .Result;


                ViewBag.Mensaje =
                    mensajeError?.Mensaje
                    ??
                    "No fue posible registrar la venta.";


                model.Clientes =
                    ConsultarClientes(client);

                model.Productos =
                    ConsultarProductos(client);


                return View(model);
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                model.Clientes =
                    ConsultarClientes(client);

                model.Productos =
                    ConsultarProductos(client);


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
                        $"api/Venta/ConsultarVentaAPI/{id}"
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
                        "La venta no existe.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar la venta.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                var venta =
                    response.Content
                        .ReadFromJsonAsync
                        <VentaCompletaResponse>()
                        .Result;


                if (venta == null)
                {
                    return RedirectToAction(
                        "Index"
                    );
                }


                return View(
                    venta
                );
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar la venta.";


                return RedirectToAction(
                    "Index"
                );
            }
        }
    }
}
