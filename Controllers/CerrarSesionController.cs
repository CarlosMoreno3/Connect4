using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Connect4.Controllers
{
    public class CerrarSesionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cerrar()
        {
            HttpContext.Session.Clear();

            TempData["MensajeCierreSesion"] = "Sesión cerrada correctamente.";
            return RedirectToAction("Index", "Login");
        }
    }
}