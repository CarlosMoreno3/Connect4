using System.ComponentModel.DataAnnotations;

namespace Connect4.Models
{
    public class LoginModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria")]
        [Range(100000000, 999999999, ErrorMessage = "La cédula debe tener 9 dígitos")]
        public int Cedula { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        
        [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres")]
        public string Apellido { get; set; } = string.Empty;
    }
}
