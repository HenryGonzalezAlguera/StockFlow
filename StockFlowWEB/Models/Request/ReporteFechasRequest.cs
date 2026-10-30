using System.ComponentModel.DataAnnotations;

namespace StockFlowWEB.Models.Request
{
    public class ReporteFechasRequest
    {
        [Required(
            ErrorMessage =
                "Debe indicar la fecha inicial."
        )]
        public DateTime? FechaDesde { get; set; }


        [Required(
            ErrorMessage =
                "Debe indicar la fecha final."
        )]
        public DateTime? FechaHasta { get; set; }
    }
}
