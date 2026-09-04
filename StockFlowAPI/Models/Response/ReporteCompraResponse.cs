namespace StockFlowAPI.Models.Response
{
    public class ReporteCompraResponse
    {
        public long IdCompra { get; set; }

        public string NumeroCompra { get; set; }
            = string.Empty;

        public DateTime FechaCompra { get; set; }

        public string Proveedor { get; set; }
            = string.Empty;

        public string Usuario { get; set; }
            = string.Empty;

        public decimal Subtotal { get; set; }

        public decimal PorcentajeImpuesto { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }

        public bool Estado { get; set; }
    }
}