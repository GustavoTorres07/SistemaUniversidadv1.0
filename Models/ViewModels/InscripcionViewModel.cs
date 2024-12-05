using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaUniversidadv1._0.Models.ViewModels
{
    // Modelo para la inscripción del estudiante a las materias
    public class InscripcionViewModel
    {
        public int EstudianteId { get; set; }
        public List<int> MateriaIds { get; set; }  // Lista de IDs de materias seleccionadas
        public List<MATERIA> MateriasDisponibles { get; set; }  // Lista de materias disponibles para seleccionar
        public string codigo_materia { get; set; }

    }

    // Modelo para la materia seleccionada (con la fecha de inscripción)
    public class MateriaSeleccionadaViewModel
    {
        public int MateriaId { get; set; }
        public string NombreMateria { get; set; }
        public bool Seleccionada { get; set; }
        public string codigo_materia { get; set; }
        public DateTime? FechaInscripcion { get; set; }  // Mantener la fecha de inscripción
    }

    // Modelo para listar las inscripciones de un estudiante
    public class ListadoInscripcionesViewModel
    {
        public int EstudianteId { get; set; }
        public string NombreEstudiante { get; set; }
        public List<CicloInscripcionViewModel> Ciclos { get; set; }
        public CARRERA Carrera { get; set; } // Agregar si falta
        public string codigo_materia { get; set; }

        // Nueva propiedad para almacenar las materias seleccionadas
        public List<int> MateriasSeleccionadas { get; set; }

        public ListadoInscripcionesViewModel()
        {
            MateriasSeleccionadas = new List<int>();
        }

    }

    // Modelo para un ciclo de inscripción
    public class CicloInscripcionViewModel
    {
        public string NombreCiclo { get; set; }
        public List<MateriaInscripcionViewModel> Materias { get; set; }
    }

    // Modelo para la materia en la vista de inscripciones
    public class MateriaInscripcionViewModel
    {
        public int MateriaId { get; set; }
        public string NombreMateria { get; set; }
        public string Estado { get; set; }

        public bool EstaSeleccionada { get; set; } // Cambiar el nombre si es necesario

        public string nombre_carrera { get; set; } // Nueva propiedad para la carrera

        public int carrera_id { get; set; } // Nueva propiedad para la carrera


        public string codigo_materia { get; set; } // Asegúrate de que esta propiedad exista
        public DateTime? FechaInscripcion { get; set; }  // Mantener la fecha de inscripción
    }


}
