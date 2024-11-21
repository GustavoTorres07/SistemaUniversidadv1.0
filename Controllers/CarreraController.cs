using SistemaUniversidadv1._0.Filtros;
using SistemaUniversidadv1._0.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SistemaUniversidadv1._0.Controllers
{
    [CustomAuthorize("Administrador")]
    public class CarreraController : Controller
    {
        private UniversidadContext db = new UniversidadContext();

        // GET: Carrera
        public ActionResult Index()
        {
            try
            {
                var carreras = db.CARRERA.ToList(); // Obtener todas las carreras de la base de datos
                return View(carreras); // Pasar el listado de carreras a la vista
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar el listado de carreras. " + ex.Message;
                return View(); // Retorna la vista con un mensaje de error
            }
        }

        public ActionResult CrearCarrera()
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
        public ActionResult CrearCarrera(CarreraCLS carreraCLS)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar duplicados para nombre de carrera
                    if (db.CARRERA.Any(c => c.nombre_carrera == carreraCLS.nombre_carrera))
                    {
                        TempData["Error"] = "Ya existe una Carrera con el mismo nombre.";
                        return RedirectToAction("Index");
                    }

                    var carrera = new CARRERA
                    {
                        nombre_carrera = carreraCLS.nombre_carrera,
                        estado_carrera = carreraCLS.estado_carrera
                    };

                    db.CARRERA.Add(carrera);
                    db.SaveChanges();

                    TempData["Success"] = "Carrera Guardada exitosamente.";
                    return RedirectToAction("Index");
                }

                TempData["Error"] = "Por favor, verifique los datos ingresados.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear la Carrera: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Carrera/Editar/{id}
        public ActionResult EditarCarrera(int id_carrera)
        {
            try
            {
                var carrera = db.CARRERA.Find(id_carrera);
                if (carrera == null)
                {
                    return HttpNotFound();
                }

                var carreraCLS = new CarreraCLS
                {
                    id_carrera = carrera.id_carrera,
                    nombre_carrera = carrera.nombre_carrera,
                    estado_carrera = carrera.estado_carrera
                };

                return View(carreraCLS);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar la carrera para editar. " + ex.Message;
                return View();
            }
        }

        // POST: Carrera/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarCarrera(CarreraCLS carreraCLS)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var carrera = db.CARRERA.Find(carreraCLS.id_carrera);
                    if (carrera == null)
                    {
                        return HttpNotFound();
                    }

                    // Verificar si ya existe una carrera con el mismo nombre (pero diferente id)
                    if (db.CARRERA.Any(c => c.nombre_carrera == carreraCLS.nombre_carrera && c.id_carrera != carreraCLS.id_carrera))
                    {
                        TempData["Error"] = "Ya Existe una Carrera con el Mismo Nombre.";
                        return RedirectToAction("Index");
                    }

                    // Actualizar los datos de la carrera
                    carrera.nombre_carrera = carreraCLS.nombre_carrera;
                    carrera.estado_carrera = carreraCLS.estado_carrera;

                    db.SaveChanges();

                    TempData["Success"] = "La Carrera se Actualizó Exitosamente.";
                    return RedirectToAction("Index");
                }

                TempData["Error"] = "Por Favor, Verifique Los Datos Ingresados.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al Editar la Carrera: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public ActionResult ActivarCarrera(int id)
        {
            using (var db = new UniversidadContext())
            {
                var carrera = db.CARRERA.Find(id);
                if (carrera == null)
                {
                    return HttpNotFound();
                }

                carrera.estado_carrera = true;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }


        public ActionResult DesactivarCarrera(int id)
        {
            using (var db = new UniversidadContext())
            {
                // Buscar la carrera por ID
                var carrera = db.CARRERA.Find(id);
                if (carrera == null)
                {
                    return HttpNotFound();
                }

                // Cambiar el estado a inactivo
                carrera.estado_carrera = false;
                db.SaveChanges();
            }

            return RedirectToAction("Index"); // Redirige a la lista de carreras
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCarrera(int id_carrera)
        {
            try
            {
                // Buscar la carrera y sus ciclos y materias relacionadas
                var carrera = db.CARRERA.Find(id_carrera);

                // Si no se encuentra la carrera, retornar un error 404
                if (carrera == null)
                {
                    return HttpNotFound();
                }

                // Buscar ciclos asociados a la carrera
                var ciclos = db.CICLO.Where(c => c.carrera_id == id_carrera).ToList();

                foreach (var ciclo in ciclos)
                {
                    // Buscar materias asociadas a cada ciclo de la carrera
                    var materias = db.MATERIA.Where(m => m.ciclo_id == ciclo.id_ciclo).ToList();

                    // Eliminar las materias asociadas al ciclo
                    db.MATERIA.RemoveRange(materias);
                }

                // Eliminar los ciclos asociados a la carrera
                db.CICLO.RemoveRange(ciclos);

                // Finalmente, eliminar la carrera
                db.CARRERA.Remove(carrera);

                // Guardar todos los cambios en la base de datos
                db.SaveChanges();

                // Mensaje de éxito
                TempData["Success"] = "La carrera se elimino correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejo de errores
                TempData["Error"] = "Error al eliminar la carrera " + ex.Message;
                return RedirectToAction("Index");
            }
        }





    }
}