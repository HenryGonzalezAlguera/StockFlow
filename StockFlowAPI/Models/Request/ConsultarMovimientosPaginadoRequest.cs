namespace StockFlowAPI.Models.Request
{
    public class ConsultarMovimientosPaginadoRequest
    {
        public string? Busqueda { get; set; }

        public long? IdProducto { get; set; }

        public long? IdTipoMovimiento { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        public int Pagina { get; set; } = 1;

        public int CantidadPagina { get; set; } = 10;
    }
}