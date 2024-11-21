using SistemaUniversidadv1._0.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaUniversidadv1._0.Models
{
    public class CondicionUsuarioCLS
    {
        public int id_condicion_usuario { get; set; }
        public string nombre_condicion_usuario { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USUARIO> USUARIO { get; set; }
    }
}



