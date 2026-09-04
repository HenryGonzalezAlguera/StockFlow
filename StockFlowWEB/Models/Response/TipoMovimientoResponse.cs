namespace StockFlowWEB.Models.Response
{
    public class TipoMovimientoResponse
    {
        public long IdTipoMovimiento { get; set; }

        public string Nombre { get; set; }
            = string.Empty;

        public string Naturaleza { get; set; }
            = string.Empty;

        public bool Estado { get; set; }
    }
}