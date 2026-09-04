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
    public class CategoriaController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public CategoriaController(
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
                    new ConsultarCategoriasPaginadoRequest
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
                        "api/Categoria/ConsultarCategoriasPaginadoAPI",
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
                        "No fue posible consultar las categorías.";

                    return View(
                        new CategoriaIndexViewModel()
                    );
                }


                var resultado =
                    response.Content
                        .ReadFromJsonAsync
                        <PaginacionResponse<CategoriaResponse>>()
                        .Result;


                resultado ??=
                    new PaginacionResponse<CategoriaResponse>();


                var model =
                    new CategoriaIndexViewModel
                    {
                        Categorias =
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
                    new CategoriaIndexViewModel()
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
                new CategoriaFormViewModel()
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
            CategoriaFormViewModel model)
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
                    new RegistrarCategoriaRequest
                    {
                        Nombre =
                            model.Nombre.Trim(),

                        Descripcion =
                            model.Descripcion?.Trim()
                    };


                var response =
                    client.PostAsJsonAsync(
                        "api/Categoria/RegistrarCategoriaAPI",
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
                        "No tiene permisos para registrar categorías.";

                    return View(model);
                }


                if (response.StatusCode ==
                    HttpStatusCode.Created)
                {
                    TempData["MensajeExito"] =
                        "Categoría registrada correctamente.";

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
                    "No fue posible registrar la categoría.";


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
                        $"api/Categoria/ConsultarCategoriaAPI/{id}"
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
                        "La categoría no existe.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] =
                        "No fue posible consultar la categoría.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                var categoria =
                    response.Content
                        .ReadFromJsonAsync
                        <CategoriaResponse>()
                        .Result;


                if (categoria == null)
                {
                    return RedirectToAction(
                        "Index"
                    );
                }


                var model =
                    new CategoriaFormViewModel
                    {
                        IdCategoria =
                            categoria.IdCategoria,

                        Nombre =
                            categoria.Nombre,

                        Descripcion =
                            categoria.Descripcion
                    };


                return View(model);
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar la categoría.";

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
            CategoriaFormViewModel model)
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
                    new ModificarCategoriaRequest
                    {
                        IdCategoria =
                            model.IdCategoria,

                        Nombre =
                            model.Nombre.Trim(),

                        Descripcion =
                            model.Descripcion?.Trim()
                    };


                var response =
                    client.PutAsJsonAsync(
                        "api/Categoria/ModificarCategoriaAPI",
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
                        "Categoría modificada correctamente.";

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
                    "No fue posible modificar la categoría.";


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
                        $"api/Categoria/CambiarEstadoCategoriaAPI/{id}",
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
                        "No tiene permisos para cambiar el estado de la categoría.";

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
                    "No fue posible actualizar la categoría.";


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
