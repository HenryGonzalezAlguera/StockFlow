using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class UsuarioIndexViewModel
    {
        public List<UsuarioResponse> Usuarios { get; set; }
            = new List<UsuarioResponse>();

        public List<RolResponse> Roles { get; set; }
            = new List<RolResponse>();

        public string? Busqueda { get; set; }

        public long? IdRol { get; set; }

        public bool? Estado { get; set; }

        public int PaginaActual { get; set; }

        public int CantidadPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }
    }
}
