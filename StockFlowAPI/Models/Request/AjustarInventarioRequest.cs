using System.ComponentModel.DataAnnotations;

namespace StockFlowAPI.Models.Request
{
    public class AjustarInventarioRequest
        : IValidatableObject
    {
        [Range(
            1,
            long.MaxValue,
            ErrorMessage =
                "Debe seleccionar un producto."
        )]
        public long IdProducto { get; set; }


        [Required(
            ErrorMessage =
                "Debe indicar el tipo de ajuste."
        )]
        [MaxLength(
            10,
            ErrorMessage =
                "El tipo de ajuste no es válido."
        )]
        public string TipoAjuste { get; set; }
            = string.Empty;


        [Range(
            1,
            double.MaxValue,
            ErrorMessage =
                "La cantidad debe ser un número entero mayor a cero."
        )]
        public decimal Cantidad { get; set; }


        [Required(
            ErrorMessage =
                "Debe indicar el motivo del ajuste."
        )]
        [MaxLength(
            300,
            ErrorMessage =
                "El motivo no puede superar los 300 caracteres."
        )]
        public string Motivo { get; set; }
            = string.Empty;


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
