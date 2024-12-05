using SistemaUniversidadv1._0.Filtros; // Importa filtros personalizados, como el de autorización.
using SistemaUniversidadv1._0.Models; // Importa el espacio de nombres con los modelos de datos.
using System; // Espacio de nombres para manejar excepciones y funciones generales.
using System.Linq; // Permite trabajar con consultas LINQ.
using System.Web.Mvc; // Espacio de nombres para controladores y vistas en ASP.NET MVC.

namespace SistemaUniversidadv1._0.Controllers
{
    [CustomAuthorize("Administrador", "Auxiliar")] // Permite el acceso a usuarios con roles de Administrador o Auxiliar.
    public class CarreraController : Controller
    {
        private UniversidadContext db = new UniversidadContext(); // Instancia del contexto para acceder a la base de datos.

        [HttpGet] // Método GET para mostrar datos.
        public ActionResult Index()
        {
            try
            {
                var carreras = db.CARRERA.ToList(); // Obtiene todas las carreras de la tabla CARRERA.
                return View(carreras); // Envía las carreras a la vista para mostrarlas.
            }
            catch (Exception ex) // Captura errores que puedan ocurrir.
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar el listado de carreras. " + ex.Message; // Muestra un mensaje de error.
                return View(); // Retorna la vista vacía si hay error.
            }
        }

        [HttpGet] // Método GET para mostrar el formulario.
        public ActionResult CrearCarrera()
        {
            try
            {
                return View(); // Retorna la vista del formulario de creación.
            }
            catch (Exception ex) // Captura errores que puedan ocurrir.
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar el formulario de creación. " + ex.Message; // Muestra un mensaje de error.
                return View(); // Retorna la vista vacía si hay error.
            }
        }

        [HttpPost] // Método POST para procesar el formulario.
        [ValidateAntiForgeryToken] // Previene ataques CSRF.
        public ActionResult CrearCarrera(CarreraCLS carreraCLS) // Recibe el modelo con los datos ingresados.
        {
            try
            {
                if (ModelState.IsValid) // Verifica si los datos cumplen con las validaciones del modelo.
                {
                    if (db.CARRERA.Any(c => c.nombre_carrera == carreraCLS.nombre_carrera)) // Comprueba si ya existe una carrera con el mismo nombre.
                    {
                        TempData["Error"] = "Ya existe una Carrera con el mismo nombre."; // Mensaje de error.
                        return RedirectToAction("Index"); // Redirige al índice.
                    }

                    var carrera = new CARRERA // Crea un nuevo objeto de tipo CARRERA.
                    {
                        nombre_carrera = carreraCLS.nombre_carrera, // Asigna el nombre ingresado.
                        estado_carrera = carreraCLS.estado_carrera // Asigna el estado ingresado.
                    };

                    db.CARRERA.Add(carrera); // Agrega la nueva carrera a la base de datos.
                    db.SaveChanges(); // Guarda los cambios.

                    TempData["Success"] = "Carrera Guardada exitosamente."; // Mensaje de éxito.
                    return RedirectToAction("Index"); // Redirige al índice.
                }

                TempData["Error"] = "Por favor, verifique los datos ingresados."; // Mensaje de error si el modelo no es válido.
                return RedirectToAction("Index"); // Redirige al índice.
            }
            catch (Exception ex) // Captura errores que puedan ocurrir.
            {
                TempData["Error"] = "Error al crear la Carrera: " + ex.Message; // Mensaje de error.
                return RedirectToAction("Index"); // Redirige al índice.
            }
        }

        [HttpGet] // Método GET para mostrar el formulario de edición.
        public ActionResult EditarCarrera(int id_carrera) // Recibe el ID de la carrera a editar.
        {
            try
            {
                var carrera = db.CARRERA.Find(id_carrera); // Busca la carrera en la base de datos.
                if (carrera == null) // Verifica si no se encontró.
                {
                    return HttpNotFound(); // Retorna error 404 si no existe.
                }

                var carreraCLS = new CarreraCLS // Crea un objeto del modelo de vista con los datos de la carrera.
                {
                    id_carrera = carrera.id_carrera, // Asigna el ID.
                    nombre_carrera = carrera.nombre_carrera, // Asigna el nombre.
                    estado_carrera = carrera.estado_carrera // Asigna el estado.
                };

                return View(carreraCLS); // Retorna la vista con los datos.
            }
            catch (Exception ex) // Captura errores que puedan ocurrir.
            {
                ViewBag.ErrorMessage = "Error al cargar la carrera para editar. " + ex.Message; // Mensaje de error.
                return View(); // Retorna la vista vacía si hay error.
            }
        }

        [HttpPost] // Método POST para guardar los cambios.
        [ValidateAntiForgeryToken] // Previene ataques CSRF.
        public ActionResult EditarCarrera(CarreraCLS carreraCLS) // Recibe el modelo con los datos editados.
        {
            try
            {
                if (ModelState.IsValid) // Verifica si los datos son válidos.
                {
                    var carrera = db.CARRERA.Find(carreraCLS.id_carrera); // Busca la carrera en la base de datos.
                    if (carrera == null) // Verifica si no se encontró.
                    {
                        return HttpNotFound(); // Retorna error 404 si no existe.
                    }

                    if (db.CARRERA.Any(c => c.nombre_carrera == carreraCLS.nombre_carrera && c.id_carrera != carreraCLS.id_carrera)) // Verifica si ya existe otra carrera con el mismo nombre.
                    {
                        TempData["Error"] = "Ya Existe una Carrera con el Mismo Nombre."; // Mensaje de error.
                        return RedirectToAction("Index"); // Redirige al índice.
                    }

                    carrera.nombre_carrera = carreraCLS.nombre_carrera; // Actualiza el nombre.
                    carrera.estado_carrera = carreraCLS.estado_carrera; // Actualiza el estado.

                    db.SaveChanges(); // Guarda los cambios.

                    TempData["Success"] = "La Carrera se Actualizó Exitosamente."; // Mensaje de éxito.
                    return RedirectToAction("Index"); // Redirige al índice.
                }

                TempData["Error"] = "Por Favor, Verifique Los Datos Ingresados."; // Mensaje de error si el modelo no es válido.
                return RedirectToAction("Index"); // Redirige al índice.
            }
            catch (Exception ex) // Captura errores que puedan ocurrir.
            {
                TempData["Error"] = "Error al Editar la Carrera: " + ex.Message; // Mensaje de error.
                return RedirectToAction("Index"); // Redirige al index.
            }
        }

        public ActionResult ActivarCarrera(int id) // Activa una carrera por su id.
        {
            using (var db = new UniversidadContext()) // Crea una nueva instancia del contexto.
            {
                var carrera = db.CARRERA.Find(id); // Busca la carrera.
                if (carrera == null) // Verifica si no se encontró.
                {
                    return HttpNotFound(); // Retorna error 404 si no existe.
                }

                carrera.estado_carrera = true; // Cambia el estado a activo.
                db.SaveChanges(); // Guarda los cambios.
            }

            return RedirectToAction("Index"); // Redirige al index.
        }

        public ActionResult DesactivarCarrera(int id) // Desactiva una carrera por su id.
        {
            using (var db = new UniversidadContext()) // Crea una nueva instancia del contexto.
            {
                var carrera = db.CARRERA.Find(id); // Busca la carrera.
                if (carrera == null) // Verifica si no se encontró.
                {
                    return HttpNotFound(); // Retorna error 404 si no existe.
                }

                carrera.estado_carrera = false; // Cambia el estado a inactivo.
                db.SaveChanges(); // Guarda los cambios.
            }

            return RedirectToAction("Index"); // Redirige al índice.
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCarrera(int id)        // Método que recibe el ID de la carrera que se desea eliminar y devuelve una acción de resultado.
        {
            try
            // Bloque `try` para manejar posibles excepciones durante el proceso de eliminación.
            {
                var carrera = db.CARRERA.Find(id);
                // Busca en la base de datos la carrera correspondiente al ID proporcionado.

                if (carrera == null)
                // Verifica si la carrera no fue encontrada en la base de datos.
                {
                    return HttpNotFound();
                    // Devuelve un error 404 indicando que la carrera no existe.
                }

                // Verificar si hay estudiantes inscritos en esta carrera
                bool tieneEstudiantes = db.ESTUDIANTE.Any(e => e.carrera_id == id);
                // Comprueba si existe al menos un estudiante asociado a esta carrera.

                if (tieneEstudiantes)
                // Si hay estudiantes inscritos en esta carrera.
                {
                    TempData["Error"] = "No se puede eliminar la carrera porque tiene estudiantes inscritos.";
                    // Guarda un mensaje de error en TempData para mostrarlo en la siguiente solicitud.

                    return RedirectToAction("Index");
                    // Redirige al usuario a la acción `Index`, sin eliminar la carrera.
                }

                db.CARRERA.Remove(carrera);
                // Marca la carrera como eliminada en el contexto de la base de datos.

                db.SaveChanges();
                // Guarda los cambios en la base de datos, eliminando permanentemente la carrera.

                TempData["Success"] = "La Carrera ha sido Eliminada Exitosamente.";
                // Guarda un mensaje de éxito en TempData para mostrarlo en la vista.
            }
            catch (Exception ex)
            // Bloque `catch` para manejar cualquier excepción que ocurra durante la eliminación.
            {
                TempData["Error"] = "Error al Eliminar la Carrera: " + ex.Message;
                // Guarda un mensaje de error detallado en TempData.
            }

            return RedirectToAction("Index");
            // Redirige al usuario a la acción `Index`, independientemente del resultado del proceso.
        }


        [HttpGet]
        public JsonResult VerificarEstudiantesEnCarrera(int id_carrera)
        // Define un método público que retorna un resultado JSON. 
        // Recibe como parámetro el ID de la carrera que se desea verificar.
        {
            bool tieneEstudiantes = db.ESTUDIANTE.Any(e => e.carrera_id == id_carrera);
            // Consulta la base de datos para verificar si existe algún estudiante asociado
            // a la carrera con el ID proporcionado. Devuelve `true` si hay estudiantes, 
            // de lo contrario, devuelve `false`.

            return Json(tieneEstudiantes, JsonRequestBehavior.AllowGet);
            // Retorna el resultado de la consulta como un objeto JSON.
            // El parámetro `JsonRequestBehavior.AllowGet` permite que se realicen solicitudes GET
            // para obtener esta respuesta JSON.
        }

    }
}
