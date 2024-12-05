using SistemaUniversidadv1._0.Filtros;
using SistemaUniversidadv1._0.Models;
using SistemaUniversidadv1._0.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaUniversidadv1._0.Controllers
{
    [CustomAuthorize("Administrador")]
    public class CondicionEstudianteController : Controller
    {
        private UniversidadContext db = new UniversidadContext();

        // GET: CondicionEstudiante
        [HttpGet]
        public ActionResult Index()
        {
            var viewModel = new CondicionEstudianteViewModel
            {
                Condiciones = db.CONDICIONESTUDIANTE.ToList(),
                NuevaCondicion = new CONDICIONESTUDIANTE() // Para el modal
            };

            return View(viewModel);
        }


        [HttpGet]
        public ActionResult CrearCondicionEstudiante()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearCondicionEstudiante(CondicionEstudianteViewModel viewModel)
        {
            try
            {
                var condicion = viewModel.NuevaCondicion;

                if (db.CONDICIONESTUDIANTE.Any(c => c.nombre_condicion_estudiante == condicion.nombre_condicion_estudiante))
                {
                    TempData["ErrorMessage"] = "Ya existe una condición activa con este nombre.";
                    return RedirectToAction("Index");
                }

                if (ModelState.IsValid)
                {
                    db.CONDICIONESTUDIANTE.Add(condicion);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Condición creada correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al crear la condición. Verifique los campos.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al guardar la condición: {ex.Message}";
            }
            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCondicionEstudiante(int id)
        {
            try
            {
                // Buscar la condición por su ID
                var condicion = db.CONDICIONESTUDIANTE
                    .FirstOrDefault(c => c.id_condicion_estudiante == id);

                if (condicion == null)
                {
                    TempData["ErrorMessage"] = "Condición no encontrada.";
                    return RedirectToAction("Index");
                }

                // Intentar eliminar la condición
                db.CONDICIONESTUDIANTE.Remove(condicion);
                db.SaveChanges();

                // Mensaje de éxito
                TempData["SuccessMessage"] = "Condición eliminada correctamente.";
            }
            catch (DbUpdateException)
            {
                // Manejar errores de integridad referencial
                TempData["ErrorMessage"] = "Esta condición no se puede eliminar porque está siendo utilizada.";
            }
            catch (Exception)
            {
                // Manejar otros errores generales
                TempData["ErrorMessage"] = "Ocurrió un error al intentar eliminar la condición.";
            }

            return RedirectToAction("Index");
        }




    }
}