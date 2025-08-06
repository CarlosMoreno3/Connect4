
using Microsoft.AspNetCore.Mvc;
using Connect4.Models;
using Connect4.Data;
using System.Linq;
using System;

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

            if (jugadorExistente != null)
            {
                if (!string.Equals(jugadorExistente.Nombre.Trim(), model.Nombre.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("", "El nombre de usuario no corresponde con esta cédula.");
                    return View(model);
                }

                ViewBag.Mensaje = "Este jugador ya existe.";
                return View(model);
            }

            var nuevoJugador = new Jugador
            {
                Cedula = model.Cedula,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
            };

            _context.Jugador.Add(nuevoJugador);
            _context.SaveChanges();

            ViewBag.Mensaje = "Jugador registrado exitosamente.";
            return View();
        }
    }
}

/*using Microsoft.AspNetCore.Mvc;
using Connect4.Models;
using Connect4.Data;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;

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
            var jugador1IdStr = HttpContext.Session.GetString("Jugador1Id");
            var jugador2IdStr = HttpContext.Session.GetString("Jugador2Id");

            if (!string.IsNullOrEmpty(jugador1IdStr) && string.IsNullOrEmpty(jugador2IdStr))
            {
                ViewBag.MensajeJugador2 = "Vas a iniciar como segundo jugador";
            }
            else if (!string.IsNullOrEmpty(jugador1IdStr) && !string.IsNullOrEmpty(jugador2IdStr))
            {
                ViewBag.Error = "Ya hay dos jugadores en sesión. Debes cerrar sesión primero.";
            }

            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Buscar jugador por cédula
            var jugadorExistente = _context.Jugador.FirstOrDefault(j => j.Cedula == model.Cedula);
            long jugadorId;
            byte numeroJugadorAsignado = 1;

            if (jugadorExistente == null)
            {
                // Verificar si ya existe jugador 1 en la BD
                bool hayJugador1 = _context.Jugador.Any(j => j.NumeroJugador == 1);
                numeroJugadorAsignado = hayJugador1 ? (byte)2 : (byte)1;

                var nuevoJugador = new Jugador
                {
                    Cedula = model.Cedula,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    NumeroJugador = numeroJugadorAsignado
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
                numeroJugadorAsignado = jugadorExistente.NumeroJugador;
            }

            var jugador1IdStr = HttpContext.Session.GetString("Jugador1Id");
            var jugador2IdStr = HttpContext.Session.GetString("Jugador2Id");

            if (numeroJugadorAsignado == 1)
            {
                if (string.IsNullOrEmpty(jugador1IdStr))
                {
                    HttpContext.Session.SetString("Jugador1Id", jugadorId.ToString());
                    HttpContext.Session.SetString("Jugador1Nombre", model.Nombre);
                }
                else if (jugador1IdStr == jugadorId.ToString())
                {
                    ModelState.AddModelError("", "Este jugador ya inició sesión como Jugador 1.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "Ya hay un Jugador 1 en sesión.");
                    return View(model);
                }
            }
            else // numeroJugadorAsignado == 2
            {
                if (string.IsNullOrEmpty(jugador2IdStr))
                {
                    HttpContext.Session.SetString("Jugador2Id", jugadorId.ToString());
                    HttpContext.Session.SetString("Jugador2Nombre", model.Nombre);
                }
                else if (jugador2IdStr == jugadorId.ToString())
                {
                    ModelState.AddModelError("", "Este jugador ya inició sesión como Jugador 2.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "Ya hay un Jugador 2 en sesión.");
                    return View(model);
                }
            }

            return RedirectToAction("Index", "CargarPartida");
        }
    }
}
*/