namespace StockFlowAPI.Models.Request
{
    public class ConsultarProductosPaginadoRequest
    {
        public string? Busqueda { get; set; }

        public long? IdCategoria { get; set; }

        public bool? Estado { get; set; }

        public int Pagina { get; set; } = 1;

        public int CantidadPagina { get; set; } = 10;
    }
}