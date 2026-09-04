namespace StockFlowWEB.Models.Request
{
    public class ModificarProductoRequest
    {
        public long IdProducto { get; set; }

        public long IdCategoria { get; set; }

        public string Codigo { get; set; }
            = string.Empty;

        public string Nombre { get; set; }
            = string.Empty;

        public string? Descripcion { get; set; }

        public decimal PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }

        public decimal StockMinimo { get; set; }
    }
}