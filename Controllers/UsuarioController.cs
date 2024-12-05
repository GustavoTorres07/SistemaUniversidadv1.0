using SistemaUniversidadv1._0.Models; 
using System; 
using System.Collections.Generic; 
using System.Linq; // Proporciona funcionalidad para consultas LINQ.
using System.Web; 
using System.Web.Mvc; 
using BCrypt.Net; // Biblioteca para trabajar con hashing seguro de contraseñas.
using SistemaUniversidadv1._0.Filtros; // Importa un filtro personalizado para la autenticación.

namespace SistemaUniversidadv1._0.Controllers 
{
    [CustomAuthorize("Administrador")] // Restringe el acceso a este controlador únicamente a usuarios con rol "Administrador".
    public class UsuarioController : Controller 
    {
        private UniversidadContext db = new UniversidadContext(); 

        // Método para mostrar la lista de usuarios.
        public ActionResult Index(int? rolId = null) // Recibe opcionalmente un id de rol para filtrar usuarios.
        {
            try
            {
                // Carga los roles en un SelectList para usarlo en la vista.
                ViewBag.Roles = new SelectList(db.ROL, "id_rol", "nombre_rol");

                // Inicializa la consulta para obtener usuarios.
                IQueryable<USUARIO> usuarios = db.USUARIO;

                // Si se proporciona un ID de rol, filtra los usuarios por ese rol.
                if (rolId.HasValue)
                {
                    usuarios = usuarios.Where(u => u.rol_id == rolId.Value);
                }
                else
                {
                    usuarios = usuarios.Where(u => false); // Evita mostrar usuarios si no hay filtro.
                }

                // Retorna la vista con la lista de usuarios.
                return View(usuarios.ToList());
            }
            catch (Exception ex) // Maneja errores al cargar usuarios.
            {
                // Almacena un mensaje de error para mostrar en la vista.
                ViewBag.ErrorMessage = "Ocurrió un error al cargar los usuarios. " + ex.Message;
                // Retorna una lista vacía para evitar errores en la vista.
                return View(new List<USUARIO>());
            }
        }

        // Método GET para mostrar el formulario de creación de usuario.
        [HttpGet]
        public ActionResult CrearUsuario()
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false; // Desactiva proxies dinámicos para evitar errores en las vistas.

                // Carga los datos necesarios para el formulario.
                ViewBag.roles = db.ROL.ToList();
                ViewBag.Sexos = db.SEXO.ToList();
                ViewBag.localidades = db.LOCALIDAD.ToList();
                ViewBag.condicionusuario = db.CONDICIONUSUARIO.ToList();

                return View(); // Retorna la vista del formulario.
            }
            catch (Exception ex) // Maneja errores al cargar el formulario.
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar el formulario de creación. " + ex.Message;
                return View(); // Retorna la vista con un mensaje de error.
            }
        }

        // Método POST para guardar un nuevo usuario.
        [HttpPost] 
        [ValidateAntiForgeryToken] 
        public ActionResult CrearUsuario(UsuarioCLS usuarioCLS) // Recibe un modelo con los datos del usuario.
        {
            try
            {
                if (ModelState.IsValid) // Verifica que el modelo sea válido.
                {
                    // Verifica duplicados en DNI, email y nombre de usuario.
                    if (db.USUARIO.Any(u => u.dni_usuario == usuarioCLS.dni_usuario))
                    {
                        ModelState.AddModelError("dni_usuario", "Ya existe un usuario con el mismo DNI.");
                        CargarViewBags(); // Recarga datos necesarios para la vista.
                        return View(usuarioCLS); // Retorna al formulario con errores.
                    }
                    if (db.USUARIO.Any(u => u.email_usuario == usuarioCLS.email_usuario))
                    {
                        ModelState.AddModelError("email_usuario", "Ya existe un usuario con el mismo correo electrónico.");
                        CargarViewBags();
                        return View(usuarioCLS);
                    }
                    if (db.USUARIO.Any(u => u.usuario_usuario == usuarioCLS.usuario_usuario))
                    {
                        ModelState.AddModelError("usuario_usuario", "Ya existe un usuario con el mismo nombre de usuario.");
                        CargarViewBags();
                        return View(usuarioCLS);
                    }

                    // Crea una nueva instancia de usuario con los datos del modelo.
                    var usuario = new USUARIO
                    {
                        nombre_usuario = usuarioCLS.nombre_usuario,
                        apellido_usuario = usuarioCLS.apellido_usuario,
                        dni_usuario = usuarioCLS.dni_usuario,
                        fecha_nacimiento_usuario = usuarioCLS.fecha_nacimiento_usuario,
                        edad_usuario = usuarioCLS.edad_usuario,
                        celular_usuario = usuarioCLS.celular_usuario,
                        email_usuario = usuarioCLS.email_usuario,
                        fecha_registro_usuario = DateTime.Now,
                        sexo_id = usuarioCLS.sexo_id,
                        localidad_id = usuarioCLS.localidad_id,
                        condicion_usuario_id = usuarioCLS.condicion_usuario_id,
                        usuario_usuario = usuarioCLS.usuario_usuario,
                        estado_usuario = usuarioCLS.estado_usuario,
                        clave_usuario = BCrypt.Net.BCrypt.HashPassword(usuarioCLS.clave_usuario), // Hashea la contraseña.
                        rol_id = usuarioCLS.rol_id
                    };

                    db.USUARIO.Add(usuario); // Agrega el usuario a la base de datos.
                    db.SaveChanges(); // Guarda los cambios.

                    return RedirectToAction("Index", new { rolId = usuario.rol_id }); // Redirige al índice.
                }

                CargarViewBags(); // Si el modelo no es válido, recarga datos necesarios.
                return View(usuarioCLS);
            }
            catch (Exception) // Maneja errores durante la creación del usuario.
            {
                TempData["SuccessMessage"] = "El usuario se creó correctamente.";
                CargarViewBags();
                return View(usuarioCLS); // Retorna al formulario con errores.
            }
        }

        // Método privado para recargar datos de la vista.
        private void CargarViewBags()
        {
            db.Configuration.ProxyCreationEnabled = false;
            ViewBag.roles = db.ROL.ToList();
            ViewBag.Sexos = db.SEXO.ToList();
            ViewBag.localidades = db.LOCALIDAD.ToList();
            ViewBag.condicionusuario = db.CONDICIONUSUARIO.ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarUsuario(int id_Usuario)
        {
            try
            {
                USUARIO usuario = db.USUARIO.Find(id_Usuario); // Busca el usuario por ID.

                if (usuario == null) // Verifica si el usuario no existe.
                {
                    TempData["ErrorMessage"] = "El usuario no existe o ya ha sido eliminado.";
                    return RedirectToAction("Index");
                }

                db.USUARIO.Remove(usuario); // Intenta eliminar el usuario.
                db.SaveChanges(); // Guarda los cambios.

                TempData["SuccessMessage"] = "El usuario se eliminó correctamente."; // Mensaje de éxito.
                return RedirectToAction("Index");
            }
            catch (Exception) // Maneja errores durante la eliminación.
            {
                TempData["ErrorMessage"] = "Error al eliminar el usuario. " +
                    "Asegúrese de que no esté asignado en una o mas materia.";
                return RedirectToAction("Index");
            }
        }

    }
}
