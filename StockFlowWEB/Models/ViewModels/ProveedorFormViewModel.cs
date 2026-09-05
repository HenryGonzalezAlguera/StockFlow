using System.ComponentModel.DataAnnotations;

namespace StockFlowWEB.Models.ViewModels
{
    public class ProveedorFormViewModel
    {
        public long IdProveedor { get; set; }


        [MaxLength(
            30,
            ErrorMessage =
                "La identificación no puede superar los 30 caracteres."
        )]
        public string? Identificacion { get; set; }


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


        [EmailAddress(
            ErrorMessage =
                "Debe indicar un correo válido."
        )]
        [MaxLength(
            150,
            ErrorMessage =
                "El correo no puede superar los 150 caracteres."
        )]
        public string? Correo { get; set; }


        [MaxLength(
            30,
            ErrorMessage =
                "El teléfono no puede superar los 30 caracteres."
        )]
        public string? Telefono { get; set; }


        [MaxLength(
            300,
            ErrorMessage =
                "La dirección no puede superar los 300 caracteres."
        )]
        public string? Direccion { get; set; }
    }
}