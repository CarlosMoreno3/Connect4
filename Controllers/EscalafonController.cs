using Microsoft.AspNetCore.Mvc;
using Connect4.Models;
using Connect4.Data;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;

public class EscalafonController : Controller
{
    private readonly ApplicationDbContext _context;

    public EscalafonController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var jugadores = _context.Jugador
            .OrderByDescending(j => j.Marcador)
            .ToList();

        return View(jugadores);
    }
}