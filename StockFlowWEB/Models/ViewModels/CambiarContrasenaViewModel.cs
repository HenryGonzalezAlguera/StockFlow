using System.ComponentModel.DataAnnotations;

namespace StockFlowWEB.Models.ViewModels
{
    public class CambiarContrasenaViewModel
    {
        [Required(
            ErrorMessage = "Debe indicar la contraseña actual."
        )]
        [MaxLength(
            100,
            ErrorMessage = "La contraseña actual no puede superar los 100 caracteres."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
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
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string ContrasenaNueva { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Debe confirmar la nueva contraseña."
        )]
        [Compare(
            nameof(ContrasenaNueva),
            ErrorMessage = "Las contraseñas no coinciden."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        public string ConfirmarContrasena { get; set; }
            = string.Empty;
    }
}