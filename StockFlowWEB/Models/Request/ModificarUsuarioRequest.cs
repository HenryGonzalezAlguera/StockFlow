namespace StockFlowWEB.Models.Request
{
    public class ModificarUsuarioRequest
    {
        public long IdUsuario { get; set; }

        public long IdRol { get; set; }

        public string Nombre { get; set; }
            = string.Empty;

        public string Correo { get; set; }
            = string.Empty;
    }
}