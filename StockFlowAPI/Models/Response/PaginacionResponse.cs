namespace StockFlowAPI.Models.Response
{
    public class PaginacionResponse<T>
    {
        public List<T> Registros { get; set; }
            = new List<T>();

        public int PaginaActual { get; set; }

        public int CantidadPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }
    }
}