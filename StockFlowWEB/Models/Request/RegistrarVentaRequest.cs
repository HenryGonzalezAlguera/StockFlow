namespace StockFlowWEB.Models.Request
{
    public class RegistrarVentaRequest
    {
        public long? IdCliente { get; set; }

        public decimal PorcentajeImpuesto { get; set; }

        public List<RegistrarVentaDetalleRequest> Detalles { get; set; }
            = new List<RegistrarVentaDetalleRequest>();
    }
}