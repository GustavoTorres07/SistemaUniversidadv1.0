using SistemaUniversidadv1._0.Models;  // Importa el espacio de nombres para los modelos de la aplicación
using System;  // Importa el espacio de nombres para clases base de .NET
using System.Linq;  // Importa el espacio de nombres para consultas LINQ
using System.Web;  // Importa el espacio de nombres para funcionalidades web
using System.Web.Mvc;  // Importa el espacio de nombres para MVC (Model-View-Controller)
using System.Web.Security;  // Importa el espacio de nombres para manejar autenticación y seguridad en formularios

namespace SistemaUniversidadv1._0.Controllers  
{
    public class AccesoController : Controller  
    {
        private readonly UniversidadContext db;  // Declara un contexto de base de datos para interactuar con el modelo de datos

        public AccesoController()  // Constructor que inicializa el contexto de base de datos
        {
            db = new UniversidadContext();  // Inicializa el contexto de base de datos
        }

        // Acción GET: Muestra el formulario de login
        public ActionResult Login()  // Acción que devuelve la vista de login
        {
            return View();  // Devuelve la vista de login
        }

        // Acción POST: Procesa el login y autentica al usuario
        [HttpPost]  // Indica que esta acción solo acepta solicitudes POST
        [ValidateAntiForgeryToken]  
        public ActionResult Login(USUARIO model)  // Recibe el modelo USUARIO con los datos del formulario
        {
            if (!ModelState.IsValid)  // Si el modelo no es válido 
            {
                TempData["LoginError"] = "Datos de inicio de sesión inválidos.";  // Muestra un mensaje de error
                return RedirectToAction("Login");  // Redirige a la vista de login
            }

            try
            {
                // Busca al usuario en la base de datos por el nombre de usuario ingresado
                var usuario = db.USUARIO.FirstOrDefault(u => u.usuario_usuario == model.usuario_usuario);

                // Verifica si el usuario existe y si la contraseña proporcionada es correcta usando BCrypt
                if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.clave_usuario, usuario.clave_usuario))
                {
                    TempData["LoginError"] = "Usuario o clave incorrectos.";  // Muestra mensaje de error si los datos son incorrectos
                    return RedirectToAction("Login");  // Redirige nuevamente al formulario de login
                }

                // Obtiene el nombre del rol del usuario desde la base de datos
                var rol = db.ROL.FirstOrDefault(r => r.id_rol == usuario.rol_id)?.nombre_rol;

                // Almacena información del usuario y su rol en la sesión
                Session["usuario_id"] = usuario.id_usuario;  // Guarda el ID del usuario
                Session["usuario_usuario"] = usuario.usuario_usuario;  // Guarda el nombre de usuario
                Session["rol_id"] = usuario.rol_id;  // Guarda el ID del rol
                Session["nombre_rol"] = rol;  // Guarda el nombre del rol

                // Crea un ticket de autenticación para el usuario y una cookie
                var authTicket = new FormsAuthenticationTicket(
                    1,  // Versión del ticket (generalmente es 1)
                    usuario.usuario_usuario,  // Nombre de usuario
                    DateTime.Now,  // Hora de creación del ticket
                    DateTime.Now.AddMinutes(30),  // Hora de expiración del ticket (30 minutos)
                    false,  // Si el ticket es persistente (no lo es en este caso)
                    usuario.rol_id.ToString()  // Incluye el rol del usuario en el ticket para su uso posterior
                );

                // Encripta el ticket de autenticación
                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                // Crea una cookie para almacenar el ticket en el navegador del usuario
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(authCookie);  // Añade la cookie al encabezado de la respuesta HTTP

                // Redirige al usuario a la página principal después de un login exitoso
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)  // Si ocurre algún error durante el proceso de autenticación
            {
                // Loguea el error (puedes agregar un logger aquí si lo necesitas)
                TempData["LoginError"] = "Error al procesar la solicitud. Por favor, intente nuevamente. " + ex.Message;  // Muestra un mensaje de error
                return RedirectToAction("Login");  // Redirige al formulario de login
            }
        }

        // Acción para cerrar sesión
        [Authorize]  // Solo usuarios autenticados pueden acceder a esta acción
        public ActionResult CerrarSesion()  // Acción para cerrar la sesión del usuario
        {
            // Cierra la sesión del usuario y limpia la cookie de autenticación
            FormsAuthentication.SignOut();  // Firma al usuario
            Session.Clear();  // Limpia la sesión
            return RedirectToAction("Login");  // Redirige al formulario de login
        }

        // Acción para asegurar que el usuario esté autenticado y autorizado
        [Authorize]  // Solo usuarios autenticados pueden acceder a esta acción
        public ActionResult Home()  // Acción que maneja la página de inicio de la aplicación
        {
            // Verifica si no hay información de sesión (si el usuario no está autenticado)
            if (Session["usuario_id"] == null || Session["rol_id"] == null)
            {
                return RedirectToAction("Login");  // Si no hay sesión activa, redirige al login
            }

            // Obtiene el rol del usuario desde la sesión
            var rolId = Session["rol_id"];
            if (rolId != null)
            {
                // Redirige a diferentes vistas según el rol del usuario
                switch (rolId.ToString())
                {
                    case "1":  // Administrador
                        return RedirectToAction("Index", "Home");  // Redirige al inicio de la página para administradores
                    case "2":  // Auxiliar
                        return RedirectToAction("Index", "Home");  // Redirige al inicio de la página para auxiliares
                    case "3":  // Profesor
                        return RedirectToAction("Index", "Home");  // Redirige al inicio de la página para profesores
                    default:  // Si el rol no es reconocido
                        return RedirectToAction("Login", "Acceso");  // Redirige a la página Login
                }
            }

            return RedirectToAction("Login");  // Si no se encuentra el rol, redirige al login
        }
    }
}
