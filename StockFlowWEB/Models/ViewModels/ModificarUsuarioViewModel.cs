using StockFlowWEB.Models.Response;
using System.ComponentModel.DataAnnotations;

namespace StockFlowWEB.Models.ViewModels
{
    public class ModificarUsuarioViewModel
    {
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "El usuario no es válido."
        )]
        public long IdUsuario { get; set; }


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


        public string RolActual { get; set; }
            = string.Empty;


        public List<RolResponse> Roles { get; set; }
            = new List<RolResponse>();
    }
}
