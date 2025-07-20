using System.ComponentModel.DataAnnotations;

namespace Connect4.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string? Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string? Contrasena { get; set; }

        public bool Recordarme { get; set; } // opcional para checkbox "Remember me"
    }
}
