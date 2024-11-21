using SistemaUniversidadv1._0.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SistemaUniversidadv1._0.Controllers
{
    public class EstudianteController : Controller
    {
        private readonly UniversidadContext db = new UniversidadContext();

        public ActionResult Index(int? carreraId)
        {
            // Obtén la lista de carreras desde el modelo o base de datos
            var carreras = db.CARRERA.ToList();  // Ajusta esto según tu modelo y base de datos

            // Asigna las carreras a ViewBag
            ViewBag.Carreras = carreras;

            // Si se pasa un filtro de carreraId, lo asignamos también
            ViewBag.SelectedCarreraId = carreraId;

            // Cargar estudiantes solo si se selecciona una carrera
            List<ESTUDIANTE> estudiantes = new List<ESTUDIANTE>(); // Inicializa la lista vacía por defecto

            if (carreraId.HasValue)
            {
                // Si se pasa un carreraId, cargar los estudiantes de esa carrera
                estudiantes = db.ESTUDIANTE.Where(e => e.carrera_id == carreraId).ToList();
            }

            return View(estudiantes);
        }



        public ActionResult CrearEstudiante()
        {
            CargarViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearEstudiante(EstudianteCLS estudianteCLS)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (db.ESTUDIANTE.Any(e => e.dni_estudiante == estudianteCLS.dni_estudiante))
                    {
                        ModelState.AddModelError("dni_estudiante", "Ya existe un estudiante con el mismo DNI.");
                        CargarViewBags();
                        return View(estudianteCLS);
                    }

                    var estudiante = new ESTUDIANTE
                    {
                        numero_legajo = estudianteCLS.numero_legajo,
                        nombre_estudiante = estudianteCLS.nombre_estudiante,
                        apellido_estudiante = estudianteCLS.apellido_estudiante,
                        dni_estudiante = estudianteCLS.dni_estudiante,
                        fecha_nacimiento_estudiante = estudianteCLS.fecha_nacimiento_estudiante,
                        edad_estudiante = estudianteCLS.edad_estudiante,
                        celular_estudiante = estudianteCLS.celular_estudiante,
                        fecha_registro_estudiante = DateTime.Now,
                        sexo_id = estudianteCLS.sexo_id,
                        carrera_id = estudianteCLS.carrera_id,
                        localidad_id = estudianteCLS.localidad_id,
                        estado_estudiante = estudianteCLS.estado_estudiante,
                        condicion_estudiante_id = estudianteCLS.condicion_estudiante_id,
                        email_estudiante = estudianteCLS.email_estudiante,
                    };

                    db.ESTUDIANTE.Add(estudiante);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }

                CargarViewBags();
                return View(estudianteCLS);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                CargarViewBags();
                return View(estudianteCLS);
            }
        }

            public ActionResult DetalleEstudiante(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                // Cargar el estudiante con todas sus relaciones
                var estudiante = db.ESTUDIANTE
                    .Include(e => e.CARRERA)
                    .Include(e => e.LOCALIDAD)
                    .Include(e => e.SEXO)
                    .Include(e => e.CONDICIONESTUDIANTE)
                    .FirstOrDefault(e => e.id_estudiante == id);

                if (estudiante == null)
                {
                    return HttpNotFound();
                }

                // Mapear a EstudianteCLS si es necesario
                var estudianteCLS = new EstudianteCLS
                {
                    id_estudiante = estudiante.id_estudiante,
                    nombre_estudiante = estudiante.nombre_estudiante,
                    numero_legajo = estudiante.numero_legajo,
                    apellido_estudiante = estudiante.apellido_estudiante,
                    dni_estudiante = estudiante.dni_estudiante,
                    fecha_nacimiento_estudiante = estudiante.fecha_nacimiento_estudiante,
                    edad_estudiante = estudiante.edad_estudiante,
                    celular_estudiante = estudiante.celular_estudiante,
                    email_estudiante = estudiante.email_estudiante,
                    estado_estudiante = estudiante.estado_estudiante,
                    fecha_registro_estudiante = estudiante.fecha_registro_estudiante,
                    CARRERA = estudiante.CARRERA,
                    LOCALIDAD = estudiante.LOCALIDAD,
                    SEXO = estudiante.SEXO,
                    CONDICIONESTUDIANTE = estudiante.CONDICIONESTUDIANTE
                };

                return View(estudianteCLS);
            }
        


        // Acción para eliminar un estudiante
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarEstudiante(int id_Estudiante)
        {
            try
            {
                // Obtener el estudiante a eliminar
                ESTUDIANTE estudiante = db.ESTUDIANTE.Find(id_Estudiante);

                if (estudiante == null)
                {
                    return HttpNotFound();
                }

                // Eliminar el estudiante
                db.ESTUDIANTE.Remove(estudiante);
                db.SaveChanges();

                TempData["SuccessMessage"] = "El estudiante se eliminó correctamente.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar el error
                ModelState.AddModelError("", "Error al eliminar el estudiante: " + ex.Message);
                return RedirectToAction("Index");
            }
        }
        private void CargarViewBags()
        {
            ViewBag.Sexos = new SelectList(db.SEXO.ToList(), "id_sexo", "nombre_sexo");
            ViewBag.Carreras = new SelectList(db.CARRERA.ToList(), "id_carrera", "nombre_carrera");
            ViewBag.Localidades = new SelectList(db.LOCALIDAD.ToList(), "id_localidad", "nombre_localidad");
            ViewBag.CondicionEstudiante = new SelectList(db.CONDICIONESTUDIANTE.ToList(), "id_condicion_estudiante", "nombre_condicion_estudiante");
        }
    }
}