using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using Connect4.Models;

namespace Proyecto.Controllers
{
    public class RegisterController : Controller
    {
        private readonly string _connectionString = "Server=JOSE;Database=Connect4DB;User Id=Connect4;Password=Connect1234;TrustServerCertificate=True;";

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("sp_crear_jugador", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombre", model.Nombre);
                    cmd.Parameters.AddWithValue("@apellido", model.Apellido);
                    cmd.Parameters.AddWithValue("@correo", model.Correo);
                    cmd.Parameters.AddWithValue("@contrasena", model.Contrasena); // Se recomienda aplicar hash

                    var result = cmd.ExecuteScalar();

                    TempData["RegistroExitoso"] = "Registro exitoso. ID: " + result;
                    return RedirectToAction("Index", "Login");
                }
                catch (SqlException ex)
                {
                    ModelState.AddModelError("", "Error al registrar: " + ex.Message);
                    return View(model);
                }
            }
        }
    }
}
