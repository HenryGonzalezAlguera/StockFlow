namespace StockFlowWEB.Models.Response
{
    public class RegistrarCompraResponse
    {
        public long IdCompra { get; set; }

        public string NumeroCompra { get; set; }
            = string.Empty;

        public decimal Subtotal { get; set; }

        public decimal PorcentajeImpuesto { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }

        public string Mensaje { get; set; }
            = string.Empty;
    }
}