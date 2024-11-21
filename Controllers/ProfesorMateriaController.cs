using SistemaUniversidadv1._0.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SistemaUniversidadv1._0.Controllers
{
    public class ProfesorMateriaController : Controller
    {
        private UniversidadContext db = new UniversidadContext();

        // GET: ProfesorMateria
        public ActionResult Index()
        {
            var asignaciones = db.PROFESORMATERIA
                .Include(pm => pm.MATERIA)
                .Include(pm => pm.USUARIO)
                .ToList();

            ViewBag.Carrera_id = new SelectList(db.CARRERA, "id_carrera", "nombre_carrera");
            return View(asignaciones);
        }

        public JsonResult GetMateriasAgrupadasPorCiclo(int carreraId)
        {
            var ciclos = db.CICLO
                .Where(c => c.carrera_id == carreraId)
                .Select(c => new
                {
                    Ciclo = c.nombre_ciclo,
                    Materias = db.MATERIA
                        .Where(m => m.ciclo_id == c.id_ciclo)
                        .Select(m => new
                        {
                            Id = m.id_materia,
                            Nombre = m.nombre_materia,
                            Profesor = db.PROFESORMATERIA
                                .Where(pm => pm.materia_id == m.id_materia)
                                .Select(pm => pm.USUARIO.nombre_usuario + " " + pm.USUARIO.apellido_usuario)
                                .FirstOrDefault()
                        }).ToList()
                }).ToList();

            return Json(ciclos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AsignarProfesorModal(int materiaId)
        {
            var materia = db.MATERIA.FirstOrDefault(m => m.id_materia == materiaId);
            if (materia == null)
                return HttpNotFound();

            var profesores = db.USUARIO
                .Where(u => u.rol_id == 3) // Asumiendo que rol_id 3 es de los profesores
                .Select(u => new
                {
                    Id = u.id_usuario,
                    Nombre = u.nombre_usuario + " " + u.apellido_usuario
                })
                .ToList();

            // Obtener el profesor actualmente asignado
            var profesorActual = db.PROFESORMATERIA
                .Where(pm => pm.materia_id == materiaId)
                .Select(pm => pm.usuario_id)
                .FirstOrDefault();

            // Añadir la opción "Sin asignar"
            profesores.Insert(0, new { Id = 0, Nombre = "Sin asignar" });

            return Json(new
            {
                MateriaId = materia.id_materia,
                MateriaNombre = materia.nombre_materia,
                Profesores = profesores,
                ProfesorActual = profesorActual
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AsignarProfesor(int materiaId, int? profesorId)
        {
            if (!profesorId.HasValue || profesorId == 0)
            {
                // Si profesorId es 0 o null, desasignamos al profesor de la materia
                var asignacionExistente = db.PROFESORMATERIA
                    .FirstOrDefault(pm => pm.materia_id == materiaId);

                if (asignacionExistente != null)
                {
                    // Eliminar la asignación existente
                    db.PROFESORMATERIA.Remove(asignacionExistente);
                    db.SaveChanges();
                }

                return Json(new { success = true });
            }

            try
            {
                // Buscar si ya existe una asignación para esta materia
                var asignacionExistente = db.PROFESORMATERIA
                    .FirstOrDefault(pm => pm.materia_id == materiaId);

                if (asignacionExistente != null)
                {
                    // Actualizar la asignación existente
                    asignacionExistente.usuario_id = profesorId.Value;
                }
                else
                {
                    // Crear nueva asignación
                    db.PROFESORMATERIA.Add(new PROFESORMATERIA
                    {
                        materia_id = materiaId,
                        usuario_id = profesorId.Value
                    });
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
