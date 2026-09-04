namespace StockFlowWEB.Models.Request
{
    public class RegistrarUsuarioRequest
    {
        public long IdRol { get; set; }

        public string Nombre { get; set; }
            = string.Empty;

        public string Correo { get; set; }
            = string.Empty;

        public string Contrasena { get; set; }
            = string.Empty;
    }
}
