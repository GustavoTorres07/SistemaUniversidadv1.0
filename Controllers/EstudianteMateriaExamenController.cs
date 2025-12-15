using System.Linq;  // Importa el espacio de nombres para LINQ, que se utiliza para consultas a bases de datos
using System.Data.Entity;  // Importa el espacio de nombres para trabajar con Entity Framework
using System.Web.Mvc;  
using SistemaUniversidadv1._0.Models;  // Importa el espacio de nombres donde se encuentran los modelos de la aplicación
using SistemaUniversidadv1._0.Filtros;  // Importa los filtros personalizados, como el de autorización

namespace SistemaUniversidadv1._0.Controllers  // Define el espacio de nombres de los controladores
{
    [CustomAuthorize("Profesor")]  //filtro de autorización para asegurar que solo los Profesores puedan acceder a esta clase
    public class EstudianteMateriaExamenController : Controller  
    {
        private readonly UniversidadContext db = new UniversidadContext();  // Instancia el contexto de base de datos para interactuar con la base de datos

        // Acción GET para calificar un examen
        [HttpGet]
        public ActionResult CalificarExamen(int? estudiante_id, int? materia_id)  // Método para manejar la solicitud GET de calificar un examen
        {
            if (estudiante_id == null || materia_id == null)  // Verifica si los parámetros son nulos
            {
                return new HttpStatusCodeResult(400, "Faltan parámetros.");  // Si faltan parámetros, retorna un error 400
            }

            // Buscar el examen existente para el estudiante y la materia especificados
            var examenExistente = db.ESTUDIANTEMATERIAEXAMEN
                .Include(e => e.ESTUDIANTE)  // Incluye la información del estudiante asociado.
                .FirstOrDefault(e => e.estudiante_id == estudiante_id && e.materia_id == materia_id);  // Busca el examen por los IDs del estudiante y la materia

            if (examenExistente == null)  // Si no existe el examen, lo crea
            {
                // Crear un nuevo objeto examen con valores predeterminados
                examenExistente = new ESTUDIANTEMATERIAEXAMEN
                {
                    estudiante_id = estudiante_id.Value,
                    materia_id = materia_id.Value,
                    condicion_estudiante_materia_id = 8, // Asigna el valor predeterminado de condición
                    examen1 = "", // Inicializa las calificaciones vacías
                    examen2 = "",
                    examen3 = "",
                    examen_final = "",
                    examen_integrador = "",
                    nota_final = ""
                };

                // Guarda el nuevo examen en la base de datos.
                db.ESTUDIANTEMATERIAEXAMEN.Add(examenExistente);
                db.SaveChanges();
            }

            // Obtiene la lista de condiciones para el dropdown en la vista
            ViewBag.Condiciones = db.CONDICIONESTUDIANTEMATERIA
                .Select(c => new SelectListItem  // Crea una lista de elementos seleccionables con valores y textos
                {
                    Value = c.id_condicion_estudiante_materia.ToString(),  // Asigna el ID de la condición
                    Text = c.nombre_condicion  // Asigna el nombre de la condición como texto
                }).ToList();  // Convierte la lista en un objeto lista

            return View(examenExistente);  // Retorna la vista para mostrar o editar el examen del estudiante
        }

        // Acción POST para calificar el examen
        [HttpPost]
        [ValidateAntiForgeryToken]  // Protege contra ataques CSRF (Cross-Site Request Forgery).
        public ActionResult CalificarExamen(ESTUDIANTEMATERIAEXAMEN modelo)  // Método para manejar la solicitud POST de calificar un examen
        {
            if (!ModelState.IsValid)  // Si el modelo no es válido
            {
                // Vuelve a cargar las condiciones para el dropdown en caso de que el modelo no sea válido
                ViewBag.Condiciones = db.CONDICIONESTUDIANTEMATERIA
                    .Select(c => new SelectListItem
                    {
                        Value = c.id_condicion_estudiante_materia.ToString(),
                        Text = c.nombre_condicion
                    }).ToList();

                return View(modelo);  // Devuelve la vista con el modelo actual para corregir cualquier error
            }

            // Busca el examen en la base de datos usando el ID del examen.
            var examenExistente = db.ESTUDIANTEMATERIAEXAMEN
                .FirstOrDefault(e => e.id_estudiante_materia_examen == modelo.id_estudiante_materia_examen);

            if (examenExistente == null)  // Si no se encuentra el examen, muestra un error
            {
                return HttpNotFound();  // Retorna un error 404 si no se encuentra el examen
            }

            // Actualiza los campos del examen con los valores enviados en el modelo
            examenExistente.examen1 = modelo.examen1;
            examenExistente.recuperatorio_examen1 = modelo.recuperatorio_examen1;
            examenExistente.examen2 = modelo.examen2;
            examenExistente.recuperatorio_examen2 = modelo.recuperatorio_examen2;
            examenExistente.examen3 = modelo.examen3;
            examenExistente.recuperatorio_examen3 = modelo.recuperatorio_examen3;
            examenExistente.examen_final = modelo.examen_final;
            examenExistente.examen_integrador = modelo.examen_integrador;
            examenExistente.nota_final = modelo.nota_final;
            examenExistente.condicion_estudiante_materia_id = modelo.condicion_estudiante_materia_id;

            // Guarda los cambios en la base de datos.
            db.SaveChanges();

            // Mensaje de éxito que se almacena temporalmente.
            TempData["SuccessMessage"] = "Calificaciones actualizadas correctamente.";

            // Redirige a la vista que muestra los estudiantes por materia.
            return RedirectToAction("EstudiantesPorMateria", "Profesor", new { idMateria = modelo.materia_id });
        }

        // Acción para ver las calificaciones de un estudiante en una materia
        public ActionResult VerCalificaciones(int? estudiante_id, int? materia_id)  // Método para manejar la solicitud GET de ver calificaciones
        {
            if (estudiante_id == null || materia_id == null)  // Verifica si los parámetros son nulos
            {
                return new HttpStatusCodeResult(400, "Faltan parámetros.");  // Retorna un error 400 si faltan parámetros
            }

            // Busca las calificaciones del estudiante en la materia especificada
            var calificaciones = db.ESTUDIANTEMATERIAEXAMEN
                .Include(e => e.ESTUDIANTE)  // Incluye la información del estudiante
                .Include(e => e.MATERIA)  // Incluye la información de la materia
                .FirstOrDefault(e => e.estudiante_id == estudiante_id && e.materia_id == materia_id);  // Busco por el ID del estudiante y la materia

            if (calificaciones == null)  // Si no existen calificaciones, crea un objeto vacío para mostrar.
            {
                calificaciones = new ESTUDIANTEMATERIAEXAMEN
                {
                    estudiante_id = estudiante_id.Value,
                    materia_id = materia_id.Value,
                    ESTUDIANTE = db.ESTUDIANTE.Find(estudiante_id),
                    MATERIA = db.MATERIA.Find(materia_id),
                    CONDICIONESTUDIANTEMATERIA = db.CONDICIONESTUDIANTEMATERIA.FirstOrDefault(),
                    examen1 = "-",  
                    recuperatorio_examen1 = "-",
                    examen2 = "-",
                    recuperatorio_examen2 = "-",
                    examen3 = "-",
                    recuperatorio_examen3 = "-",
                    examen_final = "-",
                    examen_integrador = "-",
                    nota_final = "-"
                };
            }

            return View(calificaciones);  // Devuelve la vista con las calificaciones encontradas o vacías.
        }

    }
}
