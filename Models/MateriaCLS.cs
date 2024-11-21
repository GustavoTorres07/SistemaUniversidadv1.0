using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaUniversidadv1._0.Models
{
    public class MateriaCLS
    {
        public int id_materia { get; set; }
        public int ciclo_id { get; set; }
        public string nombre_materia { get; set; }
        public string codigo_materia { get; set; }
        public string correlativa_materia { get; set; }

        public virtual CICLO CICLO { get; set; }
    }
}