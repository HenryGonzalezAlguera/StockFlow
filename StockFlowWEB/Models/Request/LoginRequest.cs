using System.ComponentModel.DataAnnotations;

namespace StockFlowWEB.Models.Request
{
    public class LoginRequest
    {
        [Required(
            ErrorMessage = "Debe indicar la identificación de la empresa."
        )]
        public string IdentificacionEmpresa { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Debe indicar el correo."
        )]
        [EmailAddress(
            ErrorMessage = "Debe indicar un correo válido."
        )]
        public string Correo { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Debe indicar la contraseña."
        )]
        public string Contrasena { get; set; }
            = string.Empty;
    }
}