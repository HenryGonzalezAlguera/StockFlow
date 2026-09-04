namespace StockFlowAPI.Models.Response
{
    public class CompraResponse
    {
        public long IdCompra { get; set; }

        public string NumeroCompra { get; set; } = string.Empty;

        public DateTime FechaCompra { get; set; }


        public long IdProveedor { get; set; }

        public string Proveedor { get; set; } = string.Empty;


        public long IdUsuario { get; set; }

        public string Usuario { get; set; } = string.Empty;


        public decimal Subtotal { get; set; }

        public decimal PorcentajeImpuesto { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }


        public bool Estado { get; set; }
    }
}