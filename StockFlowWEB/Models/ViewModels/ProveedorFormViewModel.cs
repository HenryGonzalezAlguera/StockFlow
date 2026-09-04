using System.ComponentModel.DataAnnotations;

namespace StockFlowWEB.Models.ViewModels
{
    public class ProveedorFormViewModel
    {
        public long IdProveedor { get; set; }


        [MaxLength(30)]
        public string? Identificacion { get; set; }


        [Required(
            ErrorMessage = "Debe indicar el nombre."
        )]
        [MaxLength(150)]
        public string Nombre { get; set; }
            = string.Empty;


        [EmailAddress(
            ErrorMessage = "Debe indicar un correo válido."
        )]
        [MaxLength(150)]
        public string? Correo { get; set; }


        [MaxLength(30)]
        public string? Telefono { get; set; }


        [MaxLength(300)]
        public string? Direccion { get; set; }
    }
}