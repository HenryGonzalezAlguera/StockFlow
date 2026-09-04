namespace StockFlowWEB.Models.Request
{
    public class RegistrarEmpresaRequest
    {
        public string IdentificacionEmpresa { get; set; }
            = string.Empty;


        public string NombreEmpresa { get; set; }
            = string.Empty;


        public string? CorreoEmpresa { get; set; }


        public string? TelefonoEmpresa { get; set; }


        public string? DireccionEmpresa { get; set; }


        public string NombreAdministrador { get; set; }
            = string.Empty;


        public string CorreoAdministrador { get; set; }
            = string.Empty;


        public string Contrasena { get; set; }
            = string.Empty;
    }
}