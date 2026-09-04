using System.ComponentModel.DataAnnotations;
using StockFlowWEB.Models.Request;
using StockFlowWEB.Models.Response;

namespace StockFlowWEB.Models.ViewModels
{
    public class VentaFormViewModel
    {
        public long? IdCliente { get; set; }


        [Range(
            0,
            100,
            ErrorMessage = "El impuesto debe estar entre 0 y 100."
        )]
        public decimal PorcentajeImpuesto { get; set; } = 13;


        public List<RegistrarVentaDetalleRequest> Detalles { get; set; }
            = new List<RegistrarVentaDetalleRequest>();


        public List<ClienteResponse> Clientes { get; set; }
            = new List<ClienteResponse>();


        public List<ProductoResponse> Productos { get; set; }
            = new List<ProductoResponse>();
    }
}