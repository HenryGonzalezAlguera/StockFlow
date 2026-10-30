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
    public class ProductoController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public ProductoController(
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
        // CONSULTAR CATEGORÍAS ACTIVAS
        // =========================================================

        private List<CategoriaResponse>
            ConsultarCategorias(
                HttpClient client)
        {
            var response =
                client.GetAsync(
                    "api/Categoria/ConsultarCategoriasAPI"
                ).Result;


            if (!response.IsSuccessStatusCode)
            {
                return new List<CategoriaResponse>();
            }


            var categorias =
                response.Content
                    .ReadFromJsonAsync
                    <List<CategoriaResponse>>()
                    .Result;


            return categorias?
                .Where(
                    x => x.Estado
                )
                .OrderBy(
                    x => x.Nombre
                )
                .ToList()
                ??
                new List<CategoriaResponse>();
        }


        // =========================================================
        // LISTADO
        // =========================================================

        [HttpGet]
        public IActionResult Index(
            string? busqueda,
            long? idCategoria,
            bool? estado,
            int pagina = 1,
            int cantidadPagina = 10)
        {
            try
            {
                var client =
                    CrearClienteAPI();


                // =============================================
                // CATEGORÍAS
                // =============================================

                var categorias =
                    ConsultarCategorias(
                        client
                    );


                // =============================================
                // REQUEST
                // =============================================

                var request =
                    new ConsultarProductosPaginadoRequest
                    {
                        Busqueda =
                            busqueda,

                        IdCategoria =
                            idCategoria,

                        Estado =
                            estado,

                        Pagina =
                            pagina,

                        CantidadPagina =
                            cantidadPagina
                    };


                // =============================================
                // API
                // =============================================

                var response =
                    client.PostAsJsonAsync(
                        "api/Producto/ConsultarProductosPaginadoAPI",
                        request
                    ).Result;


                // =============================================
                // 401
                // =============================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.Unauthorized
                )
                {
                    HttpContext.Session.Clear();


                    return RedirectToAction(
                        "Index",
                        "Home"
                    );
                }


                // =============================================
                // ERROR
                // =============================================

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Mensaje =
                        "No fue posible consultar los productos.";


                    return View(
                        new ProductoIndexViewModel
                        {
                            Categorias =
                                categorias
                        }
                    );
                }


                // =============================================
                // RESULTADO
                // =============================================

                var resultado =
                    response.Content
                        .ReadFromJsonAsync
                        <PaginacionResponse<ProductoResponse>>()
                        .Result;


                resultado ??=
                    new PaginacionResponse
                    <ProductoResponse>();


                var viewModel =
                    new ProductoIndexViewModel
                    {
                        Productos =
                            resultado.Registros,

                        Categorias =
                            categorias,

                        Busqueda =
                            busqueda,

                        IdCategoria =
                            idCategoria,

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
                    viewModel
                );
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                return View(
                    new ProductoIndexViewModel()
                );
            }
        }


        // =========================================================
        // REGISTRAR - GET
        // =========================================================

        [SessionAuthorize("Administrador", "Empleado")]
        [HttpGet]
        public IActionResult Registrar()
        {
            try
            {
                var client =
                    CrearClienteAPI();


                var viewModel =
                    new ProductoFormViewModel
                    {
                        Categorias =
                            ConsultarCategorias(
                                client
                            )
                    };


                return View(
                    viewModel
                );
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar el formulario.";


                return RedirectToAction(
                    "Index"
                );
            }
        }


        // =========================================================
        // REGISTRAR - POST
        //
        // EL CÓDIGO YA NO SE ENVÍA.
        // SQL SERVER LO GENERA AUTOMÁTICAMENTE.
        // =========================================================

        [SessionAuthorize("Administrador", "Empleado")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(
            ProductoFormViewModel model)
        {
            var client =
                CrearClienteAPI();


            // =====================================================
            // VALIDACIONES DEL VIEWMODEL
            // =====================================================

            if (!ModelState.IsValid)
            {
                model.Categorias =
                    ConsultarCategorias(
                        client
                    );


                return View(
                    model
                );
            }


            try
            {
                // =================================================
                // REQUEST
                //
                // IMPORTANTE:
                // NO SE ENVÍA CODIGO.
                // =================================================

                var request =
                    new RegistrarProductoRequest
                    {
                        IdCategoria =
                            model.IdCategoria,

                        Nombre =
                            model.Nombre.Trim(),

                        Descripcion =
                            model.Descripcion,

                        PrecioCompra =
                            model.PrecioCompra,

                        PrecioVenta =
                            model.PrecioVenta,

                        StockMinimo =
                            model.StockMinimo
                    };


                // =================================================
                // API
                // =================================================

                var response =
                    client.PostAsJsonAsync(
                        "api/Producto/RegistrarProductoAPI",
                        request
                    ).Result;


                // =================================================
                // 401
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.Unauthorized
                )
                {
                    HttpContext.Session.Clear();


                    return RedirectToAction(
                        "Index",
                        "Home"
                    );
                }


                // =================================================
                // 403
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.Forbidden
                )
                {
                    TempData["MensajeError"] =
                        "No tiene permisos para registrar productos.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // 201 CREATED
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.Created
                )
                {
                    TempData["MensajeExito"] =
                        "Producto registrado correctamente.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // ERROR CONTROLADO
                // =================================================

                var error =
                    response.Content
                        .ReadFromJsonAsync
                        <MensajeResponse>()
                        .Result;


                ViewBag.Mensaje =
                    error?.Mensaje
                    ??
                    "No fue posible registrar el producto.";


                model.Categorias =
                    ConsultarCategorias(
                        client
                    );


                return View(
                    model
                );
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                model.Categorias =
                    ConsultarCategorias(
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

        [SessionAuthorize("Administrador", "Empleado")]
        [HttpGet]
        public IActionResult Modificar(
            long id)
        {
            try
            {
                var client =
                    CrearClienteAPI();


                // =================================================
                // API
                // =================================================

                var response =
                    client.GetAsync(
                        $"api/Producto/ConsultarProductoAPI/{id}"
                    ).Result;


                // =================================================
                // 401
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.Unauthorized
                )
                {
                    HttpContext.Session.Clear();


                    return RedirectToAction(
                        "Index",
                        "Home"
                    );
                }


                // =================================================
                // 404
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.NotFound
                )
                {
                    TempData["MensajeError"] =
                        "El producto no existe.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // ERROR
                // =================================================

                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar el producto.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // PRODUCTO
                // =================================================

                var producto =
                    response.Content
                        .ReadFromJsonAsync
                        <ProductoResponse>()
                        .Result;


                if (producto == null)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar el producto.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // VIEWMODEL
                //
                // AQUÍ SÍ CONSERVAMOS CODIGO PORQUE SOLO
                // LO MOSTRAMOS COMO READONLY.
                // =================================================

                var viewModel =
                    new ProductoFormViewModel
                    {
                        IdProducto =
                            producto.IdProducto,

                        IdCategoria =
                            producto.IdCategoria,

                        Codigo =
                            producto.Codigo,

                        Nombre =
                            producto.Nombre,

                        Descripcion =
                            producto.Descripcion,

                        PrecioCompra =
                            producto.PrecioCompra,

                        PrecioVenta =
                            producto.PrecioVenta,

                        StockMinimo =
                            producto.StockMinimo,

                        Categorias =
                            ConsultarCategorias(
                                client
                            )
                    };


                return View(
                    viewModel
                );
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar el producto.";


                return RedirectToAction(
                    "Index"
                );
            }
        }


        // =========================================================
        // MODIFICAR - POST
        //
        // EL CÓDIGO NO SE ENVÍA.
        // NO SE PUEDE CAMBIAR.
        // =========================================================

        [SessionAuthorize("Administrador", "Empleado")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Modificar(
            ProductoFormViewModel model)
        {
            var client =
                CrearClienteAPI();


            // =====================================================
            // VALIDACIÓN
            // =====================================================

            if (!ModelState.IsValid)
            {
                model.Categorias =
                    ConsultarCategorias(
                        client
                    );


                return View(
                    model
                );
            }


            try
            {
                // =================================================
                // REQUEST
                //
                // IMPORTANTE:
                // NO SE ENVÍA CODIGO.
                // =================================================

                var request =
                    new ModificarProductoRequest
                    {
                        IdProducto =
                            model.IdProducto,

                        IdCategoria =
                            model.IdCategoria,

                        Nombre =
                            model.Nombre.Trim(),

                        Descripcion =
                            model.Descripcion,

                        PrecioCompra =
                            model.PrecioCompra,

                        PrecioVenta =
                            model.PrecioVenta,

                        StockMinimo =
                            model.StockMinimo
                    };


                // =================================================
                // API
                // =================================================

                var response =
                    client.PutAsJsonAsync(
                        "api/Producto/ModificarProductoAPI",
                        request
                    ).Result;


                // =================================================
                // 401
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.Unauthorized
                )
                {
                    HttpContext.Session.Clear();


                    return RedirectToAction(
                        "Index",
                        "Home"
                    );
                }


                // =================================================
                // 403
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.Forbidden
                )
                {
                    TempData["MensajeError"] =
                        "No tiene permisos para modificar productos.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // 404
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.NotFound
                )
                {
                    TempData["MensajeError"] =
                        "El producto no existe.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // 200 OK
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.OK
                )
                {
                    TempData["MensajeExito"] =
                        "Producto modificado correctamente.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // ERROR CONTROLADO
                // =================================================

                var error =
                    response.Content
                        .ReadFromJsonAsync
                        <MensajeResponse>()
                        .Result;


                ViewBag.Mensaje =
                    error?.Mensaje
                    ??
                    "No fue posible modificar el producto.";


                model.Categorias =
                    ConsultarCategorias(
                        client
                    );


                return View(
                    model
                );
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";


                model.Categorias =
                    ConsultarCategorias(
                        client
                    );


                return View(
                    model
                );
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
                        $"api/Producto/ConsultarProductoAPI/{id}"
                    ).Result;


                // =================================================
                // 401
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.Unauthorized
                )
                {
                    HttpContext.Session.Clear();


                    return RedirectToAction(
                        "Index",
                        "Home"
                    );
                }


                // =================================================
                // 404
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.NotFound
                )
                {
                    TempData["MensajeError"] =
                        "El producto no existe.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // ERROR
                // =================================================

                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar el producto.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // PRODUCTO
                // =================================================

                var producto =
                    response.Content
                        .ReadFromJsonAsync
                        <ProductoResponse>()
                        .Result;


                if (producto == null)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar el producto.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                return View(
                    producto
                );
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar el producto.";


                return RedirectToAction(
                    "Index"
                );
            }
        }


        // =========================================================
        // CAMBIAR ESTADO
        // SOLO ADMINISTRADOR EN API
        // =========================================================

        [SessionAuthorize("Administrador")]
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
                        $"api/Producto/CambiarEstadoProductoAPI/{id}",
                        null
                    ).Result;


                // =================================================
                // 401
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.Unauthorized
                )
                {
                    HttpContext.Session.Clear();


                    return RedirectToAction(
                        "Index",
                        "Home"
                    );
                }


                // =================================================
                // 403
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.Forbidden
                )
                {
                    TempData["MensajeError"] =
                        "No tiene permisos para cambiar el estado del producto.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // 404
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.NotFound
                )
                {
                    TempData["MensajeError"] =
                        "El producto no existe.";


                    return RedirectToAction(
                        "Index"
                    );
                }


                // =================================================
                // 200 OK
                // =================================================

                if (
                    response.StatusCode
                    ==
                    HttpStatusCode.OK
                )
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


                // =================================================
                // ERROR
                // =================================================

                var error =
                    response.Content
                        .ReadFromJsonAsync
                        <MensajeResponse>()
                        .Result;


                TempData["MensajeError"] =
                    error?.Mensaje
                    ??
                    "No fue posible actualizar el producto.";


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
