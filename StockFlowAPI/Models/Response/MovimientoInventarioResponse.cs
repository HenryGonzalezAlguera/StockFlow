namespace StockFlowAPI.Models.Response
{
    public class MovimientoInventarioResponse
    {
        public long IdMovimiento { get; set; }


        public long IdProducto { get; set; }

        public string CodigoProducto { get; set; }
            = string.Empty;

        public string Producto { get; set; }
            = string.Empty;


        public long IdTipoMovimiento { get; set; }

        public string TipoMovimiento { get; set; }
            = string.Empty;

        public string Naturaleza { get; set; }
            = string.Empty;


        public long IdUsuario { get; set; }

        public string Usuario { get; set; }
            = string.Empty;


        public long? IdCompra { get; set; }

        public string? NumeroCompra { get; set; }


        public long? IdVenta { get; set; }

        public string? NumeroVenta { get; set; }


        public decimal Cantidad { get; set; }

        public decimal StockAnterior { get; set; }

        public decimal StockNuevo { get; set; }


        public string? Motivo { get; set; }

        public DateTime FechaMovimiento { get; set; }
    }
}