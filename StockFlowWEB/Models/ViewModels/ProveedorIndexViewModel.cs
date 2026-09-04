using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class ProveedorIndexViewModel
    {
        public List<ProveedorResponse> Proveedores { get; set; }
            = new List<ProveedorResponse>();


        public string? Busqueda { get; set; }

        public bool? Estado { get; set; }


        public int PaginaActual { get; set; }

        public int CantidadPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }
    }
}