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
    public class InventarioController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public InventarioController(
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
        // PRODUCTOS
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
        // TIPOS MOVIMIENTO
        // =========================================================

        private List<TipoMovimientoResponse> ConsultarTipos(
            HttpClient client)
        {
            var response =
                client.GetAsync(
                    "api/Inventario/ConsultarTiposMovimientoAPI"
                ).Result;


            if (!response.IsSuccessStatusCode)
            {
                return new List<TipoMovimientoResponse>();
            }


            return response.Content
                .ReadFromJsonAsync
                <List<TipoMovimientoResponse>>()
                .Result
                ??
                new List<TipoMovimientoResponse>();
        }


        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public IActionResult Index(
            string? busqueda,
            long? idProducto,
            long? idTipoMovimiento,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int pagina = 1,
            int cantidadPagina = 10)
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var productos =
                    ConsultarProductos(
                        client
                    );


                var tipos =
                    ConsultarTipos(
                        client
                    );


                var request =
                    new ConsultarMovimientosPaginadoRequest
                    {
                        Busqueda =
                            busqueda,

                        IdProducto =
                            idProducto,

                        IdTipoMovimiento =
                            idTipoMovimiento,

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
                        "api/Inventario/ConsultarMovimientosPaginadoAPI",
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
                        "No fue posible consultar los movimientos.";


                    return View(
                        new InventarioIndexViewModel
                        {
                            Productos =
                                productos,

                            TiposMovimiento =
                                tipos
                        }
                    );
                }


                var resultado =
                    response.Content
                        .ReadFromJsonAsync
                        <PaginacionResponse<MovimientoInventarioResponse>>()
                        .Result;


                resultado ??=
                    new PaginacionResponse<MovimientoInventarioResponse>();


                return View(
                    new InventarioIndexViewModel
                    {
                        Movimientos =
                            resultado.Registros,

                        Productos =
                            productos,

                        TiposMovimiento =
                            tipos,

                        Busqueda =
                            busqueda,

                        IdProducto =
                            idProducto,

                        IdTipoMovimiento =
                            idTipoMovimiento,

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
                    }
                );
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                return View(
                    new InventarioIndexViewModel()
                );
            }
        }


        // =========================================================
        // AJUSTAR GET
        // =========================================================

        [HttpGet]
        [SessionAuthorize(
            "Administrador"
        )]
        public IActionResult Ajustar()
        {
            var rol =
                HttpContext.Session
                    .GetString("Rol");


            if (rol != "Administrador")
            {
                TempData["MensajeError"] =
                    "Solo un administrador puede realizar ajustes de inventario.";


                return RedirectToAction(
                    "Index"
                );
            }


            try
            {
                var client =
                    CrearClienteAPI();


                return View(
                    new AjusteInventarioViewModel
                    {
                        Productos =
                            ConsultarProductos(
                                client
                            )
                    }
                );
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar el formulario de ajuste.";


                return RedirectToAction(
                    "Index"
                );
            }
        }


        // =========================================================
        // AJUSTAR POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(
            "Administrador"
        )]
        public IActionResult Ajustar(
            AjusteInventarioViewModel model)
        {
            var rol =
                HttpContext.Session
                    .GetString("Rol");


            if (rol != "Administrador")
            {
                return RedirectToAction(
                    "Index"
                );
            }


            var client =
                CrearClienteAPI();


            if (!ModelState.IsValid)
            {
                model.Productos =
                    ConsultarProductos(
                        client
                    );


                return View(model);
            }


            try
            {
                var request =
                    new AjustarInventarioRequest
                    {
                        IdProducto =
                            model.IdProducto,

                        TipoAjuste =
                            model.TipoAjuste,

                        Cantidad =
                            model.Cantidad,

                        Motivo =
                            model.Motivo.Trim()
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Inventario/AjustarInventarioAPI",
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
                        "No tiene permisos para realizar ajustes.";


                    model.Productos =
                        ConsultarProductos(client);


                    return View(model);
                }


                if (response.IsSuccessStatusCode)
                {
                    var resultado =
                        response.Content
                            .ReadFromJsonAsync
                            <AjusteInventarioResponse>()
                            .Result;


                    TempData["MensajeExito"] =
                        resultado == null
                        ? "Ajuste realizado correctamente."
                        : $"Ajuste realizado correctamente. Stock: {resultado.StockAnterior:N3} → {resultado.StockNuevo:N3}";


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
                    "No fue posible realizar el ajuste.";


                model.Productos =
                    ConsultarProductos(client);


                return View(model);
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                model.Productos =
                    ConsultarProductos(client);


                return View(model);
            }
        }
    }
}
