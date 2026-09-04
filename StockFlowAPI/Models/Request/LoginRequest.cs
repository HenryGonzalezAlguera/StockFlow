using System.ComponentModel.DataAnnotations;

namespace StockFlowAPI.Models.Request
{
    public class LoginRequest
    {
        [Required(
            ErrorMessage =
                "Debe indicar la identificación de la empresa."
        )]
        [MaxLength(
            30,
            ErrorMessage =
                "La identificación de la empresa no puede superar los 30 caracteres."
        )]
        public string IdentificacionEmpresa { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage =
                "Debe indicar el correo electrónico."
        )]
        [EmailAddress(
            ErrorMessage =
                "El correo electrónico no es válido."
        )]
        [MaxLength(
            150,
            ErrorMessage =
                "El correo no puede superar los 150 caracteres."
        )]
        public string Correo { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage =
                "Debe indicar la contraseña."
        )]
        [MaxLength(
            100,
            ErrorMessage =
                "La contraseña no puede superar los 100 caracteres."
        )]
        public string Contrasena { get; set; }
            = string.Empty;
    }
}