namespace StockFlowAPI.Models.Response
{
    public class ProductoResponse
    {
        public long IdProducto { get; set; }

        public long IdCategoria { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }

        public decimal StockActual { get; set; }

        public decimal StockMinimo { get; set; }

        public bool StockBajo { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}