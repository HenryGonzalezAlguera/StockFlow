using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class ProductoIndexViewModel
    {
        public List<ProductoResponse> Productos { get; set; }
            = new List<ProductoResponse>();

        public List<CategoriaResponse> Categorias { get; set; }
            = new List<CategoriaResponse>();


        public string? Busqueda { get; set; }

        public long? IdCategoria { get; set; }

        public bool? Estado { get; set; }


        public int PaginaActual { get; set; }

        public int CantidadPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }
    }
}