using SistemaUniversidadv1._0.Filtros; // Utiliza filtros personalizados de autorización.
using SistemaUniversidadv1._0.Models; // Importa las clases de modelos del sistema.
using System; 
using System.Collections.Generic; // Proporciona colecciones genéricas como listas y diccionarios.
using System.Data.Entity; // Importa el Entity Framework para la manipulación de la base de datos.
using System.Linq; // Importa LINQ para consultas más sencillas en colecciones y bases de datos.
using System.Net; 
using System.Web.Mvc; 

namespace SistemaUniversidadv1._0.Controllers 
{
    // Filtro personalizado para autorizar solo a los usuarios con el rol "Administrador" o "Auxiliar".
    [CustomAuthorize("Administrador", "Auxiliar")]
    public class ProfesorMateriaController : Controller
    {
        // Instancia de la clase UniversidadContext para interactuar con la base de datos.
        private UniversidadContext db = new UniversidadContext();

        // Acción para mostrar el índice de asignaciones de materias a profesores.
        public ActionResult Index()
        {
            // Obtiene la lista de asignaciones de profesores a materias con las materias y usuarios asociados.
            var asignaciones = db.PROFESORMATERIA
                .Include(pm => pm.MATERIA)  // Incluye la relación con la entidad MATERIA.
                .Include(pm => pm.USUARIO)  // Incluye la relación con la entidad USUARIO.
                .ToList(); // Convierte el resultado en una lista.

            // Asigna una lista de carreras a la vista para usarla en un campo de selección.
            ViewBag.Carrera_id = new SelectList(db.CARRERA, "id_carrera", "nombre_carrera");

            // Devuelve la vista con la lista de asignaciones.
            return View(asignaciones);
        }

        // Acción para obtener las materias agrupadas por ciclo y carrera en formato JSON.
        public JsonResult MateriasAgrupadasPorCiclo(int carreraId)
        {
            // Obtiene los ciclos de la carrera seleccionada.
            var ciclos = db.CICLO
                .Where(c => c.carrera_id == carreraId) // Filtra los ciclos de acuerdo con la carrera.
                .Select(c => new
                {
                    Ciclo = c.nombre_ciclo,  // Nombre del ciclo.
                    Materias = db.MATERIA
                        .Where(m => m.ciclo_id == c.id_ciclo) // Filtra las materias asociadas al ciclo.
                        .Select(m => new
                        {
                            Id = m.id_materia,  // ID de la materia.
                            Nombre = m.nombre_materia,  // Nombre de la materia.
                            Profesor = db.PROFESORMATERIA
                                .Where(pm => pm.materia_id == m.id_materia)  // Busca el profesor asignado.
                                .Select(pm => pm.USUARIO.nombre_usuario + " " + pm.USUARIO.apellido_usuario) // Nombre completo del profesor.
                                .FirstOrDefault() // Toma el primer resultado, si existe.
                        }).ToList() // Convierte el resultado en una lista de materias.
                }).ToList(); // Convierte el resultado en una lista de ciclos.

            // Devuelve los ciclos y materias en formato JSON.
            return Json(ciclos, JsonRequestBehavior.AllowGet);
        }

        // Acción para mostrar un modal para asignar un profesor a una materia.
        public ActionResult AsignarProfesorModal(int materiaId)
        {
            // Obtiene la materia correspondiente al ID proporcionado.
            var materia = db.MATERIA.FirstOrDefault(m => m.id_materia == materiaId);
            if (materia == null)
                return HttpNotFound(); // Si no se encuentra la materia, devuelve un error 404.

            // Obtiene la lista de profesores (usuarios con rol de profesor).
            var profesores = db.USUARIO
                .Where(u => u.rol_id == 3) // Filtra solo a los usuarios con rol de profesor (rol_id = 3).
                .Select(u => new
                {
                    Id = u.id_usuario,  // ID del usuario (profesor).
                    Nombre = u.nombre_usuario + " " + u.apellido_usuario  // Nombre completo del profesor.
                })
                .ToList(); // Convierte el resultado en una lista.

            // Obtiene el profesor actualmente asignado a la materia.
            var profesorActual = db.PROFESORMATERIA
                .Where(pm => pm.materia_id == materiaId)
                .Select(pm => pm.usuario_id) // Obtiene el ID del usuario asignado.
                .FirstOrDefault(); // Toma el primer resultado, si existe.

            // Añade la opción "Sin asignar" al principio de la lista de profesores.
            profesores.Insert(0, new { Id = 0, Nombre = "Sin asignar" });

            // Devuelve un objeto JSON con la información de la materia, los profesores y el profesor actual asignado.
            return Json(new
            {
                MateriaId = materia.id_materia,
                MateriaNombre = materia.nombre_materia,
                Profesores = profesores,  // Lista de profesores.
                ProfesorActual = profesorActual  // Profesor actualmente asignado (si existe).
            }, JsonRequestBehavior.AllowGet); // Permite el acceso a la solicitud desde JavaScript.
        }

        // Acción POST para asignar un profesor a una materia.
        [HttpPost]
        [ValidateAntiForgeryToken] // Protege contra ataques CSRF (Cross-Site Request Forgery).
        public ActionResult AsignarProfesor(int materiaId, int? profesorId)
        {
            // Si no se proporciona un profesorId o se asigna "Sin asignar" (profesorId = 0), se desasigna al profesor.
            if (!profesorId.HasValue || profesorId == 0)
            {
                // Busca si ya existe una asignación para esta materia.
                var asignacionExistente = db.PROFESORMATERIA
                    .FirstOrDefault(pm => pm.materia_id == materiaId);

                if (asignacionExistente != null)
                {
                    // Si existe, elimina la asignación.
                    db.PROFESORMATERIA.Remove(asignacionExistente);
                    db.SaveChanges(); // Guarda los cambios en la base de datos.
                }

                // Devuelve un resultado exitoso.
                return Json(new { success = true });
            }

            try
            {
                // Busca si ya existe una asignación para esta materia.
                var asignacionExistente = db.PROFESORMATERIA
                    .FirstOrDefault(pm => pm.materia_id == materiaId);

                if (asignacionExistente != null)
                {
                    // Si existe, actualiza la asignación con el nuevo profesor.
                    asignacionExistente.usuario_id = profesorId.Value;
                }
                else
                {
                    // Si no existe, crea una nueva asignación.
                    db.PROFESORMATERIA.Add(new PROFESORMATERIA
                    {
                        materia_id = materiaId,
                        usuario_id = profesorId.Value
                    });
                }

                // Guarda los cambios en la base de datos.
                db.SaveChanges();

                // Devuelve un resultado exitoso.
                return Json(new { success = true });
            }
            catch (Exception)
            {
                // En caso de error, devuelve un resultado fallido.
                return Json(new { success = false });
            }
        }
    }
}
