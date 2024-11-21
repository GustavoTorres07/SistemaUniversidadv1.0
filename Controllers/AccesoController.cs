using SistemaUniversidadv1._0.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SistemaUniversidadv1._0.Controllers
{
    public class AccesoController : Controller
    {
        private readonly UniversidadContext db = new UniversidadContext();
        public AccesoController()
        {
            db = new UniversidadContext();
        }

        // Implementa IDisposable para liberar recursos
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Login(string username, string password)
        {
            // Validar las credenciales del usuario
            var usuario = db.USUARIO.FirstOrDefault(u => u.nombre_usuario == username && u.clave_usuario == password);

            if (usuario != null)
            {
                // Almacenar el usuario_id y rol_id en la sesión
                Session["usuario_id"] = usuario.id_usuario;
                Session["rol_id"] = usuario.rol_id; // Guardar el rol también si lo necesitas

                // Redirigir a la vista correspondiente (por ejemplo, el panel del profesor)
                return RedirectToAction("Home", "Index");
            }

            // Si el login es incorrecto, mostrar mensaje de error
            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(USUARIO model)
        {
            if (!ModelState.IsValid)
            {
                TempData["LoginError"] = "Datos de inicio de sesión inválidos.";
                return RedirectToAction("Login");
            }

            try
            {
                // Busca al usuario en la base de datos por el nombre de usuario
                var usuario = db.USUARIO.FirstOrDefault(u => u.usuario_usuario == model.usuario_usuario);

                // Verifica si el usuario existe y si la contraseña proporcionada es correcta
                if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.clave_usuario, usuario.clave_usuario))
                {
                    TempData["LoginError"] = "Usuario o clave incorrectos.";
                    return RedirectToAction("Login");
                }
                // Almacenar el usuario_id y rol_id en la sesión
                Session["usuario_id"] = usuario.id_usuario;  // Guarda el id del usuario
                Session["rol_id"] = usuario.rol_id;  // Guarda el rol del usuario
                // Crea un ticket de autenticación y una cookie para el usuario autenticado
                var authTicket = new FormsAuthenticationTicket(
                    1,
                    usuario.usuario_usuario,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    false,
                    FormsAuthentication.FormsCookiePath
                );

                // Encripta el ticket y lo agrega a la cookie de autenticación
                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(authCookie);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                // Log the exception here
                TempData["LoginError"] = "Error al procesar la solicitud. Por favor, intente nuevamente.";
                return RedirectToAction("Login");
            }
        }

        [Authorize]
        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}