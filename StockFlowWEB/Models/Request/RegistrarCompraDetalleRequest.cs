using System.ComponentModel.DataAnnotations;

namespace StockFlowWEB.Models.Request
{
    public class RegistrarCompraDetalleRequest
    {
        [Range(1, long.MaxValue)]
        public long IdProducto { get; set; }


        [Range(
            0.001,
            double.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a cero."
        )]
        public decimal Cantidad { get; set; }


        [Range(
            0.01,
            double.MaxValue,
            ErrorMessage = "El costo debe ser mayor a cero."
        )]
        public decimal PrecioUnitario { get; set; }
    }
}