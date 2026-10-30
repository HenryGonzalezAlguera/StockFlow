using System.ComponentModel.DataAnnotations;
using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class AjusteInventarioViewModel
        : IValidatableObject
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
            1,
            double.MaxValue,
            ErrorMessage = "La cantidad debe ser un número entero mayor a cero."
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


        public IEnumerable<ValidationResult>
            Validate(
                ValidationContext validationContext)
        {
            if (
                Cantidad
                !=
                decimal.Truncate(
                    Cantidad
                )
            )
            {
                yield return
                    new ValidationResult(
                        "La cantidad debe ser un número entero.",
                        new[]
                        {
                            nameof(Cantidad)
                        }
                    );
            }
        }
    }
}
