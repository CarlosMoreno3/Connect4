using Connect4.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Connect4.Data; 

namespace Connect4.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginModel model)
        {
            /*
            if (!ModelState.IsValid)
                return View(model);

            // Buscar al usuario por correo y contraseña
            var jugador = _context.Jugador
                .FirstOrDefault(j => j.Correo == model.Correo && j.Contrasena == model.Contrasena);

            if (jugador == null)
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos");
                return View(model);
            }

            // Aquí podrías guardar sesión, cookie, etc.
            // Por ahora redireccionamos a una página de bienvenida*/
            return RedirectToAction("Index", "Home");
        }
    }
}