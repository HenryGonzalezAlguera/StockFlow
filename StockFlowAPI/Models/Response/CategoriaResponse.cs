namespace StockFlowAPI.Models.Response
{
    public class CategoriaResponse
    {
        public long IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}