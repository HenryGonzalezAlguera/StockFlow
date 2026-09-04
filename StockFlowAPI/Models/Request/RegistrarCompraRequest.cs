using System.ComponentModel.DataAnnotations;

namespace StockFlowAPI.Models.Request
{
    public class RegistrarCompraRequest
    {
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un proveedor."
        )]
        public long IdProveedor { get; set; }


        [Range(
            0,
            100,
            ErrorMessage = "El impuesto debe estar entre 0 y 100."
        )]
        public decimal PorcentajeImpuesto { get; set; }


        [Required(
            ErrorMessage = "Debe incluir productos en la compra."
        )]
        [MinLength(
            1,
            ErrorMessage = "Debe incluir al menos un producto."
        )]
        public List<RegistrarCompraDetalleRequest> Detalles { get; set; }
            = new List<RegistrarCompraDetalleRequest>();
    }
}