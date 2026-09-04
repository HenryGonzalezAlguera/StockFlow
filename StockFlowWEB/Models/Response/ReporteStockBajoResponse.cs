namespace StockFlowWEB.Models.Response
{
    public class ReporteStockBajoResponse
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


        public decimal CantidadFaltante { get; set; }
    }
}