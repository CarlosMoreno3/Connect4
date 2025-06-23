using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

public class JuegoController : Controller
{
    private const string SessionKey = "Juego";

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

    public IActionResult Index()
    {
        return View(Abrir());
    }

    [HttpPost]
    public IActionResult InsertarFicha(int columna)
    {
        var juego = Abrir();
        juego.InsertarFicha(columna);
        Guardar(juego);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Reset()
    {
        var juego = new Juego();
        Guardar(juego);
        return RedirectToAction("Index");
    }
}
