using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Connect4.Data;
using Microsoft.EntityFrameworkCore;
using Connect4.Models;

public class JuegoController : Controller
{
    private const string SessionKey = "Juego";

    private readonly ApplicationDbContext _context;

    public JuegoController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(int? id)
    {
        Juego juego;

        if (id.HasValue)
        {
            var partida = _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Movimientos)
                .FirstOrDefault(p => p.Id == id.Value);

            if (partida != null)
            {
                juego = new Juego
                {
                    IdPartida = partida.Id,
                    NombreJugador1 = partida.Jugador1?.Nombre ?? "Desconocido",
                    NombreJugador2 = partida.Jugador2?.Nombre ?? "Desconocido",
                    JugadorActual = partida.TurnoActual
                };

                if (partida.Movimientos != null)
                {
                    foreach (var movimiento in partida.Movimientos.OrderBy(m => m.OrdenMovimiento))
                    {
                        int col = movimiento.Columna[0] - 'A';
                        juego.InsertarFichaDesdeBD(col, movimiento.Jugador);
                    }
                }

                Guardar(juego);
            }
            else
            {
                juego = Abrir();
            }
        }
        else
        {
            juego = Abrir();
        }

        return View(juego);
    }

    private Juego Abrir()
    {
        var juego = HttpContext.Session.GetObjectFromJson<Juego>(SessionKey);
        if (juego == null)
        {
            juego = new Juego();
            Guardar(juego);
        }
        return juego;
    }

    private void Guardar(Juego juego)
    {
        HttpContext.Session.SetObjectAsJson(SessionKey, juego);
    }

    [HttpPost]
    [Route("Juego/InsertarFicha/{id}")]
    public IActionResult InsertarFicha(int columna, int id)
    {
        var juego = Abrir();
        juego.IdPartida = id;
        int fila = juego.InsertarFicha(columna);

        if (fila == -1)
        {
            return RedirectToAction("Index", new { id });
        }

        Guardar(juego);

        var partida = _context.Partidas
            .Include(p => p.Movimientos)
            .FirstOrDefault(p => p.Id == juego.IdPartida);

        if (partida != null)
        {
            var movimiento = new Movimiento
            {
                PartidaId = partida.Id,
                Columna = ((char)('A' + columna)).ToString(),
                Fila = (byte)fila,
                Jugador = (byte)(juego.JugadorActual == 1 ? 2 : 1),
                OrdenMovimiento = (partida.Movimientos?.Count ?? 0) + 1
            };

            _context.Movimientos.Add(movimiento);

            partida.TurnoActual = (byte)juego.JugadorActual;
            _context.SaveChanges();
        }

        return RedirectToAction("Index", new { id });
    }

    [HttpPost]
    public IActionResult Reset()
    {
        var juego = new Juego();
        Guardar(juego);
        return RedirectToAction("Index");
    }

    public IActionResult Continuar(int id)
    {
        var partida = _context.Partidas
            .Include(p => p.Jugador1)
            .Include(p => p.Jugador2)
            .Include(p => p.Movimientos)
            .FirstOrDefault(p => p.Id == id);

        if (partida == null)
        {
            return NotFound();
        }

        var juego = new Juego
        {
            IdPartida = partida.Id,
            NombreJugador1 = partida.Jugador1?.Nombre ?? "Desconocido",
            NombreJugador2 = partida.Jugador2?.Nombre ?? "Desconocido",
            JugadorActual = partida.TurnoActual
        };

        if (partida.Movimientos != null)
        {
            foreach (var movimiento in partida.Movimientos)
            {
                int col = movimiento.Columna[0] - 'A';
                juego.InsertarFichaDesdeBD(col, movimiento.Jugador);
            }
        }

        Guardar(juego);

        return RedirectToAction("Index", new { id = partida.Id });
    }
    
    [HttpGet]
    public IActionResult Partida(int id)
    {
        var juego = Abrir();
        juego.IdPartida = id;
        Guardar(juego);
        return View("Index");
    }

}
