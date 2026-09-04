namespace StockFlowAPI.Models
{
    public class UsuarioLoginModel
    {
        public long IdUsuario { get; set; }
        public long IdEmpresa { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreEmpresa { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;

        public string Contrasena { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;
    }
}