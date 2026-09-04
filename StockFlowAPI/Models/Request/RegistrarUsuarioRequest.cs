using System.ComponentModel.DataAnnotations;

namespace StockFlowAPI.Models.Request
{
    public class RegistrarUsuarioRequest
    {
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un rol."
        )]
        public long IdRol { get; set; }


        [Required(
            ErrorMessage = "Debe indicar el nombre."
        )]
        [MaxLength(
            150,
            ErrorMessage = "El nombre no puede superar los 150 caracteres."
        )]
        public string Nombre { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Debe indicar el correo."
        )]
        [EmailAddress(
            ErrorMessage = "El correo electrónico no es válido."
        )]
        [MaxLength(
            150,
            ErrorMessage = "El correo no puede superar los 150 caracteres."
        )]
        public string Correo { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Debe indicar una contraseña."
        )]
        [MinLength(
            8,
            ErrorMessage = "La contraseña debe contener al menos 8 caracteres."
        )]
        [MaxLength(
            100,
            ErrorMessage = "La contraseña no puede superar los 100 caracteres."
        )]
        public string Contrasena { get; set; }
            = string.Empty;
    }
}
