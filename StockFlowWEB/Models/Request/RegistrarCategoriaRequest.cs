namespace StockFlowWEB.Models.Request
{
    public class RegistrarCategoriaRequest
    {
        public string Nombre { get; set; }
            = string.Empty;

        public string? Descripcion { get; set; }
    }
}