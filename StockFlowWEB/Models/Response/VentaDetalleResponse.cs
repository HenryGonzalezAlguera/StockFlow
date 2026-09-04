namespace StockFlowWEB.Models.Response
{
    public class VentaDetalleResponse
    {
        public long IdDetalleVenta { get; set; }

        public long IdProducto { get; set; }

        public string CodigoProducto { get; set; }
            = string.Empty;

        public string Producto { get; set; }
            = string.Empty;

        public decimal Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}