using SistemaUniversidadv1._0.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SistemaUniversidadv1._0.Controllers
{
    public class EstudianteMateriaExamenController : Controller
    {
        private UniversidadContext db = new UniversidadContext(); // El contexto de la base de datos

        // Validar si el usuario tiene rol de profesor
        private bool UsuarioEsProfesor()
        {
            // Validar que la sesión tiene el ID del usuario y el rol
            if (Session["UsuarioId"] == null || Session["rol_id"] == null)
            {
                return false; // Si no hay usuario o rol en la sesión, no es profesor
            }

            int rolId = (int)Session["rol_id"];
            return rolId == 3; // Asumimos que el rol de "Profesor" tiene id = 1
        }

        // Listar las materias asignadas al profesor logueado
        public ActionResult MateriasAsignadas()
        {
            if (!UsuarioEsProfesor()) // Verificar si el usuario es profesor
            {
                return RedirectToAction("Login", "Acceso"); // Redirigir a la página de login si no es profesor
            }

            int usuarioId = (int)Session["UsuarioId"]; // Obtener usuario logueado

            var materias = db.PROFESORMATERIA
                               .Where(pm => pm.usuario_id == usuarioId)
                               .Select(pm => pm.MATERIA)
                               .ToList();

            return View(materias); // Pasar las materias a la vista
        }

        // Listar exámenes de estudiantes de una materia
        public ActionResult ExamenesPorMateria(int materiaId)
        {
            if (!UsuarioEsProfesor()) // Verificar si el usuario es profesor
            {
                return RedirectToAction("Login", "Acceso"); // Redirigir a la página de login si no es profesor
            }

            int usuarioId = (int)Session["UsuarioId"]; // Obtener usuario logueado

            // Verificar que el profesor tenga acceso a esta materia
            bool tieneAcceso = db.PROFESORMATERIA
                                   .Any(pm => pm.usuario_id == usuarioId && pm.materia_id == materiaId);

            if (!tieneAcceso)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "No tiene acceso a esta materia.");
            }

            // Obtener los exámenes de la materia
            var examenes = db.ESTUDIANTEMATERIAEXAMEN
                               .Where(eme => eme.materia_id == materiaId)
                               .Include(eme => eme.ESTUDIANTE) // Opcional, si deseas incluir información del estudiante
                               .ToList();

            return View(examenes); // Pasar los exámenes a la vista
        }

        // GET: Mostrar formulario para calificar un examen
        public ActionResult CalificarExamen(int id)
        {
            if (!UsuarioEsProfesor()) // Verificar si el usuario es profesor
            {
                return RedirectToAction("Login", "Acceso"); // Redirigir a la página de login si no es profesor
            }

            var examen = db.ESTUDIANTEMATERIAEXAMEN
                                 .FirstOrDefault(eme => eme.id_estudiante_materia_examen == id);

            if (examen == null)
            {
                return HttpNotFound("Examen no encontrado.");
            }

            return View(examen); // Pasar el examen a la vista para calificación
        }

        // POST: Guardar las calificaciones del examen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CalificarExamen(ESTUDIANTEMATERIAEXAMEN examenActualizado)
        {
            if (!UsuarioEsProfesor()) // Verificar si el usuario es profesor
            {
                return RedirectToAction("Login", "Acceso"); // Redirigir a la página de login si no es profesor
            }

            if (!ModelState.IsValid)
            {
                return View(examenActualizado); // Retornar con errores de validación
            }

            var examen = db.ESTUDIANTEMATERIAEXAMEN
                                 .FirstOrDefault(eme => eme.id_estudiante_materia_examen == examenActualizado.id_estudiante_materia_examen);

            if (examen == null)
            {
                return HttpNotFound("Examen no encontrado.");
            }

            // Actualizar las calificaciones
            examen.examen1 = examenActualizado.examen1;
            examen.recuperatorio_examen1 = examenActualizado.recuperatorio_examen1;
            examen.examen2 = examenActualizado.examen2;
            examen.recuperatorio_examen2 = examenActualizado.recuperatorio_examen2;
            examen.examen3 = examenActualizado.examen3;
            examen.recuperatorio_examen3 = examenActualizado.recuperatorio_examen3;
            examen.examen_final = examenActualizado.examen_final;
            examen.examen_integrador = examenActualizado.examen_integrador;
            examen.nota_final = examenActualizado.nota_final;
            examen.condicion_estudiante_materia_id = examenActualizado.condicion_estudiante_materia_id;

            db.SaveChanges();

            return RedirectToAction("ExamenesPorMateria", new { materiaId = examen.materia_id });
        }
    }

}