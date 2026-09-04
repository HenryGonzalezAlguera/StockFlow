namespace StockFlowWEB.Models.Response
{
    public class VentaMesResponse
    {
        public DateTime FechaMes { get; set; }

        public int Anio { get; set; }

        public int Mes { get; set; }

        public decimal TotalVentas { get; set; }
    }
}