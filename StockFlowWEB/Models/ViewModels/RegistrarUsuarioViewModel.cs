using StockFlowWEB.Models.Response;
using System.ComponentModel.DataAnnotations;

namespace StockFlowWEB.Models.ViewModels
{
    public class RegistrarUsuarioViewModel
    {
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un rol."
        )]
        [Display(Name = "Rol")]
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
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Debe confirmar la contraseña."
        )]
        [Compare(
            nameof(Contrasena),
            ErrorMessage = "Las contraseñas no coinciden."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarContrasena { get; set; }
            = string.Empty;


        public List<RolResponse> Roles { get; set; }
            = new List<RolResponse>();
    }
}