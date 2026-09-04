using System.ComponentModel.DataAnnotations;

namespace StockFlowAPI.Models.Request
{
    public class RegistrarEmpresaRequest
    {
        [Required(
            ErrorMessage =
                "Debe indicar la identificación de la empresa."
        )]
        [MaxLength(
            30,
            ErrorMessage =
                "La identificación no puede superar los 30 caracteres."
        )]
        public string IdentificacionEmpresa { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage =
                "Debe indicar el nombre de la empresa."
        )]
        [MaxLength(
            150,
            ErrorMessage =
                "El nombre de la empresa no puede superar los 150 caracteres."
        )]
        public string NombreEmpresa { get; set; }
            = string.Empty;


        [EmailAddress(
            ErrorMessage =
                "El correo de la empresa no es válido."
        )]
        [MaxLength(
            150,
            ErrorMessage =
                "El correo de la empresa no puede superar los 150 caracteres."
        )]
        public string? CorreoEmpresa { get; set; }


        [MaxLength(
            30,
            ErrorMessage =
                "El teléfono no puede superar los 30 caracteres."
        )]
        public string? TelefonoEmpresa { get; set; }


        [MaxLength(
            300,
            ErrorMessage =
                "La dirección no puede superar los 300 caracteres."
        )]
        public string? DireccionEmpresa { get; set; }


        [Required(
            ErrorMessage =
                "Debe indicar el nombre del administrador."
        )]
        [MaxLength(
            150,
            ErrorMessage =
                "El nombre del administrador no puede superar los 150 caracteres."
        )]
        public string NombreAdministrador { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage =
                "Debe indicar el correo del administrador."
        )]
        [EmailAddress(
            ErrorMessage =
                "El correo del administrador no es válido."
        )]
        [MaxLength(
            150,
            ErrorMessage =
                "El correo del administrador no puede superar los 150 caracteres."
        )]
        public string CorreoAdministrador { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage =
                "Debe indicar una contraseña."
        )]
        [MinLength(
            8,
            ErrorMessage =
                "La contraseña debe contener al menos 8 caracteres."
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