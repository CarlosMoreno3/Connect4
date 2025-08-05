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

    public IActionResult Index(int? IdPartida, bool showModal = false)
    {
        Juego juego = new();

        if (IdPartida.HasValue)
        {
            juego = Abrir();

            JugadoresEnPartida jugadoresEnPartida = new()
            {
                NombreJugador1 = juego.NombreJugador1,
                NombreJugador2 = juego.NombreJugador2,
                TurnoActual = juego.JugadorActual,
                IdJugador1 = juego.IdJugador1,
                IdJugador2 = juego.IdJugador2
            };
            ViewBag.JugadoresEnPartida = jugadoresEnPartida;

        }
        else
        {
            List<JugadoresJuego> jugadores = [.. _context.Jugador
                .Select(j => new JugadoresJuego
                {
                    Id = j.Id,
                    Nombre = j.Nombre
                })];

            ViewBag.JugadoresDisponibles = jugadores;
            //juego = Abrir();
        }

        ViewBag.ShowModal = showModal;
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
    [Route("Juego/IniciarJuego")]
    public IActionResult IniciarJuego(int jugadorSeleccionado1, int jugadorSeleccionado2)
    {
        try
        {
            Partida partida = new()
            {
                Jugador1Id = jugadorSeleccionado1,
                Jugador2Id = jugadorSeleccionado2,
                FechaCreacion = DateTime.Now,
                TurnoActual = 1 // Comienza con el jugador 1
            };

            _context.Partidas.Add(partida);
            var result = _context.SaveChanges();
            Juego juego = CargarJuegoDesdeBD(partida.Id);

            Guardar(juego);

            return RedirectToAction("Index", new { IdPartida = partida.Id });
        }
        catch (Exception)
        {

            throw;
        }
    }

    [HttpPost]
    [Route("Juego/InsertarFicha")]
    public IActionResult InsertarFicha(int IdPartida, int columna)
    {
        var partida = _context.Partidas
            .Include(p => p.Movimientos)
            .Include(p => p.Jugador1)
            .Include(p => p.Jugador2)
            .FirstOrDefault(p => p.Id == IdPartida);

        if (partida == null || partida.Estado == "finalizada")
            return RedirectToAction("Index", new { IdPartida });

        var juego = CargarJuegoDesdeBD(IdPartida);
        byte jugadorQueHaceMovimiento = (byte)juego.JugadorActual;

        int fila = juego.InsertarFicha(columna);
        if (fila == -1)
            return RedirectToAction("Index", new { IdPartida });

        Guardar(juego);
        partida.TurnoActual = (byte)juego.JugadorActual;

        var movimiento = new Movimiento
        {
            PartidaId = partida.Id,
            Columna = ((char)('A' + columna)).ToString(),
            Fila = (byte)fila,
            Jugador = jugadorQueHaceMovimiento,
            OrdenMovimiento = (partida.Movimientos?.Count ?? 0) + 1
        };

        _context.Movimientos.Add(movimiento);

        if (juego.GameOver)
        {
            partida.Estado = "finalizada";
            partida.Resultado = juego.Winner switch
            {
                1 => "jugador1",
                2 => "jugador2",
                _ => "empate"
            };
            partida.FechaFinalizacion = DateTime.Now;

            if (partida.Jugador1 != null && partida.Jugador2 != null)
            {
                if (juego.Winner == 1)
                {
                    partida.Jugador1.PartidasGanadas++;
                    partida.Jugador2.PartidasPerdidas++;
                }
                else if (juego.Winner == 2)
                {
                    partida.Jugador2.PartidasGanadas++;
                    partida.Jugador1.PartidasPerdidas++;
                }
                else
                {
                    partida.Jugador1.PartidasEmpatadas++;
                    partida.Jugador2.PartidasEmpatadas++;
                }
            }
        }

        _context.SaveChanges();
        return RedirectToAction("Index", new { IdPartida, showModal = juego.GameOver });
    }

    [HttpPost]
    public IActionResult Reset(int IdJugadorEnPartida1Reset, int IdJugadorEnPartida2Reset)
    {
        Partida partida = new()
        {
            Jugador1Id = IdJugadorEnPartida1Reset,
            Jugador2Id = IdJugadorEnPartida2Reset,
            FechaCreacion = DateTime.Now,
            TurnoActual = 1 // Comienza con el jugador 1
        };

        _context.Partidas.Add(partida);
        var result = _context.SaveChanges();
        Juego juego = CargarJuegoDesdeBD(partida.Id);

        Guardar(juego);
        juego = Abrir();

        return RedirectToAction("Index", new { IdPartida = partida.Id });
        //var juego = new Juego();
        //Guardar(juego);
        //return RedirectToAction("Index");
    }

    public IActionResult Continuar(int id)
    {
        var juego = CargarJuegoDesdeBD(id);
        Guardar(juego);
        return RedirectToAction("Index", new { juego.IdPartida });
    }

    [HttpGet]
    public IActionResult Partida(int id)
    {
        var juego = Abrir();
        juego.IdPartida = id;
        Guardar(juego);
        return View("Index");
    }
    
    private Juego CargarJuegoDesdeBD(int id)
    {
        var partida = _context.Partidas
            .Include(p => p.Jugador1)
            .Include(p => p.Jugador2)
            .Include(p => p.Movimientos)
            .FirstOrDefault(p => p.Id == id);

        if (partida == null)
            throw new Exception("Partida no encontrada");

        var juego = new Juego
        {
            IdPartida = partida.Id,
            NombreJugador1 = partida.Jugador1?.Nombre ?? "Desconocido",
            NombreJugador2 = partida.Jugador2?.Nombre ?? "Desconocido",
            JugadorActual = partida.TurnoActual,
            IdJugador1 = partida.Jugador1Id,
            IdJugador2 = partida.Jugador2Id,
        };

        if (partida.Movimientos != null)
        {
            foreach (var movimiento in partida.Movimientos.OrderBy(m => m.OrdenMovimiento))
            {
                int col = movimiento.Columna[0] - 'A';
                juego.InsertarFichaDesdeBD(col, movimiento.Jugador);
            }
        }

        return juego;
    }
}
