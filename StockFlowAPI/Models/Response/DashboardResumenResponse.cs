namespace StockFlowAPI.Models.Response
{
    public class DashboardResumenResponse
    {
        public decimal VentasMes { get; set; }

        public decimal ComprasMes { get; set; }

        public decimal GananciaBrutaMes { get; set; }

        public int ProductosActivos { get; set; }

        public int ProductosStockBajo { get; set; }

        public int ClientesActivos { get; set; }

        public int ProveedoresActivos { get; set; }
    }
}