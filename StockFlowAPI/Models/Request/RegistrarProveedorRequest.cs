using System.ComponentModel.DataAnnotations;

namespace StockFlowAPI.Models.Request
{
    public class RegistrarProveedorRequest
    {
        [MaxLength(30)]
        public string? Identificacion { get; set; }


        [Required(
            ErrorMessage = "Debe indicar el nombre."
        )]
        [MaxLength(150)]
        public string Nombre { get; set; }
            = string.Empty;


        [EmailAddress(
            ErrorMessage = "El correo electrónico no es válido."
        )]
        [MaxLength(150)]
        public string? Correo { get; set; }


        [MaxLength(30)]
        public string? Telefono { get; set; }


        [MaxLength(300)]
        public string? Direccion { get; set; }
    }
}