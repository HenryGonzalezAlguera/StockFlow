using System.ComponentModel.DataAnnotations;
using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class AjusteInventarioViewModel
    {
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un producto."
        )]
        public long IdProducto { get; set; }


        [Required(
            ErrorMessage = "Debe seleccionar el tipo de ajuste."
        )]
        public string TipoAjuste { get; set; }
            = string.Empty;


        [Range(
            0.001,
            double.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a cero."
        )]
        public decimal Cantidad { get; set; }


        [Required(
            ErrorMessage = "Debe indicar el motivo del ajuste."
        )]
        [MaxLength(300)]
        public string Motivo { get; set; }
            = string.Empty;


        public List<ProductoResponse> Productos { get; set; }
            = new List<ProductoResponse>();
    }
}