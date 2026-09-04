namespace StockFlowWEB.Models.Response
{
    public class UsuarioResponse
    {
        public long IdUsuario { get; set; }

        public long IdRol { get; set; }

        public string Rol { get; set; }
            = string.Empty;

        public string Nombre { get; set; }
            = string.Empty;

        public string Correo { get; set; }
            = string.Empty;

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
