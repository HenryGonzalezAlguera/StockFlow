using System.ComponentModel.DataAnnotations;
using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class ProductoFormViewModel
    {
        public long IdProducto { get; set; }


        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar una categoría."
        )]
        public long IdCategoria { get; set; }


        [Required(
            ErrorMessage = "Debe indicar el código."
        )]
        [MaxLength(50)]
        public string Codigo { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Debe indicar el nombre."
        )]
        [MaxLength(150)]
        public string Nombre { get; set; }
            = string.Empty;


        [MaxLength(300)]
        public string? Descripcion { get; set; }


        [Range(
            0,
            double.MaxValue,
            ErrorMessage = "El precio de compra no puede ser negativo."
        )]
        public decimal PrecioCompra { get; set; }


        [Range(
            0,
            double.MaxValue,
            ErrorMessage = "El precio de venta no puede ser negativo."
        )]
        public decimal PrecioVenta { get; set; }


        [Range(
            0,
            double.MaxValue,
            ErrorMessage = "El stock mínimo no puede ser negativo."
        )]
        public decimal StockMinimo { get; set; }


        public List<CategoriaResponse> Categorias { get; set; }
            = new List<CategoriaResponse>();
    }
}