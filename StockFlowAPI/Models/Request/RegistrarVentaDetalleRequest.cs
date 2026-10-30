using System.ComponentModel.DataAnnotations;

namespace StockFlowAPI.Models.Request
{
    public class RegistrarVentaDetalleRequest
        : IValidatableObject
    {
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "El producto no es válido."
        )]
        public long IdProducto { get; set; }


        [Range(
            1,
            double.MaxValue,
            ErrorMessage = "La cantidad debe ser un número entero mayor a cero."
        )]
        public decimal Cantidad { get; set; }


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
