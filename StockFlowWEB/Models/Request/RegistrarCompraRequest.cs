namespace StockFlowWEB.Models.Request
{
    public class RegistrarCompraRequest
    {
        public long IdProveedor { get; set; }

        public decimal PorcentajeImpuesto { get; set; }

        public List<RegistrarCompraDetalleRequest> Detalles { get; set; }
            = new List<RegistrarCompraDetalleRequest>();
    }
}