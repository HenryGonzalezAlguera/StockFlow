namespace StockFlowWEB.Models.Response
{
    public class DashboardResponse
    {
        public DashboardResumenResponse Resumen { get; set; }
            = new DashboardResumenResponse();


        public List<ProductoStockBajoResponse> ProductosStockBajo
        {
            get;
            set;
        }
            = new List<ProductoStockBajoResponse>();


        public List<VentaMesResponse> VentasUltimosMeses
        {
            get;
            set;
        }
            = new List<VentaMesResponse>();


        public List<ProductoMasVendidoResponse> ProductosMasVendidos
        {
            get;
            set;
        }
            = new List<ProductoMasVendidoResponse>();
    }
}