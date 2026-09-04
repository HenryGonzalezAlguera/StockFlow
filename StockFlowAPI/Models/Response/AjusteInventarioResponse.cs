namespace StockFlowAPI.Models.Response
{
    public class AjusteInventarioResponse
    {
        public long IdMovimiento { get; set; }

        public long IdProducto { get; set; }

        public string TipoAjuste { get; set; }
            = string.Empty;

        public decimal Cantidad { get; set; }

        public decimal StockAnterior { get; set; }

        public decimal StockNuevo { get; set; }

        public string Motivo { get; set; }
            = string.Empty;

        public string Mensaje { get; set; }
            = string.Empty;
    }
}