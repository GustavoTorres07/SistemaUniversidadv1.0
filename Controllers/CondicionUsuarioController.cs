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

    public class CondicionUsuarioController : Controller
    {
        private UniversidadContext db = new UniversidadContext(); 

        
        // GET: CondicionUsuario
        public ActionResult Index()
        {
            var condicionusuario = db.CONDICIONUSUARIO.ToList();
            return View(condicionusuario); 
        }

        
        public ActionResult CrearCondicionUsuario()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un Error al Cargar el Formulario de Creación. " + ex.Message;
                return View(); // Retorna la vista con un mensaje de error
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearCondicionUsuario(CondicionUsuarioCLS condicionUsuarioCLS)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar duplicados para nombre de localidad
                    if (db.CONDICIONUSUARIO.Any(cu => cu.nombre_condicion_usuario == condicionUsuarioCLS.nombre_condicion_usuario))
                    {
                        // En lugar de retornar una vista, redirigimos al Index con un mensaje de error
                        TempData["Error"] = "Ya Existe una Condicion con el Mismo Nombre.";
                        return RedirectToAction("Index");
                    }

                    var condicionusuario = new CONDICIONUSUARIO
                    {
                        nombre_condicion_usuario = condicionUsuarioCLS.nombre_condicion_usuario,
                    };

                    db.CONDICIONUSUARIO.Add(condicionusuario);
                    db.SaveChanges();

                    // Mensaje de éxito
                    TempData["Success"] = "La Condicion se Guardo Exitosamente.";
                    return RedirectToAction("Index");
                }

                // Si el modelo no es válido
                TempData["Error"] = "Por Favor, Verifique Los Datos Ingresados.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al Crear la Condicion: " + ex.Message;
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCondicionUsuario(int id_condicion_usuario)
        {
            try
            {
                // Obtener la localidad a eliminar de la base de datos
                CONDICIONUSUARIO condicionusuario = db.CONDICIONUSUARIO.Find(id_condicion_usuario);

                // Si la localidad no existe, retornar un error 404
                if (condicionusuario == null)
                {
                    return HttpNotFound();
                }

                // Eliminar la localidad de la base de datos
                db.CONDICIONUSUARIO.Remove(condicionusuario);

                // Guardar los cambios en la base de datos
                db.SaveChanges();

                // Almacenar un mensaje de éxito en TempData
                TempData["Success"] = "la condicion se eliminó correctamente.";

                // Redirigir al índice después de eliminar la localidad exitosamente
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // Almacenar el mensaje de error en TempData
                TempData["Error"] = "Error al eliminar la condición. Puede que esté en uso o haya ocurrido otro problema.";

                // Redireccionar al índice
                return RedirectToAction("Index");
            }
        }

    }
}