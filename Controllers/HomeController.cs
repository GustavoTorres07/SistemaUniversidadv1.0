using SistemaUniversidadv1._0.Filtros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SistemaUniversidadv1._0.Controllers
{
    public class HomeController : Controller
    {

        [CustomAuthorize("Administrador" , "Auxiliar", "Profesor")]

        public ActionResult Index()
        {
            return View();
        }

    }
}