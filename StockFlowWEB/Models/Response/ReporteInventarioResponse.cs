namespace StockFlowWEB.Models.Response
{
    public class ReporteInventarioResponse
    {
        public long IdProducto { get; set; }


        public string Codigo { get; set; }
            = string.Empty;


        public string Nombre { get; set; }
            = string.Empty;


        public string Categoria { get; set; }
            = string.Empty;


        public decimal PrecioCompra { get; set; }


        public decimal PrecioVenta { get; set; }


        public decimal StockActual { get; set; }


        public decimal StockMinimo { get; set; }


        public decimal ValorInventario { get; set; }


        public bool StockBajo { get; set; }


        public bool Estado { get; set; }
    }
}