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
    public class CompraController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public CompraController(
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
        // PROVEEDORES ACTIVOS
        // =========================================================

        private List<ProveedorResponse> ConsultarProveedores(
            HttpClient client)
        {
            var response =
                client.GetAsync(
                    "api/Proveedor/ConsultarProveedoresAPI"
                ).Result;


            if (!response.IsSuccessStatusCode)
            {
                return new List<ProveedorResponse>();
            }


            return response.Content
                .ReadFromJsonAsync
                <List<ProveedorResponse>>()
                .Result?
                .Where(x => x.Estado)
                .OrderBy(x => x.Nombre)
                .ToList()
                ??
                new List<ProveedorResponse>();
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
            long? idProveedor,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int pagina = 1,
            int cantidadPagina = 10)
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var proveedores =
                    ConsultarProveedores(
                        client
                    );


                var request =
                    new ConsultarComprasPaginadoRequest
                    {
                        Busqueda =
                            busqueda,

                        IdProveedor =
                            idProveedor,

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
                        "api/Compra/ConsultarComprasPaginadoAPI",
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
                        "No fue posible consultar las compras.";


                    return View(
                        new CompraIndexViewModel
                        {
                            Proveedores =
                                proveedores
                        }
                    );
                }


                var resultado =
                    response.Content
                        .ReadFromJsonAsync
                        <PaginacionResponse<CompraResponse>>()
                        .Result;


                resultado ??=
                    new PaginacionResponse<CompraResponse>();


                var model =
                    new CompraIndexViewModel
                    {
                        Compras =
                            resultado.Registros,

                        Proveedores =
                            proveedores,

                        Busqueda =
                            busqueda,

                        IdProveedor =
                            idProveedor,

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
                    new CompraIndexViewModel()
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
                    new CompraFormViewModel
                    {
                        PorcentajeImpuesto = 13,

                        Proveedores =
                            ConsultarProveedores(
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
                    "No fue posible cargar el formulario de compra.";

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
            CompraFormViewModel model)
        {
            var client =
                CrearClienteAPI();


            if (model.Detalles == null ||
                model.Detalles.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Debe agregar al menos un producto a la compra."
                );
            }


            if (!ModelState.IsValid)
            {
                model.Proveedores =
                    ConsultarProveedores(
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
                    new RegistrarCompraRequest
                    {
                        IdProveedor =
                            model.IdProveedor,

                        PorcentajeImpuesto =
                            model.PorcentajeImpuesto,

                        Detalles =
                            model.Detalles
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Compra/RegistrarCompraAPI",
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
                        "No tiene permisos para registrar compras.";

                    model.Proveedores =
                        ConsultarProveedores(client);

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
                            <RegistrarCompraResponse>()
                            .Result;


                    TempData["MensajeExito"] =
                        resultado == null
                        ? "Compra registrada correctamente."
                        : $"Compra {resultado.NumeroCompra} registrada correctamente.";


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
                    "No fue posible registrar la compra.";


                model.Proveedores =
                    ConsultarProveedores(
                        client
                    );

                model.Productos =
                    ConsultarProductos(
                        client
                    );


                return View(model);
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                model.Proveedores =
                    ConsultarProveedores(
                        client
                    );

                model.Productos =
                    ConsultarProductos(
                        client
                    );


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
                        $"api/Compra/ConsultarCompraAPI/{id}"
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
                        "La compra no existe.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar la compra.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                var compra =
                    response.Content
                        .ReadFromJsonAsync
                        <CompraCompletaResponse>()
                        .Result;


                if (compra == null)
                {
                    return RedirectToAction(
                        "Index"
                    );
                }


                return View(
                    compra
                );
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar la compra.";

                return RedirectToAction(
                    "Index"
                );
            }
        }
    }
}
