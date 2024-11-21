using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaUniversidadv1._0.Models
{
    public class EstudianteCLS
    {
        public int id_estudiante { get; set; }
        public int sexo_id { get; set; }
        public int carrera_id { get; set; }
        public int localidad_id { get; set; }
        public int condicion_estudiante_id { get; set; }
        public int numero_legajo { get; set; }
        public string nombre_estudiante { get; set; }
        public string apellido_estudiante { get; set; }
        public int edad_estudiante { get; set; }
        public System.DateTime fecha_nacimiento_estudiante { get; set; }
        public int dni_estudiante { get; set; }
        public bool estado_estudiante { get; set; }
        public System.DateTime fecha_registro_estudiante { get; set; }
        public string celular_estudiante { get; set; }
        public string email_estudiante { get; set; }


        public virtual CARRERA CARRERA { get; set; }
        public virtual CONDICIONESTUDIANTE CONDICIONESTUDIANTE { get; set; }
        public virtual LOCALIDAD LOCALIDAD { get; set; }
        public virtual SEXO SEXO { get; set; }
    }
}