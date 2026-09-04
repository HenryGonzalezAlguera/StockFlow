using System.ComponentModel.DataAnnotations;

namespace StockFlowAPI.Models.Request
{
    public class RegistrarVentaDetalleRequest
    {
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "El producto no es válido."
        )]
        public long IdProducto { get; set; }


        [Range(
            0.001,
            double.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a cero."
        )]
        public decimal Cantidad { get; set; }
    }
}