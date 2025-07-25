using Microsoft.AspNetCore.Mvc;
using Connect4.Models;
using Connect4.Data;
using System;
using System.Linq;

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
            if (!ModelState.IsValid)
                return View(model);

            var jugadorExistente = _context.Jugador.FirstOrDefault(j => j.Cedula == model.Cedula);
            long jugadorId;

            if (jugadorExistente == null)
            {
                // Crear nuevo jugador si no existe
                var nuevoJugador = new Jugador
                {
                    Cedula = model.Cedula,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido
                };

                _context.Jugador.Add(nuevoJugador);
                _context.SaveChanges();

                jugadorId = nuevoJugador.Id;
            }
            else
            {
                // Validar que el nombre coincida con la cédula
                if (!string.Equals(jugadorExistente.Nombre.Trim(), model.Nombre.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("", "El nombre de usuario no corresponde con esta cédula.");
                    return View(model);
                }

                jugadorId = jugadorExistente.Id;
            }

            // Si usas sesión, podrías guardar el ID aquí
            // HttpContext.Session.SetInt64("JugadorId", jugadorId);

            return RedirectToAction("Index", "CargarPartida");
        }
    }
}