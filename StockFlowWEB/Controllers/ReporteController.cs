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
    public class ReporteController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public ReporteController(
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
        public IActionResult Index()
        {
            return View();
        }


        // =========================================================
        // REPORTE VENTAS
        // =========================================================

        [HttpGet]
        public IActionResult Ventas(
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            try
            {
                var desde =
                    fechaDesde?.Date
                    ??
                    new DateTime(
                        DateTime.Today.Year,
                        DateTime.Today.Month,
                        1
                    );


                var hasta =
                    fechaHasta?.Date
                    ??
                    DateTime.Today;


                if (desde > hasta)
                {
                    ViewBag.Mensaje =
                        "La fecha inicial no puede ser mayor que la fecha final.";

                    return View(
                        new ReporteVentasViewModel
                        {
                            FechaDesde = desde,
                            FechaHasta = hasta
                        }
                    );
                }


                var client =
                    CrearClienteAPI();


                var request =
                    new ReporteFechasRequest
                    {
                        FechaDesde =
                            desde,

                        FechaHasta =
                            hasta
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Reporte/VentasAPI",
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
                        "No fue posible consultar el reporte de ventas.";

                    return View(
                        new ReporteVentasViewModel
                        {
                            FechaDesde = desde,
                            FechaHasta = hasta
                        }
                    );
                }


                var ventas =
                    response.Content
                        .ReadFromJsonAsync
                        <List<ReporteVentaResponse>>()
                        .Result
                    ??
                    new List<ReporteVentaResponse>();


                return View(
                    new ReporteVentasViewModel
                    {
                        FechaDesde =
                            desde,

                        FechaHasta =
                            hasta,

                        Ventas =
                            ventas
                    }
                );
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";

                return View(
                    new ReporteVentasViewModel
                    {
                        FechaDesde =
                            DateTime.Today,

                        FechaHasta =
                            DateTime.Today
                    }
                );
            }
        }


        // =========================================================
        // REPORTE COMPRAS
        // =========================================================

        [HttpGet]
        public IActionResult Compras(
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            try
            {
                var desde =
                    fechaDesde?.Date
                    ??
                    new DateTime(
                        DateTime.Today.Year,
                        DateTime.Today.Month,
                        1
                    );


                var hasta =
                    fechaHasta?.Date
                    ??
                    DateTime.Today;


                if (desde > hasta)
                {
                    ViewBag.Mensaje =
                        "La fecha inicial no puede ser mayor que la fecha final.";

                    return View(
                        new ReporteComprasViewModel
                        {
                            FechaDesde = desde,
                            FechaHasta = hasta
                        }
                    );
                }


                var client =
                    CrearClienteAPI();


                var request =
                    new ReporteFechasRequest
                    {
                        FechaDesde = desde,
                        FechaHasta = hasta
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Reporte/ComprasAPI",
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
                        "No fue posible consultar el reporte de compras.";

                    return View(
                        new ReporteComprasViewModel
                        {
                            FechaDesde = desde,
                            FechaHasta = hasta
                        }
                    );
                }


                var compras =
                    response.Content
                        .ReadFromJsonAsync
                        <List<ReporteCompraResponse>>()
                        .Result
                    ??
                    new List<ReporteCompraResponse>();


                return View(
                    new ReporteComprasViewModel
                    {
                        FechaDesde =
                            desde,

                        FechaHasta =
                            hasta,

                        Compras =
                            compras
                    }
                );
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";

                return View(
                    new ReporteComprasViewModel
                    {
                        FechaDesde =
                            DateTime.Today,

                        FechaHasta =
                            DateTime.Today
                    }
                );
            }
        }


        // =========================================================
        // INVENTARIO ACTUAL
        // =========================================================

        [HttpGet]
        public IActionResult InventarioActual()
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var response =
                    client.GetAsync(
                        "api/Reporte/InventarioActualAPI"
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
                        "No fue posible consultar el inventario.";

                    return View(
                        new List<ReporteInventarioResponse>()
                    );
                }


                var inventario =
                    response.Content
                        .ReadFromJsonAsync
                        <List<ReporteInventarioResponse>>()
                        .Result
                    ??
                    new List<ReporteInventarioResponse>();


                return View(inventario);
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";

                return View(
                    new List<ReporteInventarioResponse>()
                );
            }
        }


        // =========================================================
        // STOCK BAJO
        // =========================================================

        [HttpGet]
        public IActionResult StockBajo()
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var response =
                    client.GetAsync(
                        "api/Reporte/StockBajoAPI"
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
                        "No fue posible consultar el stock bajo.";

                    return View(
                        new List<ReporteStockBajoResponse>()
                    );
                }


                var productos =
                    response.Content
                        .ReadFromJsonAsync
                        <List<ReporteStockBajoResponse>>()
                        .Result
                    ??
                    new List<ReporteStockBajoResponse>();


                return View(productos);
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";

                return View(
                    new List<ReporteStockBajoResponse>()
                );
            }
        }


        // =========================================================
        // PRODUCTOS MAS VENDIDOS
        // =========================================================

        [HttpGet]
        public IActionResult ProductosMasVendidos(
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            try
            {
                var desde =
                    fechaDesde?.Date
                    ??
                    new DateTime(
                        DateTime.Today.Year,
                        DateTime.Today.Month,
                        1
                    );


                var hasta =
                    fechaHasta?.Date
                    ??
                    DateTime.Today;


                if (desde > hasta)
                {
                    ViewBag.Mensaje =
                        "La fecha inicial no puede ser mayor que la fecha final.";

                    return View(
                        new ReporteProductosVendidosViewModel
                        {
                            FechaDesde = desde,
                            FechaHasta = hasta
                        }
                    );
                }


                var client =
                    CrearClienteAPI();


                var request =
                    new ReporteFechasRequest
                    {
                        FechaDesde =
                            desde,

                        FechaHasta =
                            hasta
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Reporte/ProductosMasVendidosAPI",
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
                        "No fue posible consultar los productos más vendidos.";

                    return View(
                        new ReporteProductosVendidosViewModel
                        {
                            FechaDesde = desde,
                            FechaHasta = hasta
                        }
                    );
                }


                var productos =
                    response.Content
                        .ReadFromJsonAsync
                        <List<ReporteProductoVendidoResponse>>()
                        .Result
                    ??
                    new List<ReporteProductoVendidoResponse>();


                return View(
                    new ReporteProductosVendidosViewModel
                    {
                        FechaDesde =
                            desde,

                        FechaHasta =
                            hasta,

                        Productos =
                            productos
                    }
                );
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";

                return View(
                    new ReporteProductosVendidosViewModel
                    {
                        FechaDesde =
                            DateTime.Today,

                        FechaHasta =
                            DateTime.Today
                    }
                );
            }
        }
    }
}