using System.ComponentModel.DataAnnotations;

namespace StockFlowWEB.Models.ViewModels
{
    public class RegistrarEmpresaViewModel
    {
        // =========================================================
        // EMPRESA
        // =========================================================

        [Required(
            ErrorMessage =
                "Debe indicar la identificación de la empresa."
        )]
        [MaxLength(
            30,
            ErrorMessage =
                "La identificación no puede superar los 30 caracteres."
        )]
        [Display(
            Name =
                "Identificación de la empresa"
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
        [Display(
            Name =
                "Nombre de la empresa"
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
        [Display(
            Name =
                "Correo de la empresa"
        )]
        public string? CorreoEmpresa { get; set; }


        [MaxLength(
            30,
            ErrorMessage =
                "El teléfono no puede superar los 30 caracteres."
        )]
        [Display(
            Name =
                "Teléfono"
        )]
        public string? TelefonoEmpresa { get; set; }


        [MaxLength(
            300,
            ErrorMessage =
                "La dirección no puede superar los 300 caracteres."
        )]
        [Display(
            Name =
                "Dirección"
        )]
        public string? DireccionEmpresa { get; set; }


        // =========================================================
        // ADMINISTRADOR
        // =========================================================

        [Required(
            ErrorMessage =
                "Debe indicar el nombre del administrador."
        )]
        [MaxLength(
            150,
            ErrorMessage =
                "El nombre del administrador no puede superar los 150 caracteres."
        )]
        [Display(
            Name =
                "Nombre del administrador"
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
        [Display(
            Name =
                "Correo del administrador"
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
        [DataType(
            DataType.Password
        )]
        [Display(
            Name =
                "Contraseña"
        )]
        public string Contrasena { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage =
                "Debe confirmar la contraseña."
        )]
        [DataType(
            DataType.Password
        )]
        [Compare(
            nameof(Contrasena),
            ErrorMessage =
                "Las contraseñas no coinciden."
        )]
        [Display(
            Name =
                "Confirmar contraseña"
        )]
        public string ConfirmarContrasena { get; set; }
            = string.Empty;
    }
}