namespace StockFlowWEB.Models.Request
{
    public class ModificarCategoriaRequest
    {
        public long IdCategoria { get; set; }

        public string Nombre { get; set; }
            = string.Empty;

        public string? Descripcion { get; set; }
    }
}