namespace StockFlowWEB.Models.Response
{
    public class ReporteVentaResponse
    {
        public long IdVenta { get; set; }


        public string NumeroVenta { get; set; }
            = string.Empty;


        public DateTime FechaVenta { get; set; }


        public string Cliente { get; set; }
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