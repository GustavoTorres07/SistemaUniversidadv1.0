using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using SistemaUniversidadv1._0.Models;

namespace SistemaUniversidadv1._0.Controllers
{
    public class CondicionEstudianteMateriaController : Controller
    {
        private UniversidadContext db = new UniversidadContext();

        // LISTAR
        public ActionResult Index()
        {
            try
            {
                var condiciones = db.CONDICIONESTUDIANTEMATERIA.ToList();
                return View(condiciones);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar las condiciones: " + ex.Message;
                return View(new List<CONDICIONESTUDIANTEMATERIA>());
            }
        }

        // CREAR - GET

        public ActionResult CrearCondicionEstudianteMateria()

        {

            return View();

        }

        // CREAR - POST

        [HttpPost]

        [ValidateAntiForgeryToken]

        public ActionResult CrearCondicionEstudianteMateria(CONDICIONESTUDIANTEMATERIA condicion)

        {

            if (ModelState.IsValid)

            {

                db.CONDICIONESTUDIANTEMATERIA.Add(condicion);

                db.SaveChanges();

                TempData["Mensaje"] = "Condición creada correctamente.";

                return RedirectToAction("Index");

            }

            return View(condicion);

        }

        // ACTUALIZAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarCondicionEstudianteMateria(CONDICIONESTUDIANTEMATERIA condicion)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(condicion).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["Mensaje"] = "Condición actualizada correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Por favor, verifique los datos ingresados.";
                }
            }
            catch (DbEntityValidationException ex)
            {
                string errorMessage = "Error de validación: ";
                foreach (var error in ex.EntityValidationErrors)
                {
                    foreach (var validationError in error.ValidationErrors)
                    {
                        errorMessage += validationError.ErrorMessage + " ";
                    }
                }
                TempData["ErrorMessage"] = errorMessage;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar la condición: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // OBTENER CONDICIÓN PARA EDITAR (para AJAX)
        [HttpGet]
        public JsonResult ObtenerCondicion(int id)
        {
            var condicion = db.CONDICIONESTUDIANTEMATERIA.Find(id);
            if (condicion != null)
            {
                return Json(new { success = true, data = condicion }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "No se encontró la condición" }, JsonRequestBehavior.AllowGet);
        }

        // ELIMINAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCondicionEstudianteMateria(int? id_condicion_estudiante_materia)
        {
            try
            {
                if (id_condicion_estudiante_materia == null)
                {
                    TempData["ErrorMessage"] = "ID de condición no válido.";
                    return RedirectToAction("Index");
                }

                var condicion = db.CONDICIONESTUDIANTEMATERIA.Find(id_condicion_estudiante_materia);
                if (condicion != null)
                {
                    db.CONDICIONESTUDIANTEMATERIA.Remove(condicion);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Condición eliminada correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se encontró la condición.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la condición: " + ex.Message;
            }

            return RedirectToAction("Index");
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