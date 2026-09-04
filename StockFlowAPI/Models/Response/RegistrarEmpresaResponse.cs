namespace StockFlowAPI.Models.Response
{
    public class RegistrarEmpresaResponse
    {
        public long IdEmpresa { get; set; }
        public long IdUsuario { get; set; }

        public string NombreEmpresa { get; set; } = string.Empty;
        public string NombreAdministrador { get; set; } = string.Empty;
        public string CorreoAdministrador { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;
    }
}