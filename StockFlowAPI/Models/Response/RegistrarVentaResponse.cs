namespace StockFlowAPI.Models.Response
{
    public class RegistrarVentaResponse
    {
        public long IdVenta { get; set; }

        public string NumeroVenta { get; set; }
            = string.Empty;

        public decimal Subtotal { get; set; }

        public decimal PorcentajeImpuesto { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }

        public string Mensaje { get; set; }
            = string.Empty;
    }
}