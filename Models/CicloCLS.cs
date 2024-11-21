using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SistemaUniversidadv1._0.Models
{
    public class CicloCLS
    {
        public int id_ciclo { get; set; }

        [Required(ErrorMessage = "La carrera es requerida")]
        [Display(Name = "Carrera")]
        public int carrera_id { get; set; }

        [Required(ErrorMessage = "El nombre del ciclo es requerido")]
        [Display(Name = "Nombre del Ciclo")]
        public string nombre_ciclo { get; set; }
        public string nombre_carrera { get; set; }

        public virtual CARRERA CARRERA { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MATERIA> MATERIA { get; set; }
    }
}