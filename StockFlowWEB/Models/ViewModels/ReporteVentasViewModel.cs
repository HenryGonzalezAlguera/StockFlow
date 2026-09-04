using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class ReporteVentasViewModel
    {
        public DateTime FechaDesde { get; set; }

        public DateTime FechaHasta { get; set; }

        public List<ReporteVentaResponse> Ventas { get; set; }
            = new List<ReporteVentaResponse>();
    }
}