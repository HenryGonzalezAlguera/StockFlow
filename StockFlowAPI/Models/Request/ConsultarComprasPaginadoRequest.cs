namespace StockFlowAPI.Models.Request
{
    public class ConsultarComprasPaginadoRequest
    {
        public string? Busqueda { get; set; }

        public long? IdProveedor { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        public int Pagina { get; set; } = 1;

        public int CantidadPagina { get; set; } = 10;
    }
}