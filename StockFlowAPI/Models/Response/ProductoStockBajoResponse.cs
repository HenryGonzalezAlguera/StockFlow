namespace StockFlowAPI.Models.Response
{
    public class ProductoStockBajoResponse
    {
        public long IdProducto { get; set; }

        public string Codigo { get; set; }
            = string.Empty;

        public string Nombre { get; set; }
            = string.Empty;

        public string Categoria { get; set; }
            = string.Empty;

        public decimal StockActual { get; set; }

        public decimal StockMinimo { get; set; }
    }
}