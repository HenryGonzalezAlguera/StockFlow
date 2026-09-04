namespace StockFlowWEB.Models.Response
{
    public class ProductoMasVendidoResponse
    {
        public long IdProducto { get; set; }

        public string Codigo { get; set; }
            = string.Empty;

        public string Nombre { get; set; }
            = string.Empty;

        public decimal CantidadVendida { get; set; }

        public decimal TotalVendido { get; set; }
    }
}
