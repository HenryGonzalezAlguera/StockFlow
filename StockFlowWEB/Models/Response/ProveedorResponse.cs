namespace StockFlowWEB.Models.Response
{
    public class ProveedorResponse
    {
        public long IdProveedor { get; set; }

        public string? Identificacion { get; set; }

        public string Nombre { get; set; }
            = string.Empty;

        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public string? Direccion { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}