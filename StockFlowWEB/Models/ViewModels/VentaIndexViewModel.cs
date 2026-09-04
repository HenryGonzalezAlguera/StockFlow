using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class VentaIndexViewModel
    {
        public List<VentaResponse> Ventas { get; set; }
            = new List<VentaResponse>();

        public List<ClienteResponse> Clientes { get; set; }
            = new List<ClienteResponse>();


        public string? Busqueda { get; set; }

        public long? IdCliente { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }


        public int PaginaActual { get; set; }

        public int CantidadPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }
    }
}