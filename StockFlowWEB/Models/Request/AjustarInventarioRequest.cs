namespace StockFlowWEB.Models.Request
{
    public class AjustarInventarioRequest
    {
        public long IdProducto { get; set; }

        public string TipoAjuste { get; set; }
            = string.Empty;

        public decimal Cantidad { get; set; }

        public string Motivo { get; set; }
            = string.Empty;
    }
}