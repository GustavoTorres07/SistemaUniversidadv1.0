using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaUniversidadv1._0.Models
{
    public class CarreraCLS
    {
        public int id_carrera { get; set; }
        public string nombre_carrera { get; set; }
        public bool estado_carrera { get; set; }

        public virtual ICollection<CICLO> CICLO { get; set; }
    }
}