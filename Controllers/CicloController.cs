using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SistemaUniversidadv1._0.Filtros;
using SistemaUniversidadv1._0.Models;

namespace SistemaUniversidadv1._0.Controllers
{
    [CustomAuthorize("Administrador", "Auxiliar")]
    public class CicloController : Controller
    {
        private UniversidadContext db = new UniversidadContext();

        public ActionResult Index(int? carrera_id)
        // Esta es la acción del controlador que recibe un parámetro 'carrera_id', que representa el ID de la carrera seleccionada.

        {
            // Cargar la lista de carreras para el dropdown
            ViewBag.Carreras = db.CARRERA.Where(c => c.estado_carrera).ToList();
            // Se consulta la base de datos para obtener todas las carreras que están activas (estado_carrera es verdadero) y se asignan a la propiedad 'Carreras' del ViewBag para ser utilizadas en el dropdown de la vista.

            // Si se selecciona una carrera, filtrar ciclos por esa carrera
            if (carrera_id != null)
            // Si 'carrera_id' no es nulo filtra los ciclos asociados a esa carrera.

            {
                var carreraSeleccionada = db.CARRERA.Find(carrera_id);
                // Se busca la carrera seleccionada en la base de datos utilizando el 'carrera_id'.

                ViewBag.CarreraSeleccionada = carreraSeleccionada?.nombre_carrera;
                // Si se encuentra la carrera, se asigna el nombre de la carrera seleccionada al ViewBag para mostrarlo en la vista.

                var ciclos = db.CICLO.Where(c => c.carrera_id == carrera_id).ToList();
                // Se consulta la base de datos para obtener todos los ciclos que están asociados a la carrera seleccionada.

                if (!ciclos.Any())
                // Si no se encuentran ciclos asociados a la carrera seleccionada,
                {
                    ViewBag.Mensaje = "No hay ciclos para la carrera seleccionada.";
                    // Se asigna un mensaje al ViewBag que indica que no hay ciclos disponibles para esa carrera.
                }

                return View(ciclos);
                // Se pasa la lista de ciclos a la vista. Si hay ciclos disponibles, se mostrarán en la vista.
            }
            else
            // Si no se selecciona una carrera (carrera_id es nulo), entonces se ejecuta el siguiente bloque.

            {
                // Cuando no hay carrera seleccionada, enviar un mensaje
                ViewBag.Mensaje = "Por favor seleccione una carrera para ver sus ciclos.";
                // Se asigna un mensaje al ViewBag indicando que el usuario debe seleccionar una carrera.

                return View(new List<CICLO>());
                // Se retorna una vista vacía, mostrando una lista vacía de ciclos, ya que no se seleccionó ninguna carrera.
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken] 
        public ActionResult CrearCiclo(int carrera_id, string nombre_ciclo)
        // La acción recibe dos parámetros: 'carrera_id' (ID de la carrera en la que se va a crear el ciclo) y 'nombre_ciclo' (nombre del ciclo a crear).

        {
            // Verificar si ya existe un ciclo con el mismo nombre en la carrera seleccionada
            if (db.CICLO.Any(c => c.nombre_ciclo == nombre_ciclo))
            // Verifica si ya existe un ciclo con el mismo 'nombre_ciclo' en la base de datos. Si existe, se ejecuta el bloque siguiente.

            {
                TempData["Error"] = "El ciclo ya existe en esta carrera.";
                // Si el ciclo ya existe, asigna un mensaje de error a TempData para mostrarlo en la vista.

                return RedirectToAction("Index", new { carrera_id });
                // Redirige al usuario a la acción "Index" pasando el 'carrera_id', para que vea la lista de ciclos de la carrera seleccionada con el mensaje de error.
            }

            // Crear el nuevo ciclo
            var nuevoCiclo = new CICLO
            // Crea un nuevo objeto de tipo CICLO.

            {
                carrera_id = carrera_id,
                // Asigna el 'carrera_id' al nuevo ciclo.

                nombre_ciclo = nombre_ciclo
                // Asigna el 'nombre_ciclo' al nuevo ciclo.
            };

            db.CICLO.Add(nuevoCiclo);
            // Añade el nuevo ciclo a la base de datos.

            db.SaveChanges();
            // Guarda los cambios en la base de datos (es decir, guarda el nuevo ciclo).

            TempData["Success"] = "Ciclo creado exitosamente.";
            // Asigna un mensaje de éxito a TempData para mostrarlo en la vista.

            return RedirectToAction("Index", new { carrera_id });
            // Redirige a la acción "Index" pasando el 'carrera_id' para mostrar la lista actualizada de ciclos con el mensaje de éxito.
        }


        [HttpPost]  
        [ValidateAntiForgeryToken] 
        public ActionResult EliminarCiclo(int id_ciclo)
        // La acción recibe el parámetro 'id_ciclo', que es el ID del ciclo a eliminar.

        {
            var ciclo = db.CICLO.Find(id_ciclo);
            // Busca el ciclo en la base de datos usando el 'id_ciclo' recibido como parámetro.

            // Verificar si el ciclo tiene materias asociadas
            if (db.MATERIA.Any(m => m.ciclo_id == id_ciclo))
            // Verifica si hay alguna materia asociada al ciclo

            {
                TempData["Error"] = "No se puede eliminar el ciclo porque tiene materias asociadas.";
                // Si el ciclo tiene materias asociadas, asigna un mensaje de error a TempData para mostrarlo en la vista.

                return RedirectToAction("Index", new { carrera_id = ciclo.carrera_id });
                // Redirige al usuario a la acción "Index", pasando el 'carrera_id' del ciclo, para que vea la lista de ciclos de la carrera con el mensaje de error.
            }

            // Eliminar el ciclo
            db.CICLO.Remove(ciclo);
            // Elimina el ciclo de la base de datos.

            db.SaveChanges();
            // Guarda los cambios en la base de datos (eliminando el ciclo).

            TempData["Success"] = "Ciclo eliminado exitosamente.";
            // Asigna un mensaje de éxito a TempData para mostrarlo en la vista.

            return RedirectToAction("Index", new { carrera_id = ciclo.carrera_id });
            // Redirige al usuario a la acción "Index", pasando el 'carrera_id' del ciclo, para que vea la lista actualizada de ciclos con el mensaje de éxito.
        }

    }
}
