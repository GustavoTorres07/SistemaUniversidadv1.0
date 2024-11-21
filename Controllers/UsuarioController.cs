using SistemaUniversidadv1._0.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BCrypt.Net; // Asegúrate de incluir esta línea

namespace SistemaUniversidadv1._0.Controllers
{
    public class UsuarioController : Controller
    {
        private UniversidadContext db = new UniversidadContext(); // El contexto de la base de datos


        public ActionResult Index(int? rolId = null)
        {
            try
            {
                // Obtener todos los roles y enviarlos a la vista para el dropdown
                ViewBag.Roles = new SelectList(db.ROL, "id_rol", "nombre_rol");

                // Inicializar la lista de usuarios como vacía si no se ha seleccionado un rol
                IQueryable<USUARIO> usuarios = db.USUARIO;

                // Si rolId tiene un valor, filtra los usuarios por rol
                if (rolId.HasValue)
                {
                    usuarios = usuarios.Where(u => u.rol_id == rolId.Value);
                }
                else
                {
                    usuarios = usuarios.Where(u => false);  // No mostrar usuarios si no hay filtro
                }

                return View(usuarios.ToList());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar los usuarios. " + ex.Message;
                return View(new List<USUARIO>());
            }
        }





        // Acción para crear un nuevo usuario - Muestra el formulario
        public ActionResult CrearUsuario()
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false; // Desactiva los proxies dinámicos

                ViewBag.roles = db.ROL.ToList(); // Obtiene la lista de roles para mostrar en el formulario
                ViewBag.Sexos = db.SEXO.ToList();
                ViewBag.localidades = db.LOCALIDAD.ToList(); // Obtiene la lista de localidades para mostrar en el formulario
                ViewBag.condicionusuario = db.CONDICIONUSUARIO.ToList(); // Obtiene la lista de condicionusuario para mostrar en el formulario

                return View();
            }
            catch (Exception ex)
            {
                // Manejo del error
                ViewBag.ErrorMessage = "Ocurrió un error al cargar el formulario de creación. " + ex.Message;
                return View(); // Retorna la vista con un mensaje de error
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearUsuario(UsuarioCLS usuarioCLS)
        {
            try
            {
                // Verificar si el modelo es válido
                if (ModelState.IsValid)
                {
                    // Verificar duplicados para DNI, email y nombre de usuario
                    if (db.USUARIO.Any(u => u.dni_usuario == usuarioCLS.dni_usuario))
                    {
                        ModelState.AddModelError("dni_usuario", "Ya existe un usuario con el mismo DNI.");
                        // Recargar los ViewBag necesarios
                        CargarViewBags();
                        return View(usuarioCLS);
                    }
                    if (db.USUARIO.Any(u => u.email_usuario == usuarioCLS.email_usuario))
                    {
                        ModelState.AddModelError("email_usuario", "Ya existe un usuario con el mismo correo electrónico.");
                        // Recargar los ViewBag necesarios
                        CargarViewBags();
                        return View(usuarioCLS);
                    }
                    if (db.USUARIO.Any(u => u.usuario_usuario == usuarioCLS.usuario_usuario))
                    {
                        ModelState.AddModelError("usuario_usuario", "Ya existe un usuario con el mismo nombre de usuario.");
                        // Recargar los ViewBag necesarios
                        CargarViewBags();
                        return View(usuarioCLS);
                    }

                    // Crear una nueva entidad de usuario a partir del modelo recibido
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
                        clave_usuario = BCrypt.Net.BCrypt.HashPassword(usuarioCLS.clave_usuario),
                        rol_id = usuarioCLS.rol_id
                    };

                    db.USUARIO.Add(usuario);
                    db.SaveChanges();

                    return RedirectToAction("Index", new { rolId = usuario.rol_id });
                }

                // Si el ModelState no es válido, recargar los ViewBag
                CargarViewBags();
                return View(usuarioCLS);
            }
            catch (Exception)
            {
                TempData["SuccessMessage"] = "El usuario se creó correctamente.";
                // Recargar los ViewBag en caso de error
                CargarViewBags();
                return View(usuarioCLS);
            }
        }

        // Método privado para cargar los ViewBag
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
                // Obtener el usuario a eliminar de la base de datos
                USUARIO usuario = db.USUARIO.Find(id_Usuario);

                // Si el usuario no existe, retornar un error 404
                if (usuario == null)
                {
                    return HttpNotFound();
                }

                // Eliminar el usuario de la base de datos
                db.USUARIO.Remove(usuario);

                // Guardar los cambios en la base de datos
                db.SaveChanges();

                // Almacenar un mensaje de éxito en TempData
                TempData["SuccessMessage"] = "El usuario se eliminó correctamente.";

                // Redirigir al índice después de eliminar el usuario exitosamente
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción y agregar el mensaje de error al modelo
                ModelState.AddModelError("", "Error al eliminar el usuario: " + ex.Message);

                // Redireccionar al índice sin filtrado (puedes modificar esto si deseas filtrar)
                return RedirectToAction("Index");
            }
        }


    }
}
