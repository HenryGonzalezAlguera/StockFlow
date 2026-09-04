namespace StockFlowAPI.Models.Response
{
    public class VentaResponse
    {
        public long IdVenta { get; set; }

        public string NumeroVenta { get; set; }
            = string.Empty;

        public DateTime FechaVenta { get; set; }


        public long? IdCliente { get; set; }

        public string Cliente { get; set; }
            = string.Empty;


        public long IdUsuario { get; set; }

        public string Usuario { get; set; }
            = string.Empty;


        public decimal Subtotal { get; set; }

        public decimal PorcentajeImpuesto { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }


        public bool Estado { get; set; }
    }
}