using System.ComponentModel.DataAnnotations;
using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class ProductoFormViewModel
        : IValidatableObject
    {
        public long IdProducto { get; set; }


        // El código se genera automáticamente.
        // Se conserva aquí únicamente para mostrarlo
        // en Modificar y Detalle.
        public string Codigo { get; set; }
            = string.Empty;


        [Range(
            1,
            long.MaxValue,
            ErrorMessage =
                "Debe seleccionar una categoría."
        )]
        public long IdCategoria { get; set; }


        [Required(
            ErrorMessage =
                "Debe indicar el nombre."
        )]
        [MaxLength(
            150,
            ErrorMessage =
                "El nombre no puede superar los 150 caracteres."
        )]
        public string Nombre { get; set; }
            = string.Empty;


        [MaxLength(
            300,
            ErrorMessage =
                "La descripción no puede superar los 300 caracteres."
        )]
        public string? Descripcion { get; set; }


        [Range(
            0,
            double.MaxValue,
            ErrorMessage =
                "El precio de compra no puede ser negativo."
        )]
        public decimal PrecioCompra { get; set; }


        [Range(
            0,
            double.MaxValue,
            ErrorMessage =
                "El precio de venta no puede ser negativo."
        )]
        public decimal PrecioVenta { get; set; }


        [Range(
            0,
            double.MaxValue,
            ErrorMessage =
                "El stock mínimo no puede ser negativo."
        )]
        public decimal StockMinimo { get; set; }


        public List<CategoriaResponse>
            Categorias
        { get; set; }
                = new List<CategoriaResponse>();


        // =========================================================
        // VALIDACIONES ADICIONALES
        // =========================================================

        public IEnumerable<ValidationResult>
            Validate(
                ValidationContext validationContext)
        {
            if (
                StockMinimo
                !=
                decimal.Truncate(
                    StockMinimo
                )
            )
            {
                yield return
                    new ValidationResult(
                        "El stock mínimo debe ser un número entero.",
                        new[]
                        {
                            nameof(
                                StockMinimo
                            )
                        }
                    );
            }
        }
    }
}