namespace StockFlowWEB.Models.Request
{
    public class RegistrarProveedorRequest
    {
        public string? Identificacion { get; set; }

        public string Nombre { get; set; }
            = string.Empty;

        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public string? Direccion { get; set; }
    }
}