using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class ReporteComprasViewModel
    {
        public DateTime FechaDesde { get; set; }

        public DateTime FechaHasta { get; set; }

        public List<ReporteCompraResponse> Compras { get; set; }
            = new List<ReporteCompraResponse>();
    }
}