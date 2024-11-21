using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SistemaUniversidadv1._0.Models
{
    public class UsuarioCLS
    {
        public int id_usuario { get; set; }

        [Required]
        [Display(Name = "Localidad")]
        public int localidad_id { get; set; }

        [Required]
        [Display(Name = "Sexo")]
        public int sexo_id { get; set; }

        [Required]
        [Display(Name = "Rol")]
        public int rol_id { get; set; }

        [Required]
        [Display(Name = "Condicion")]
        public int condicion_usuario_id { get; set; }

        [Required]
        [Display(Name = "Nombre")]
        public string nombre_usuario { get; set; }

        [Required]
        [Display(Name = "Apellido")]
        public string apellido_usuario { get; set; }

        [Required]
        [Display(Name = "Dni")]
        public int dni_usuario { get; set; }

        [Required]
        [Display(Name = "Fecha de Nacimiento")]
        public System.DateTime fecha_nacimiento_usuario { get; set; }

        [Required]
        [Display(Name = "Edad")]
        public int edad_usuario { get; set; }

        public Nullable<decimal> celular_usuario { get; set; }
        public string email_usuario { get; set; }

        public System.DateTime fecha_registro_usuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [Display(Name = "Nombre de Usuario")]
        public string usuario_usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string clave_usuario { get; set; }

        [Required]
        [Display(Name = "Estado")]
        public bool estado_usuario { get; set; }

        public virtual CONDICIONUSUARIO CONDICIONUSUARIO { get; set; }
        public virtual LOCALIDAD LOCALIDAD { get; set; }
        public virtual ROL ROL { get; set; }
        public virtual SEXO SEXO { get; set; }
    }
}