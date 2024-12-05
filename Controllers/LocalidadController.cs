using SistemaUniversidadv1._0.Filtros;
using SistemaUniversidadv1._0.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SistemaUniversidadv1._0.Controllers
{
    [CustomAuthorize("Administrador")]
    public class LocalidadController : Controller
    {
        private UniversidadContext db = new UniversidadContext();

        // GET: Localidad
        public ActionResult Index()
        {
            var localidades = db.LOCALIDAD.ToList();  // Obtienes la lista de localidades desde tu base de datos
            return View(localidades);  // Pasas la lista a la vista
        }

        // GET: Localidad/Crear
        [HttpGet]
        public ActionResult CrearLocalidad()
        {
            return View();
        }

        // En el controlador
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearLocalidad(LOCALIDAD localidad)
        {
            try
            {
                // Verificamos si ya existe una localidad con el mismo nombre
                var localidadExistente = db.LOCALIDAD.FirstOrDefault(l =>
                    l.nombre_localidad.Trim().ToLower() == localidad.nombre_localidad.Trim().ToLower());

                if (localidadExistente != null)
                {
                    TempData["ErrorMessage"] = "Ya existe una localidad con este nombre.";
                    return RedirectToAction("Index");
                }

                if (ModelState.IsValid)
                {
                    db.LOCALIDAD.Add(localidad);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Localidad creada exitosamente.";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Por favor, complete todos los campos correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear la localidad: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // También en el método de editar para mantener la consistencia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarLocalidad(LOCALIDAD localidad)
        {
            try
            {
                // Verificamos si ya existe otra localidad con el mismo nombre (excluyendo la actual)
                var localidadExistente = db.LOCALIDAD.FirstOrDefault(l =>
                    l.nombre_localidad.Trim().ToLower() == localidad.nombre_localidad.Trim().ToLower() &&
                    l.id_localidad != localidad.id_localidad);

                if (localidadExistente != null)
                {
                    TempData["ErrorMessage"] = "Ya existe otra localidad con este nombre.";
                    return RedirectToAction("Index");
                }

                if (ModelState.IsValid)
                {
                    db.Entry(localidad).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Localidad editada exitosamente.";
                    return RedirectToAction("Index");
                }

                return View(localidad);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al editar la localidad: " + ex.Message;
                return View(localidad);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarLocalidad(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Error: ID de localidad no proporcionado.";
                return RedirectToAction("Index");
            }

            try
            {
                var localidad = db.LOCALIDAD.Find(id.Value);
                if (localidad == null)
                {
                    TempData["ErrorMessage"] = "Error: No se encontró la localidad especificada.";
                    return RedirectToAction("Index");
                }

                // Verificar si la localidad está siendo utilizada en otras tablas
                // Agrega aquí la lógica necesaria si hay relaciones con otras tablas

                db.LOCALIDAD.Remove(localidad);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Localidad eliminada exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la localidad: " + ex.Message;
                return RedirectToAction("Index");
            }
        }


    }
}
