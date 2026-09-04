using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class CategoriaIndexViewModel
    {
        public List<CategoriaResponse> Categorias { get; set; }
            = new List<CategoriaResponse>();


        public string? Busqueda { get; set; }

        public bool? Estado { get; set; }


        public int PaginaActual { get; set; }

        public int CantidadPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }
    }
}