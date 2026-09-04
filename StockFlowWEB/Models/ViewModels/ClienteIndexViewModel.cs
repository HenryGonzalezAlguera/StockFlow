using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class ClienteIndexViewModel
    {
        public List<ClienteResponse> Clientes { get; set; }
            = new List<ClienteResponse>();


        public string? Busqueda { get; set; }

        public bool? Estado { get; set; }


        public int PaginaActual { get; set; }

        public int CantidadPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }
    }
}