using Microsoft.AspNetCore.Mvc;
using StockFlowWEB.Attributes;
using StockFlowWEB.Models.Request;
using StockFlowWEB.Models.Response;
using StockFlowWEB.Services;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StockFlowWEB.Controllers
{
    [SessionAuthorize]
    public class ReportePdfController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;

        private readonly ReportePdfService
            _reportePdfService;

        private readonly ILogger<ReportePdfController>
            _logger;


        private static readonly CultureInfo
            Cultura =
                CultureInfo.GetCultureInfo(
                    "es-CR"
                );


        public ReportePdfController(
            IHttpClientFactory httpClientFactory,
            ReportePdfService reportePdfService,
            ILogger<ReportePdfController> logger)
        {
            _httpClientFactory =
                httpClientFactory;

            _reportePdfService =
                reportePdfService;

            _logger =
                logger;
        }


        // =========================================================
        // HTTP CLIENT CON JWT
        // =========================================================

        private HttpClient CrearClienteAPI()
        {
            var token =
                HttpContext.Session
                    .GetString(
                        "Token"
                    );


            var client =
                _httpClientFactory
                    .CreateClient(
                        "StockFlowAPI"
                    );


            if (!string.IsNullOrWhiteSpace(
                token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token
                    );
            }


            return client;
        }


        // =========================================================
        // EMPRESA ACTUAL
        // =========================================================

        private string ObtenerEmpresa()
        {
            return HttpContext.Session
                .GetString(
                    "NombreEmpresa"
                )
                ??
                "StockFlow";
        }


        // =========================================================
        // FORMATO DE CANTIDAD
        // =========================================================

        private static string FormatearCantidad(
            decimal cantidad)
        {
            return cantidad.ToString(
                "0.###",
                Cultura
            );
        }


        // =========================================================
        // FORMATO DE MONEDA
        // =========================================================

        private static string FormatearMoneda(
            decimal monto)
        {
            return
                $"₡{monto.ToString("N2", Cultura)}";
        }


        // =========================================================
        // 401 / 403
        // =========================================================

        private IActionResult? ValidarAutorizacion(
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
        // VENTAS PDF
        // =========================================================

        [HttpGet]
        public IActionResult ExportarVentas(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            try
            {
                fechaDesde =
                    fechaDesde.Date;

                fechaHasta =
                    fechaHasta.Date;


                if (fechaDesde >
                    fechaHasta)
                {
                    TempData["Mensaje"] =
                        "La fecha inicial no puede ser mayor que la fecha final.";


                    return RedirectToAction(
                        "Ventas",
                        "Reporte"
                    );
                }


                var client =
                    CrearClienteAPI();


                var response =
                    client.PostAsJsonAsync(
                        "api/Reporte/VentasAPI",
                        new ReporteFechasRequest
                        {
                            FechaDesde =
                                fechaDesde,

                            FechaHasta =
                                fechaHasta
                        }
                    ).Result;


                var autorizacion =
                    ValidarAutorizacion(
                        response
                    );


                if (autorizacion != null)
                    return autorizacion;


                if (!response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] =
                        "No fue posible generar el PDF de ventas.";


                    return RedirectToAction(
                        "Ventas",
                        "Reporte",
                        new
                        {
                            fechaDesde,
                            fechaHasta
                        }
                    );
                }


                var datos =
                    response.Content
                        .ReadFromJsonAsync
                        <List<ReporteVentaResponse>>()
                        .Result
                    ??
                    new List<ReporteVentaResponse>();


                var filas =
                    datos.Select(
                        x =>
                            new[]
                            {
                                x.NumeroVenta,
                                x.FechaVenta.ToString(
                                    "dd/MM/yyyy HH:mm"
                                ),
                                x.Cliente,
                                x.Usuario,
                                FormatearMoneda(
                                    x.Subtotal
                                ),
                                FormatearMoneda(
                                    x.Impuesto
                                ),
                                FormatearMoneda(
                                    x.Total
                                )
                            }
                    );


                var pdf =
                    _reportePdfService
                        .GenerarReporte(
                            ObtenerEmpresa(),
                            "Reporte de ventas",
                            $"{fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}",
                            new[]
                            {
                                "Venta",
                                "Fecha",
                                "Cliente",
                                "Usuario",
                                "Subtotal",
                                "Impuesto",
                                "Total"
                            },
                            filas,
                            $"Total vendido: {FormatearMoneda(datos.Sum(x => x.Total))}"
                        );


                return File(
                    pdf,
                    "application/pdf",
                    $"StockFlow_Ventas_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el PDF de ventas."
                );


                TempData["Mensaje"] =
                    "No fue posible generar el PDF de ventas.";


                return RedirectToAction(
                    "Ventas",
                    "Reporte",
                    new
                    {
                        fechaDesde,
                        fechaHasta
                    }
                );
            }
        }


        // =========================================================
        // COMPRAS PDF
        // =========================================================

        [HttpGet]
        public IActionResult ExportarCompras(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            try
            {
                fechaDesde =
                    fechaDesde.Date;

                fechaHasta =
                    fechaHasta.Date;


                if (fechaDesde >
                    fechaHasta)
                {
                    TempData["Mensaje"] =
                        "La fecha inicial no puede ser mayor que la fecha final.";


                    return RedirectToAction(
                        "Compras",
                        "Reporte"
                    );
                }


                var client =
                    CrearClienteAPI();


                var response =
                    client.PostAsJsonAsync(
                        "api/Reporte/ComprasAPI",
                        new ReporteFechasRequest
                        {
                            FechaDesde =
                                fechaDesde,

                            FechaHasta =
                                fechaHasta
                        }
                    ).Result;


                var autorizacion =
                    ValidarAutorizacion(
                        response
                    );


                if (autorizacion != null)
                    return autorizacion;


                if (!response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] =
                        "No fue posible generar el PDF de compras.";


                    return RedirectToAction(
                        "Compras",
                        "Reporte",
                        new
                        {
                            fechaDesde,
                            fechaHasta
                        }
                    );
                }


                var datos =
                    response.Content
                        .ReadFromJsonAsync
                        <List<ReporteCompraResponse>>()
                        .Result
                    ??
                    new List<ReporteCompraResponse>();


                var filas =
                    datos.Select(
                        x =>
                            new[]
                            {
                                x.NumeroCompra,
                                x.FechaCompra.ToString(
                                    "dd/MM/yyyy HH:mm"
                                ),
                                x.Proveedor,
                                x.Usuario,
                                FormatearMoneda(
                                    x.Subtotal
                                ),
                                FormatearMoneda(
                                    x.Impuesto
                                ),
                                FormatearMoneda(
                                    x.Total
                                )
                            }
                    );


                var pdf =
                    _reportePdfService
                        .GenerarReporte(
                            ObtenerEmpresa(),
                            "Reporte de compras",
                            $"{fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}",
                            new[]
                            {
                                "Compra",
                                "Fecha",
                                "Proveedor",
                                "Usuario",
                                "Subtotal",
                                "Impuesto",
                                "Total"
                            },
                            filas,
                            $"Total comprado: {FormatearMoneda(datos.Sum(x => x.Total))}"
                        );


                return File(
                    pdf,
                    "application/pdf",
                    $"StockFlow_Compras_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el PDF de compras."
                );


                TempData["Mensaje"] =
                    "No fue posible generar el PDF de compras.";


                return RedirectToAction(
                    "Compras",
                    "Reporte",
                    new
                    {
                        fechaDesde,
                        fechaHasta
                    }
                );
            }
        }


        // =========================================================
        // INVENTARIO ACTUAL PDF
        // =========================================================

        [HttpGet]
        public IActionResult ExportarInventario()
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var response =
                    client.GetAsync(
                        "api/Reporte/InventarioActualAPI"
                    ).Result;


                var autorizacion =
                    ValidarAutorizacion(
                        response
                    );


                if (autorizacion != null)
                    return autorizacion;


                if (!response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] =
                        "No fue posible generar el PDF de inventario.";


                    return RedirectToAction(
                        "InventarioActual",
                        "Reporte"
                    );
                }


                var datos =
                    response.Content
                        .ReadFromJsonAsync
                        <List<ReporteInventarioResponse>>()
                        .Result
                    ??
                    new List<ReporteInventarioResponse>();


                var filas =
                    datos.Select(
                        x =>
                            new[]
                            {
                                x.Codigo,
                                x.Nombre,
                                x.Categoria,
                                FormatearCantidad(
                                    x.StockActual
                                ),
                                FormatearCantidad(
                                    x.StockMinimo
                                ),
                                FormatearMoneda(
                                    x.PrecioCompra
                                ),
                                FormatearMoneda(
                                    x.PrecioVenta
                                ),
                                FormatearMoneda(
                                    x.ValorInventario
                                )
                            }
                    );


                var pdf =
                    _reportePdfService
                        .GenerarReporte(
                            ObtenerEmpresa(),
                            "Inventario actual",
                            null,
                            new[]
                            {
                                "Código",
                                "Producto",
                                "Categoría",
                                "Stock",
                                "Mínimo",
                                "Costo",
                                "Precio",
                                "Valor"
                            },
                            filas,
                            $"Valor total del inventario: {FormatearMoneda(datos.Sum(x => x.ValorInventario))}"
                        );


                return File(
                    pdf,
                    "application/pdf",
                    $"StockFlow_Inventario_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el PDF de inventario."
                );


                TempData["Mensaje"] =
                    "No fue posible generar el PDF de inventario.";


                return RedirectToAction(
                    "InventarioActual",
                    "Reporte"
                );
            }
        }


        // =========================================================
        // STOCK BAJO PDF
        // =========================================================

        [HttpGet]
        public IActionResult ExportarStockBajo()
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var response =
                    client.GetAsync(
                        "api/Reporte/StockBajoAPI"
                    ).Result;


                var autorizacion =
                    ValidarAutorizacion(
                        response
                    );


                if (autorizacion != null)
                    return autorizacion;


                if (!response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] =
                        "No fue posible generar el PDF de stock bajo.";


                    return RedirectToAction(
                        "StockBajo",
                        "Reporte"
                    );
                }


                var datos =
                    response.Content
                        .ReadFromJsonAsync
                        <List<ReporteStockBajoResponse>>()
                        .Result
                    ??
                    new List<ReporteStockBajoResponse>();


                var filas =
                    datos.Select(
                        x =>
                            new[]
                            {
                                x.Codigo,
                                x.Nombre,
                                x.Categoria,
                                FormatearCantidad(
                                    x.StockActual
                                ),
                                FormatearCantidad(
                                    x.StockMinimo
                                ),
                                FormatearCantidad(
                                    x.CantidadFaltante
                                )
                            }
                    );


                var pdf =
                    _reportePdfService
                        .GenerarReporte(
                            ObtenerEmpresa(),
                            "Productos con stock bajo",
                            null,
                            new[]
                            {
                                "Código",
                                "Producto",
                                "Categoría",
                                "Stock",
                                "Mínimo",
                                "Faltante"
                            },
                            filas,
                            $"Total por reponer: {FormatearCantidad(datos.Sum(x => x.CantidadFaltante))}"
                        );


                return File(
                    pdf,
                    "application/pdf",
                    $"StockFlow_StockBajo_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el PDF de stock bajo."
                );


                TempData["Mensaje"] =
                    "No fue posible generar el PDF de stock bajo.";


                return RedirectToAction(
                    "StockBajo",
                    "Reporte"
                );
            }
        }


        // =========================================================
        // PRODUCTOS MÁS VENDIDOS PDF
        // =========================================================

        [HttpGet]
        public IActionResult ExportarProductosMasVendidos(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            try
            {
                fechaDesde =
                    fechaDesde.Date;

                fechaHasta =
                    fechaHasta.Date;


                if (fechaDesde >
                    fechaHasta)
                {
                    TempData["Mensaje"] =
                        "La fecha inicial no puede ser mayor que la fecha final.";


                    return RedirectToAction(
                        "ProductosMasVendidos",
                        "Reporte"
                    );
                }


                var client =
                    CrearClienteAPI();


                var response =
                    client.PostAsJsonAsync(
                        "api/Reporte/ProductosMasVendidosAPI",
                        new ReporteFechasRequest
                        {
                            FechaDesde =
                                fechaDesde,

                            FechaHasta =
                                fechaHasta
                        }
                    ).Result;


                var autorizacion =
                    ValidarAutorizacion(
                        response
                    );


                if (autorizacion != null)
                    return autorizacion;


                if (!response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] =
                        "No fue posible generar el PDF de productos más vendidos.";


                    return RedirectToAction(
                        "ProductosMasVendidos",
                        "Reporte",
                        new
                        {
                            fechaDesde,
                            fechaHasta
                        }
                    );
                }


                var datos =
                    response.Content
                        .ReadFromJsonAsync
                        <List<ReporteProductoVendidoResponse>>()
                        .Result
                    ??
                    new List<ReporteProductoVendidoResponse>();


                var filas =
                    datos.Select(
                        x =>
                            new[]
                            {
                                x.Codigo,
                                x.Nombre,
                                FormatearCantidad(
                                    x.CantidadVendida
                                ),
                                FormatearMoneda(
                                    x.TotalVendido
                                ),
                                FormatearMoneda(
                                    x.GananciaBruta
                                )
                            }
                    );


                var pdf =
                    _reportePdfService
                        .GenerarReporte(
                            ObtenerEmpresa(),
                            "Productos más vendidos",
                            $"{fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}",
                            new[]
                            {
                                "Código",
                                "Producto",
                                "Cantidad",
                                "Total vendido",
                                "Ganancia bruta"
                            },
                            filas,
                            $"Total vendido: {FormatearMoneda(datos.Sum(x => x.TotalVendido))} | Ganancia bruta: {FormatearMoneda(datos.Sum(x => x.GananciaBruta))}"
                        );


                return File(
                    pdf,
                    "application/pdf",
                    $"StockFlow_ProductosVendidos_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el PDF de productos más vendidos."
                );


                TempData["Mensaje"] =
                    "No fue posible generar el PDF de productos más vendidos.";


                return RedirectToAction(
                    "ProductosMasVendidos",
                    "Reporte",
                    new
                    {
                        fechaDesde,
                        fechaHasta
                    }
                );
            }
        }
    }
}