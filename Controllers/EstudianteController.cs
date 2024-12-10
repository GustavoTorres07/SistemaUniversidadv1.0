using SistemaUniversidadv1._0.Filtros;  // Importa los filtros personalizados (como autorización).
using SistemaUniversidadv1._0.Models;   // Importa los modelos del sistema (como los estudiantes, carreras, etc.).
using System;
using System.Collections.Generic;
using System.Data.Entity;               // Importa la librería para interactuar con Entity Framework.
using System.Linq;                      // Importa LINQ para consultas de datos.
using System.Net;
using System.Web.Mvc;

namespace SistemaUniversidadv1._0.Controllers
{
    // Atributo de autorización personalizada, restringe el acceso solo a Administradores y Auxiliares.
    [CustomAuthorize("Administrador", "Auxiliar")]
    public class EstudianteController : Controller
    {
        // Instancia del contexto de la base de datos para interactuar con las tablas de la base de datos.
        private readonly UniversidadContext db = new UniversidadContext();

        // Acción para mostrar la lista de estudiantes, con un filtro opcional por carrera.
        public ActionResult Index(int? carreraId)
        {
            // Obtiene todas las carreras desde la base de datos.
            var carreras = db.CARRERA.ToList();

            // Pasa las carreras a la vista para mostrar en un dropdown.
            ViewBag.Carreras = carreras;

            // Si se pasa un carreraId, lo asigna a ViewBag para marcar el filtro seleccionado.
            ViewBag.SelectedCarreraId = carreraId;

            // Inicializa una lista vacía de estudiantes por defecto.
            List<ESTUDIANTE> estudiantes = new List<ESTUDIANTE>();

            // Si carreraId tiene valor, filtra los estudiantes por la carrera seleccionada.
            if (carreraId.HasValue)
            {
                estudiantes = db.ESTUDIANTE.Where(e => e.carrera_id == carreraId).ToList();
            }

            // Devuelve la lista de estudiantes a la vista.
            return View(estudiantes);
        }

        // Acción GET para mostrar el formulario de creación de un estudiante.
        [HttpGet]
        public ActionResult CrearEstudiante()
        {
            // Llama a un método para cargar las opciones 
            CargarViewBags();

            // Devuelve la vista para crear un estudiante.
            return View();
        }

        // Acción POST para guardar un nuevo estudiante en la base de datos.
        [HttpPost]
        [ValidateAntiForgeryToken]  // Protección contra ataques CSRF.
        public ActionResult CrearEstudiante(EstudianteCLS estudianteCLS)
        {
            try
            {
                // Verifica si el modelo es válido (validación de las reglas definidas en el modelo).
                if (ModelState.IsValid)
                {
                    // Verifica si ya existe un estudiante con el mismo DNI.
                    if (db.ESTUDIANTE.Any(e => e.dni_estudiante == estudianteCLS.dni_estudiante))
                    {
                        // Si ya existe, agrega un error al modelo y recarga los select lists.
                        ModelState.AddModelError("dni_estudiante", "Ya existe un estudiante con el mismo DNI.");
                        CargarViewBags();
                        return View(estudianteCLS);
                    }

                    // Crea un objeto ESTUDIANTE con los datos del formulario.
                    var estudiante = new ESTUDIANTE
                    {
                        numero_legajo = estudianteCLS.numero_legajo,
                        nombre_estudiante = estudianteCLS.nombre_estudiante,
                        apellido_estudiante = estudianteCLS.apellido_estudiante,
                        dni_estudiante = estudianteCLS.dni_estudiante,
                        fecha_nacimiento_estudiante = estudianteCLS.fecha_nacimiento_estudiante,
                        edad_estudiante = estudianteCLS.edad_estudiante,
                        celular_estudiante = estudianteCLS.celular_estudiante,
                        fecha_registro_estudiante = DateTime.Now, // Fecha actual para el registro.
                        sexo_id = estudianteCLS.sexo_id,
                        carrera_id = estudianteCLS.carrera_id,
                        localidad_id = estudianteCLS.localidad_id,
                        estado_estudiante = estudianteCLS.estado_estudiante,
                        condicion_estudiante_id = estudianteCLS.condicion_estudiante_id,
                        email_estudiante = estudianteCLS.email_estudiante,
                    };

                    // Agrega el nuevo estudiante a la base de datos y guarda los cambios.
                    db.ESTUDIANTE.Add(estudiante);
                    db.SaveChanges();

                    // Redirige a la vista Index después de guardar el nuevo estudiante.
                    return RedirectToAction("Index");
                }

                // Si el modelo no es válido, recarga los select lists y vuelve al formulario.
                CargarViewBags();
                return View(estudianteCLS);
            }
            catch (Exception ex)
            {
                // En caso de error, muestra el mensaje de error y vuelve al formulario.
                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                CargarViewBags();
                return View(estudianteCLS);
            }
        }

        // Acción para mostrar los detalles de un estudiante específico.
        public ActionResult DetalleEstudiante(int? id)
        {
            // Si el id es nulo, devuelve un error de solicitud incorrecta.
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Busca el estudiante con el id proporcionado e incluye las relaciones necesarias.
            var estudiante = db.ESTUDIANTE
                .Include(e => e.CARRERA)
                .Include(e => e.LOCALIDAD)
                .Include(e => e.SEXO)
                .Include(e => e.CONDICIONESTUDIANTE)
                .FirstOrDefault(e => e.id_estudiante == id);

            // Si no se encuentra el estudiante, devuelve un error "no encontrado".
            if (estudiante == null)
            {
                return HttpNotFound();
            }

            // Crea un objeto EstudianteCLS para pasar los datos a la vista.
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

            // Devuelve la vista con los detalles del estudiante.
            return View(estudianteCLS);
        }


        // Acción para eliminar un estudiante.
        [HttpPost]
        [ValidateAntiForgeryToken]  // Protección contra ataques CSRF.
        public ActionResult EliminarEstudiante(int id_Estudiante)
        {
            try
            {
                // Busca el estudiante en la base de datos con el id proporcionado.
                ESTUDIANTE estudiante = db.ESTUDIANTE.Find(id_Estudiante);

                // Si el estudiante no se encuentra, devuelve un error "no encontrado".
                if (estudiante == null)
                {
                    return HttpNotFound();
                }

                // Verifica si el estudiante tiene inscripciones asociadas.
                bool tieneMateriasInscritas = db.INSCRIPCIONESTUDIANTEMATERIA.Any(i => i.estudiante_id == id_Estudiante);

                // Si el estudiante tiene materias inscritas, no se puede eliminar.
                if (tieneMateriasInscritas)
                {
                    TempData["ErrorMessage"] = "No se puede eliminar el estudiante";
                    return RedirectToAction("Index");
                }

                // Si no tiene materias inscritas, se procede con la eliminación.
                db.ESTUDIANTE.Remove(estudiante);
                db.SaveChanges();

                // Muestra un mensaje de éxito y redirige a la vista Index.
                TempData["SuccessMessage"] = "El estudiante se eliminó correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error al eliminar el estudiante.";
                return RedirectToAction("Index");
            }
        }


        // Método auxiliar para cargar los datos necesarios para los select list en los formularios.
        private void CargarViewBags()
        {
            // Carga los sexos disponibles en el sistema para el campo de selección de sexo.
            ViewBag.Sexos = new SelectList(db.SEXO.ToList(), "id_sexo", "nombre_sexo");

            // Carga las carreras disponibles en el sistema para el campo de selección de carrera.
            ViewBag.Carreras = new SelectList(db.CARRERA.ToList(), "id_carrera", "nombre_carrera");

            // Carga las localidades disponibles en el sistema para el campo de selección de localidad.
            ViewBag.Localidades = new SelectList(db.LOCALIDAD.ToList(), "id_localidad", "nombre_localidad");

            // Carga las condiciones de estudiante disponibles para el campo de selección de condición.
            ViewBag.CondicionEstudiante = new SelectList(db.CONDICIONESTUDIANTE.ToList(), "id_condicion_estudiante", "nombre_condicion_estudiante");
        }
    }
}
