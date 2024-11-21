using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaUniversidadv1._0.Models
{
    public class SexoCLS
    {
        public int id_sexo { get; set; }
        public string nombre_sexo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USUARIO> USUARIO { get; set; }
    }
}