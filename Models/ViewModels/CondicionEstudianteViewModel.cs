using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaUniversidadv1._0.Models.ViewModels
{
    public class CondicionEstudianteViewModel
    {
        public List<CONDICIONESTUDIANTE> Condiciones { get; set; }
        public CONDICIONESTUDIANTE NuevaCondicion { get; set; }
    }
}