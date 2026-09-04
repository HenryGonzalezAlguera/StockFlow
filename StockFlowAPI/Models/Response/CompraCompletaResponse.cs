namespace StockFlowAPI.Models.Response
{
    public class CompraCompletaResponse
    {
        public CompraResponse Compra { get; set; }
            = new CompraResponse();

        public List<CompraDetalleResponse> Detalles { get; set; }
            = new List<CompraDetalleResponse>();
    }
}