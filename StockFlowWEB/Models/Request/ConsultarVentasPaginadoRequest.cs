namespace StockFlowWEB.Models.Request
{
    public class ConsultarVentasPaginadoRequest
    {
        public string? Busqueda { get; set; }

        public long? IdCliente { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        public int Pagina { get; set; } = 1;

        public int CantidadPagina { get; set; } = 10;
    }
}