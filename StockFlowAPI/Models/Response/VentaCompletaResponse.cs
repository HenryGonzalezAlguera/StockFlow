namespace StockFlowAPI.Models.Response
{
    public class VentaCompletaResponse
    {
        public VentaResponse Venta { get; set; }
            = new VentaResponse();

        public List<VentaDetalleResponse> Detalles { get; set; }
            = new List<VentaDetalleResponse>();
    }
}