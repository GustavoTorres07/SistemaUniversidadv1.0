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
    
    public partial class USUARIO
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public USUARIO()
        {
            this.PROFESORMATERIA = new HashSet<PROFESORMATERIA>();
        }
    
        public int id_usuario { get; set; }
        public int localidad_id { get; set; }
        public int sexo_id { get; set; }
        public int rol_id { get; set; }
        public int condicion_usuario_id { get; set; }
        public string nombre_usuario { get; set; }
        public string apellido_usuario { get; set; }
        public int dni_usuario { get; set; }
        public System.DateTime fecha_nacimiento_usuario { get; set; }
        public int edad_usuario { get; set; }
        public Nullable<decimal> celular_usuario { get; set; }
        public string email_usuario { get; set; }
        public System.DateTime fecha_registro_usuario { get; set; }
        public string usuario_usuario { get; set; }
        public string clave_usuario { get; set; }
        public bool estado_usuario { get; set; }
    
        public virtual CONDICIONUSUARIO CONDICIONUSUARIO { get; set; }
        public virtual LOCALIDAD LOCALIDAD { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PROFESORMATERIA> PROFESORMATERIA { get; set; }
        public virtual ROL ROL { get; set; }
        public virtual SEXO SEXO { get; set; }
    }
}
