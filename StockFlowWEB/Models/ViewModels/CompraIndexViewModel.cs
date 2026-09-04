using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class CompraIndexViewModel
    {
        public List<CompraResponse> Compras { get; set; }
            = new List<CompraResponse>();

        public List<ProveedorResponse> Proveedores { get; set; }
            = new List<ProveedorResponse>();


        public string? Busqueda { get; set; }

        public long? IdProveedor { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }


        public int PaginaActual { get; set; }

        public int CantidadPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }
    }
}