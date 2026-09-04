using Microsoft.AspNetCore.Mvc;
using StockFlowWEB.Attributes;
using StockFlowWEB.Models.Response;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StockFlowWEB.Controllers
{
    [SessionAuthorize]
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public DashboardController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                // =================================================
                // TOKEN
                // =================================================

                var token =
                    HttpContext.Session
                        .GetString("Token");


                if (string.IsNullOrWhiteSpace(token))
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction(
                        "Index",
                        "Home"
                    );
                }


                // =================================================
                // CLIENTE HTTP
                // =================================================

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


                // =================================================
                // LLAMAR DASHBOARD API
                // =================================================

                var response =
                    client.GetAsync(
                        "api/Dashboard/ConsultarDashboardAPI"
                    ).Result;


                // =================================================
                // TOKEN VENCIDO / INVALIDO
                // =================================================

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction(
                        "Index",
                        "Home"
                    );
                }


                // =================================================
                // ERROR API
                // =================================================

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Mensaje =
                        "No fue posible cargar la información del dashboard.";

                    return View(
                        new DashboardResponse()
                    );
                }


                // =================================================
                // LEER RESPONSE
                // =================================================

                var dashboard =
                    response.Content
                        .ReadFromJsonAsync
                        <DashboardResponse>()
                        .Result;


                if (dashboard == null)
                {
                    dashboard =
                        new DashboardResponse();
                }


                // =================================================
                // INFORMACION SESSION
                // =================================================

                ViewBag.Nombre =
                    HttpContext.Session
                        .GetString("Nombre");


                ViewBag.Empresa =
                    HttpContext.Session
                        .GetString("NombreEmpresa");


                ViewBag.Rol =
                    HttpContext.Session
                        .GetString("Rol");


                return View(dashboard);
            }
            catch (Exception)
            {
                ViewBag.Mensaje =
                    "No fue posible comunicarse con StockFlowAPI.";

                return View(
                    new DashboardResponse()
                );
            }
        }
    }
}