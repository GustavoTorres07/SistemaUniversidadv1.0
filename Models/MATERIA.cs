//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SistemaUniversidadv1._0.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class MATERIA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MATERIA()
        {
            this.ESTUDIANTEMATERIAEXAMEN = new HashSet<ESTUDIANTEMATERIAEXAMEN>();
            this.INSCRIPCIONESTUDIANTEMATERIA = new HashSet<INSCRIPCIONESTUDIANTEMATERIA>();
            this.PROFESORMATERIA = new HashSet<PROFESORMATERIA>();
        }
    
        public int id_materia { get; set; }
        public int ciclo_id { get; set; }
        public string nombre_materia { get; set; }
        public string codigo_materia { get; set; }
        public string correlativa_materia { get; set; }
    
        public virtual CICLO CICLO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ESTUDIANTEMATERIAEXAMEN> ESTUDIANTEMATERIAEXAMEN { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<INSCRIPCIONESTUDIANTEMATERIA> INSCRIPCIONESTUDIANTEMATERIA { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PROFESORMATERIA> PROFESORMATERIA { get; set; }
    }
}
