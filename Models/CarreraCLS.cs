using System; // Nos da acceso a cosas básicas como tipos de datos (int, string) y excepciones.
using System.Collections.Generic; // Se usa para trabajar con colecciones como listas o conjuntos.
using System.ComponentModel.DataAnnotations; // Nos deja usar atributos para validar datos, como [Required].
using System.Linq; // Útil para hacer consultas sobre colecciones (aunque aquí no se usa explícitamente).
using System.Web; // Relacionado con funcionalidades web de ASP.NET (no se usa aquí).

namespace SistemaUniversidadv1._0.Models // Define el "espacio de nombres" donde está esta clase, o sea, su categoría.
{
    public class CarreraCLS // Esto define la clase "CarreraCLS", que representa una carrera en el sistema.
    {
        public int id_carrera { get; set; }
        // Identificador único para cada carrera. Es como el id en la base de datos.

        [Required(ErrorMessage = "El nombre de la carrera es obligatorio.")]
        // Dice que el campo "nombre_carrera" no puede estar vacío. Si lo está, muestra el mensaje personalizado.

        [StringLength(250, ErrorMessage = "El nombre no puede exceder los 250 caracteres.")]
        // Limita la longitud del nombre de la carrera a 250 caracteres.

        public string nombre_carrera { get; set; }
        // Guarda el nombre de la carrera.

        public bool estado_carrera { get; set; }
        // Indica si la carrera está activa (true) o inactiva (false).

        public virtual ICollection<CICLO> CICLO { get; set; }
        // Es una lista (colección) de ciclos asociados a esta carrera.
        // La palabra "virtual" permite que Entity Framework cargue los datos de los ciclos automáticamente si se necesitan.
    }
}
