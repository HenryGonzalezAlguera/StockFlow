namespace StockFlowAPI.Models.Response
{
    public class LoginResponse
    {
        public long IdUsuario { get; set; }
        public long IdEmpresa { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreEmpresa { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
    }
}