namespace StockFlowWEB.Models.Response
{
    public class MiCuentaResponse
    {
        public long IdUsuario { get; set; }

        public string Nombre { get; set; }
            = string.Empty;

        public string Correo { get; set; }
            = string.Empty;

        public string Rol { get; set; }
            = string.Empty;

        public string NombreEmpresa { get; set; }
            = string.Empty;

        public string IdentificacionEmpresa { get; set; }
            = string.Empty;

        public DateTime FechaRegistro { get; set; }
    }
}
