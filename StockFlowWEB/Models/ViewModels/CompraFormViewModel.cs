using System.ComponentModel.DataAnnotations;
using StockFlowWEB.Models.Request;
using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class CompraFormViewModel
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
            ErrorMessage = "El porcentaje debe estar entre 0 y 100."
        )]
        public decimal PorcentajeImpuesto { get; set; } = 13;


        public List<RegistrarCompraDetalleRequest> Detalles { get; set; }
            = new List<RegistrarCompraDetalleRequest>();


        public List<ProveedorResponse> Proveedores { get; set; }
            = new List<ProveedorResponse>();


        public List<ProductoResponse> Productos { get; set; }
            = new List<ProductoResponse>();
    }
}