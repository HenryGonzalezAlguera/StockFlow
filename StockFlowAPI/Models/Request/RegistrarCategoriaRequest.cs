using System.ComponentModel.DataAnnotations;

namespace StockFlowAPI.Models.Request
{
    public class RegistrarCategoriaRequest
    {
        [Required(
            ErrorMessage =
                "Debe indicar el nombre de la categoría."
        )]
        [MaxLength(
            100,
            ErrorMessage =
                "El nombre no puede superar los 100 caracteres."
        )]
        public string Nombre { get; set; }
            = string.Empty;


        [MaxLength(
            250,
            ErrorMessage =
                "La descripción no puede superar los 250 caracteres."
        )]
        public string? Descripcion { get; set; }
    }
}