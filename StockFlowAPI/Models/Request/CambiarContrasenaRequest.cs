using System.ComponentModel.DataAnnotations;

namespace StockFlowAPI.Models.Request
{
    public class CambiarContrasenaRequest
    {
        [Required(
            ErrorMessage = "Debe indicar la contraseña actual."
        )]
        [MaxLength(
            100,
            ErrorMessage = "La contraseña actual no puede superar los 100 caracteres."
        )]
        public string ContrasenaActual { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Debe indicar la nueva contraseña."
        )]
        [MinLength(
            8,
            ErrorMessage = "La nueva contraseña debe contener al menos 8 caracteres."
        )]
        [MaxLength(
            100,
            ErrorMessage = "La nueva contraseña no puede superar los 100 caracteres."
        )]
        public string ContrasenaNueva { get; set; }
            = string.Empty;
    }
}

