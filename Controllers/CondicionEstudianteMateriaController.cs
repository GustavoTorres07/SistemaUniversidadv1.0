using System; 
using System.Collections.Generic; // Permite trabajar con colecciones genéricas, como List<T>.
using System.Data.Entity.Validation; // Proporciona clases para manejar validaciones específicas de Entity Framework.
using System.Linq; // Facilita el uso de consultas LINQ.
using System.Web.Mvc; 
using SistemaUniversidadv1._0.Filtros; // Importa filtros personalizados del sistema, como CustomAuthorize.
using SistemaUniversidadv1._0.Models; 

namespace SistemaUniversidadv1._0.Controllers
{
    // Aplica un filtro de autorización personalizado, permitiendo el acceso a usuarios con roles "Administrador" y "Auxiliar".
    [CustomAuthorize("Administrador" , "Auxiliar")]
    public class CondicionEstudianteMateriaController : Controller
    {
        // Declara una instancia del contexto de la base de datos para interactuar con las tablas.
        private UniversidadContext db = new UniversidadContext();

        // Método para listar las condiciones de estudiante-materia.
        public ActionResult Index()
        {
            try
            {
                // Obtiene todas las condiciones de la tabla CONDICIONESTUDIANTEMATERIA.
                var condiciones = db.CONDICIONESTUDIANTEMATERIA.ToList();
                // Retorna la vista con la lista de condiciones.
                return View(condiciones);
            }
            catch (Exception ex) // Captura cualquier error que ocurra durante la consulta.
            {
                // Almacena un mensaje de error en TempData para mostrarlo en la vista.
                TempData["ErrorMessage"] = "Error al cargar las condiciones: " + ex.Message;
                // Retorna una vista vacía en caso de error.
                return View(new List<CONDICIONESTUDIANTEMATERIA>());
            }
        }

        // Método GET para mostrar el formulario de creación de una nueva condición.
        [HttpGet]
        public ActionResult CrearCondicionEstudianteMateria()
        {
            // Retorna la vista de creación sin ningún dato adicional.
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearCondicionEstudianteMateria(CONDICIONESTUDIANTEMATERIA condicion)
        {
            // Verificar si la condición ya existe
            var condicionExistente = db.CONDICIONESTUDIANTEMATERIA
                                        .FirstOrDefault(c => c.nombre_condicion == condicion.nombre_condicion);

            if (condicionExistente != null)
            {
                TempData["ErrorMessage"] = "La condición ya existe.";
                return RedirectToAction("Index"); // O redirigir a la vista que deseas
            }

            // Código para crear la nueva condición
            db.CONDICIONESTUDIANTEMATERIA.Add(condicion);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Condición creada exitosamente.";
            return RedirectToAction("Index"); // O redirigir a la vista que deseas
        }



        // Método POST para eliminar una condición existente.
        [HttpPost] // Responde solo a solicitudes POST.
        [ValidateAntiForgeryToken] // Protege contra ataques CSRF.
        public ActionResult EliminarCondicionEstudianteMateria(int? id_condicion_estudiante_materia)
        {
            try
            {
                // Verifica si el ID proporcionado es nulo.
                if (id_condicion_estudiante_materia == null)
                {
                    // Almacena un mensaje de error en TempData.
                    TempData["ErrorMessage"] = "ID de condición no válido.";
                    // Redirige al método Index.
                    return RedirectToAction("Index");
                }

                // Busca la condición en la base de datos por su ID.
                var condicion = db.CONDICIONESTUDIANTEMATERIA.Find(id_condicion_estudiante_materia);
                if (condicion != null) // Verifica si la condición existe.
                {
                    // Elimina la condición del contexto.
                    db.CONDICIONESTUDIANTEMATERIA.Remove(condicion);
                    // Guarda los cambios en la base de datos.
                    db.SaveChanges();
                    // Almacena un mensaje de éxito en TempData.
                    TempData["SuccessMessage"] = "Condición eliminada correctamente.";
                }
                else
                {
                    // Almacena un mensaje de error si no se encuentra la condición.
                    TempData["ErrorMessage"] = "No se encontró la condición.";
                }
            }
            catch (Exception ex) // Captura cualquier error que ocurra durante el proceso.
            {
                // Almacena un mensaje de error en TempData con los detalles de la excepción.
                TempData["ErrorMessage"] = "Error al eliminar la condición: " + ex.Message;
            }

            // Redirige al método Index para mostrar la lista actualizada.
            return RedirectToAction("Index");
        }
    }
}
