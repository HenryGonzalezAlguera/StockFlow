using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class InventarioIndexViewModel
    {
        public List<MovimientoInventarioResponse> Movimientos { get; set; }
            = new List<MovimientoInventarioResponse>();

        public List<ProductoResponse> Productos { get; set; }
            = new List<ProductoResponse>();

        public List<TipoMovimientoResponse> TiposMovimiento { get; set; }
            = new List<TipoMovimientoResponse>();


        public string? Busqueda { get; set; }

        public long? IdProducto { get; set; }

        public long? IdTipoMovimiento { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }


        public int PaginaActual { get; set; }

        public int CantidadPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }
    }
}