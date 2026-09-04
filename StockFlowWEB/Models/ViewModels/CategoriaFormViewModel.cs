using System.ComponentModel.DataAnnotations;

namespace StockFlowWEB.Models.ViewModels
{
    public class CategoriaFormViewModel
    {
        public long IdCategoria { get; set; }


        [Required(
            ErrorMessage = "Debe indicar el nombre."
        )]
        [MaxLength(
            100,
            ErrorMessage = "El nombre no puede superar los 100 caracteres."
        )]
        public string Nombre { get; set; }
            = string.Empty;


        [MaxLength(
            300,
            ErrorMessage = "La descripción no puede superar los 300 caracteres."
        )]
        public string? Descripcion { get; set; }
    }
}