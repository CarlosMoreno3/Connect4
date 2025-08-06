using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using Connect4.Models;
using Connect4.Data;

public class CargarPartidaController : Controller
{
    private readonly ApplicationDbContext _context;

    public CargarPartidaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var partidas = _context.Partidas
            .Include(p => p.Jugador1)
            .Include(p => p.Jugador2)
            .OrderByDescending(p => p.FechaCreacion)
            .ToList();
        return View(partidas);
    }

    [HttpGet]
    public IActionResult Continuar(int id)
    {
        var partida = _context.Partidas.FirstOrDefault(p => p.Id == id);
        if (partida == null)
        {
            return NotFound();
        }
        return RedirectToAction("Continuar", "Juego", new { id = partida.Id });
    }
    
    public async Task<IActionResult> CargarPartida(int id)
    {
        var partida = await _context.Partidas
            .Include(p => p.Jugador1)
            .Include(p => p.Jugador2)
            .Include(p => p.Movimientos)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (partida == null)
            return NotFound();

        return View(partida);
    }

}
