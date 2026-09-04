namespace StockFlowWEB.Models.Request
{
    public class RegistrarClienteRequest
    {
        public string? Identificacion { get; set; }

        public string Nombre { get; set; }
            = string.Empty;

        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public string? Direccion { get; set; }
    }
}