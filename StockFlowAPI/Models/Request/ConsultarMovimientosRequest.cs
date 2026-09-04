namespace StockFlowAPI.Models.Request
{
    public class ConsultarMovimientosRequest
    {
        public long? IdProducto { get; set; }

        public long? IdTipoMovimiento { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }
    }
}