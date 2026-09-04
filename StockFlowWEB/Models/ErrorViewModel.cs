namespace StockFlowWEB.Models
{
    public class ErrorViewModel
    {
        public int StatusCode { get; set; }


        public string Titulo { get; set; }
            = "Ocurrió un problema";


        public string Mensaje { get; set; }
            = "No fue posible completar la solicitud.";


        public string? RequestId { get; set; }


        public bool ShowRequestId =>
            !string.IsNullOrWhiteSpace(
                RequestId
            );
    }
}