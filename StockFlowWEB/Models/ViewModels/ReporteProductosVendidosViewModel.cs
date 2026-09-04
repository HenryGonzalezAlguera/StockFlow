using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class ReporteProductosVendidosViewModel
    {
        public DateTime FechaDesde { get; set; }

        public DateTime FechaHasta { get; set; }

        public List<ReporteProductoVendidoResponse> Productos { get; set; }
            = new List<ReporteProductoVendidoResponse>();
    }
}