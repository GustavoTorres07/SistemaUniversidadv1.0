using SistemaUniversidadv1._0.Filtros;
using SistemaUniversidadv1._0.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaUniversidadv1._0.Controllers
{
    [CustomAuthorize("Administrador")]
    public class SexoController : Controller
    {
        private UniversidadContext db = new UniversidadContext(); // El contexto de la base de datos

        // GET: Sexo
        public ActionResult Index()
        {
            var sexos = db.SEXO.ToList(); // Obtiene la lista de localidades desde la base de datos
            return View(sexos); // Pasa la lista a la vista
        }


        // Acción para crear una nueva localidad - Muestra el formulario
        public ActionResult CrearSexo()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar el formulario de creación. " + ex.Message;
                return View(); // Retorna la vista con un mensaje de error
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearSexo(SexoCLS sexoCLS)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar duplicados para nombre de localidad
                    if (db.SEXO.Any(s => s.nombre_sexo == sexoCLS.nombre_sexo))
                    {
                        // En lugar de retornar una vista, redirigimos al Index con un mensaje de error
                        TempData["Error"] = "Ya existe un sexo con el mismo nombre.";
                        return RedirectToAction("Index");
                    }

                    var sexo = new SEXO
                    {
                        nombre_sexo = sexoCLS.nombre_sexo,
                    };

                    db.SEXO.Add(sexo);
                    db.SaveChanges();

                    // Mensaje de éxito
                    TempData["Success"] = "Sexo Guardado exitosamente.";
                    return RedirectToAction("Index");
                }

                // Si el modelo no es válido
                TempData["Error"] = "Por favor, verifique los datos ingresados.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear el sexo: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarSexo(int id_sexo)
        {
            try
            {
                // Obtener la localidad a eliminar de la base de datos
                SEXO sexo = db.SEXO.Find(id_sexo);

                // Si la localidad no existe, retornar un error 404
                if (sexo == null)
                {
                    return HttpNotFound();
                }

                // Eliminar la localidad de la base de datos
                db.SEXO.Remove(sexo);

                // Guardar los cambios en la base de datos
                db.SaveChanges();

                // Almacenar un mensaje de éxito en TempData
                TempData["Success"] = "el sexo se eliminó correctamente.";

                // Redirigir al índice después de eliminar la localidad exitosamente
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Capturar el mensaje de error y pasarlo a TempData
                TempData["Error"] = "Error al eliminar el sexo: ";
                return RedirectToAction("Index");
            }
        }


    }
}