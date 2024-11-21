using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SistemaUniversidadv1._0.Models;

namespace SistemaUniversidadv1._0.Controllers
{
    public class CicloController : Controller
    {
        private UniversidadContext db = new UniversidadContext();

        public ActionResult Index(int? carrera_id)
        {
            // Cargar la lista de carreras para el dropdown
            ViewBag.Carreras = db.CARRERA.Where(c => c.estado_carrera).ToList();

            // Si se selecciona una carrera, filtrar ciclos por esa carrera
            if (carrera_id != null)
            {
                var carreraSeleccionada = db.CARRERA.Find(carrera_id);
                ViewBag.CarreraSeleccionada = carreraSeleccionada?.nombre_carrera;

                var ciclos = db.CICLO.Where(c => c.carrera_id == carrera_id).ToList();
                if (!ciclos.Any())
                {
                    ViewBag.Mensaje = "No hay ciclos para la carrera seleccionada.";
                }
                return View(ciclos);
            }
            else
            {
                // Cuando no hay carrera seleccionada, enviar un mensaje
                ViewBag.Mensaje = "Por favor seleccione una carrera para ver sus ciclos.";
                return View(new List<CICLO>());
            }
        }

        // Acción para Crear Ciclo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearCiclo(int carrera_id, string nombre_ciclo)
        {
            // Verificar si ya existe un ciclo con el mismo nombre en la carrera seleccionada
            if (db.CICLO.Any(c => c.carrera_id == carrera_id && c.nombre_ciclo == nombre_ciclo))
            {
                TempData["Error"] = "El ciclo ya existe en esta carrera.";
                return RedirectToAction("Index", new { carrera_id });
            }

            // Crear el nuevo ciclo
            var nuevoCiclo = new CICLO
            {
                carrera_id = carrera_id,
                nombre_ciclo = nombre_ciclo
            };
            db.CICLO.Add(nuevoCiclo);
            db.SaveChanges();

            TempData["Success"] = "Ciclo creado exitosamente.";
            return RedirectToAction("Index", new { carrera_id });
        }

        // Acción para Eliminar Ciclo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCiclo(int id_ciclo)
        {
            var ciclo = db.CICLO.Find(id_ciclo);

            // Verificar si el ciclo tiene materias asociadas
            if (db.MATERIA.Any(m => m.ciclo_id == id_ciclo))
            {
                TempData["Error"] = "No se puede eliminar el ciclo porque tiene materias asociadas.";
                return RedirectToAction("Index", new { carrera_id = ciclo.carrera_id });
            }

            // Eliminar el ciclo
            db.CICLO.Remove(ciclo);
            db.SaveChanges();

            TempData["Success"] = "Ciclo eliminado exitosamente.";
            return RedirectToAction("Index", new { carrera_id = ciclo.carrera_id });
        }
    }
}
