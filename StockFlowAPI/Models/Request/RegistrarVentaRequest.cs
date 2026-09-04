using System.ComponentModel.DataAnnotations;

namespace StockFlowAPI.Models.Request
{
    public class RegistrarVentaRequest
    {
        public long? IdCliente { get; set; }


        [Range(
            0,
            100,
            ErrorMessage = "El impuesto debe estar entre 0 y 100."
        )]
        public decimal PorcentajeImpuesto { get; set; }


        [Required(
            ErrorMessage = "Debe incluir productos en la venta."
        )]
        [MinLength(
            1,
            ErrorMessage = "Debe incluir al menos un producto."
        )]
        public List<RegistrarVentaDetalleRequest> Detalles { get; set; }
            = new List<RegistrarVentaDetalleRequest>();
    }
}