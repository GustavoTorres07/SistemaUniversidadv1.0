using System.Linq; // Importa el espacio de nombres que contiene LINQ, que es utilizado para consultas a bases de datos y otras colecciones.
using System.Web.Mvc; // Importa el espacio de nombres para las funcionalidades de ASP.NET MVC.
using System; 
using System.IO; 
using SistemaUniversidadv1._0.Filtros; // Importa los filtros personalizados definidos en la aplicación (como un filtro de autorización).
using SistemaUniversidadv1._0.Models; // Importa los modelos de la aplicación, donde se encuentran las clases de entidades y contexto de la base de datos.
using SistemaUniversidadv1._0.Models.ViewModels; // Importa los modelos de vista que son utilizados para enviar datos entre el controlador y las vistas.
using System.Collections.Generic; // Importa el espacio de nombres para colecciones genéricas como List y Dictionary.

namespace SistemaUniversidadv1._0.Controllers // Define el espacio de nombres para el controlador.
{
    [CustomAuthorize("Profesor")] // Aplica un filtro personalizado de autorización para que solo los usuarios con rol "Profesor" puedan acceder a este controlador.
    public class ProfesorController : Controller // Define el controlador "ProfesorController" que hereda de Controller.
    {
        private readonly UniversidadContext db = new UniversidadContext(); // Crea una instancia del contexto de base de datos para acceder a los datos.

        public ActionResult MateriasAsignadas(int? carreraId) // Acción que maneja la vista de las materias asignadas al profesor. Recibe un parámetro opcional carreraId.
        {
            int usuarioId = (int)Session["usuario_id"]; // Obtiene el ID del usuario desde la sesión.
            int rolId = (int)Session["rol_id"]; // Obtiene el ID del rol del usuario desde la sesión.

            if (rolId != 3) // Si el rol no es el de "Profesor" (asumido que "3" es el ID del rol de Profesor)
            {
                return RedirectToAction("Login", "Acceso"); // Redirige al login si el rol no es el adecuado.
            }

            // Consulta las carreras en las que el profesor tiene materias asignadas.
            var carrerasConMaterias = db.PROFESORMATERIA
                .Where(pm => pm.usuario_id == usuarioId) // Filtra las asignaciones de materias por el usuario actual.
                .Select(pm => pm.MATERIA.CICLO.CARRERA) // Obtiene las carreras de las materias asignadas.
                .Distinct() // Elimina duplicados para que no se repitan las carreras.
                .ToList(); // Convierte el resultado en una lista.

            var materias = new List<MateriaInscripcionViewModel>(); // Inicializa una lista vacía para las materias.

            // Si se ha seleccionado una carrera, consulta las materias asignadas a esa carrera.
            if (carreraId.HasValue)
            {
                materias = db.PROFESORMATERIA
                    .Where(pm => pm.usuario_id == usuarioId && pm.MATERIA.CICLO.carrera_id == carreraId.Value) // Filtra las materias por carrera seleccionada.
                    .Select(pm => new MateriaInscripcionViewModel // Crea un modelo de vista para las materias.
                    {
                        MateriaId = pm.materia_id,
                        NombreMateria = pm.MATERIA.nombre_materia,
                        codigo_materia = pm.MATERIA.codigo_materia,
                        nombre_carrera = pm.MATERIA.CICLO.CARRERA.nombre_carrera,
                        carrera_id = pm.MATERIA.CICLO.carrera_id
                    })
                    .ToList(); // Convierte el resultado en una lista.
            }

            // Prepara la lista de carreras para mostrarla en un dropdown en la vista.
            ViewBag.Carreras = new SelectList(carrerasConMaterias, "id_carrera", "nombre_carrera", carreraId);
            ViewBag.CarreraSeleccionada = carreraId.HasValue; // Indica si se ha seleccionado una carrera.

            return View(materias); // Retorna la vista con las materias obtenidas (vacía si no hay filtro).
        }

        public ActionResult EstudiantesPorMateria(int idMateria) // Acción que muestra los estudiantes inscritos en una materia específica.
        {
            int usuarioId = (int)Session["usuario_id"]; // Obtiene el ID del usuario desde la sesión.
            int rolId = (int)Session["rol_id"]; // Obtiene el ID del rol del usuario desde la sesión.

            if (rolId != 3) // Si el rol no es el de "Profesor".
            {
                return RedirectToAction("Login", "Acceso"); // Redirige a una página de acceso no autorizado si el rol no es el adecuado.
            }

            // Verifica si la materia está asignada al profesor actual.
            var materiaAsignada = db.PROFESORMATERIA
                .Any(pm => pm.usuario_id == usuarioId && pm.materia_id == idMateria); // Verifica si existe una asignación de materia para ese profesor.

            if (!materiaAsignada) // Si la materia no está asignada al profesor.
            {
                return HttpNotFound(); // Retorna un error 404 si no se encuentra la materia asignada.
            }

            // Obtiene los estudiantes inscritos en la materia especificada.
            var estudiantes = db.INSCRIPCIONESTUDIANTEMATERIA
                .Where(i => i.materia_id == idMateria) // Filtra las inscripciones de estudiantes para esa materia.
                .Select(i => i.ESTUDIANTE)  // Selecciona la entidad ESTUDIANTE.
                .ToList(); // Convierte el resultado en una lista.

            // Obtiene la información de la materia seleccionada para mostrar en la vista.
            var materiaSeleccionada = db.MATERIA
                .Where(m => m.id_materia == idMateria)
                .Select(m => new MateriaSeleccionadaViewModel
                {
                    MateriaId = m.id_materia,
                    NombreMateria = m.nombre_materia,
                    codigo_materia = m.codigo_materia,
                })
                .FirstOrDefault(); // Obtiene la primera materia que coincida con el ID.

            if (materiaSeleccionada == null) // Si no se encuentra la materia.
            {
                return HttpNotFound(); // Retorna un error 404 si la materia no existe.
            }

            // Asigna el modelo de materia a ViewBag para usar en la vista.
            ViewBag.Materia = materiaSeleccionada;

            return View(estudiantes); // Retorna la vista con la lista de estudiantes para esa materia.
        }
    }
}
