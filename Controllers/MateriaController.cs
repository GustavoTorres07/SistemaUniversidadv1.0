using iTextSharp.text.pdf;  // Importa las clases necesarias para trabajar con archivos PDF (iTextSharp).
using iTextSharp.text;      // Importa las clases necesarias para la creación de contenido dentro de los PDF.
using SistemaUniversidadv1._0.Models;  
using System;             
using System.Collections.Generic; 
using System.IO;          
using System.Linq;         
using System.Web.Mvc;    
using SistemaUniversidadv1._0.Filtros;  // Importa los filtros personalizados definidos en el sistema (como el filtro de autorización).

// El controlador se aplica a usuarios con los roles "Administrador", "Auxiliar" o "Profesor".
[CustomAuthorize("Administrador", "Auxiliar", "Profesor")]
public class MateriaController : Controller
{
    // Se crea un objeto de contexto para interactuar con la base de datos.
    private UniversidadContext db = new UniversidadContext();

    // Acción para cargar la vista principal con el listado de carreras activas.
    public ActionResult Index()
    {
        // Se obtienen las carreras activas de la base de datos.
        var carreras = db.CARRERA.Where(c => c.estado_carrera).ToList();

        // Si existen carreras activas, se pasan a la vista, de lo contrario, se pasa una lista vacía.
        ViewBag.CarrerasActivas = carreras.Count > 0 ? carreras : new List<CARRERA>();

        // Retorna la vista.
        return View();
    }

    // Acción para cargar los ciclos de una carrera específica.
    public JsonResult CargarCiclosPorCarrera(int carreraId)
    {
        // Se obtienen los ciclos correspondientes a la carrera indicada, devolviendo solo el ID y el nombre del ciclo.
        var ciclos = db.CICLO
            .Where(c => c.carrera_id == carreraId)
            .Select(c => new { c.id_ciclo, c.nombre_ciclo })
            .ToList();

        // Se retorna el resultado en formato JSON.
        return Json(ciclos, JsonRequestBehavior.AllowGet);
    }

    // Acción para obtener las materias asociadas a una carrera.
    public JsonResult ObtenerMateriasPorCarrera(int carreraId)
    {
        // Se realiza una consulta que combina las tablas MATERIA y CICLO para obtener las materias de la carrera indicada.
        var materias = (from m in db.MATERIA
                        join c in db.CICLO on m.ciclo_id equals c.id_ciclo
                        where c.carrera_id == carreraId
                        select new
                        {
                            m.id_materia,
                            m.nombre_materia,
                            m.codigo_materia,
                            m.correlativa_materia,
                            c.nombre_ciclo
                        }).ToList();

        // Se retorna el resultado en formato JSON.
        return Json(materias, JsonRequestBehavior.AllowGet);
    }

    // Acción para crear una nueva materia.
    [HttpPost]  // Indica que esta acción maneja solicitudes POST.
    [ValidateAntiForgeryToken]  // Protege contra ataques CSRF (Cross-Site Request Forgery).
    public JsonResult CrearMateria(MateriaCLS materia)
    {
        try
        {
            // Si el modelo es válido, se crea una nueva materia en la base de datos.
            if (ModelState.IsValid)
            {
                var nuevaMateria = new MATERIA
                {
                    ciclo_id = materia.ciclo_id,
                    nombre_materia = materia.nombre_materia,
                    codigo_materia = materia.codigo_materia,
                    correlativa_materia = materia.correlativa_materia
                };

                // Se agrega la nueva materia a la base de datos y se guardan los cambios.
                db.MATERIA.Add(nuevaMateria);
                db.SaveChanges();

                // Se retorna una respuesta JSON indicando que la creación fue exitosa.
                return Json(new { success = true, message = "Materia creada correctamente." });
            }

            // Si el modelo no es válido, se recopilan los errores del ModelState.
            var errores = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();

            // Se retorna una respuesta JSON con los errores encontrados.
            return Json(new
            {
                success = false,
                message = "Datos inválidos.",
                errores = errores
            });
        }
        catch (Exception)
        {
            // Se retorna una respuesta JSON con un mensaje genérico de error.
            return Json(new
            {
                success = false,
                message = "Ocurrió un error al crear la materia."
            });
        }
    }


    // Acción para eliminar una materia.
    [HttpPost]  // Indica que esta acción maneja solicitudes POST.
    [ValidateAntiForgeryToken]  // Protege contra ataques CSRF.
    public ActionResult EliminarMateria(int id_materia)
    {
        using (var transaction = db.Database.BeginTransaction())  // Inicia una transacción para asegurar que la eliminación sea atómica.
        {
            try
            {
                // Se busca la materia por su ID.
                var materia = db.MATERIA.Find(id_materia);
                if (materia == null)
                {
                    // Si no se encuentra la materia, se retorna un mensaje de error.
                    return Json(new { success = false, message = "La materia no existe." });
                }

                // Se elimina la materia de la base de datos.
                db.MATERIA.Remove(materia);
                db.SaveChanges();

                // Si todo es exitoso, se confirma la transacción.
                transaction.Commit();

                // Se retorna un mensaje indicando que la materia fue eliminada correctamente.
                return Json(new { success = true, message = "Materia eliminada correctamente" });
            }
            catch (Exception ex)
            {
                // Si ocurre un error, se revierte la transacción.
                transaction.Rollback();

                // Se retorna un mensaje de error.
                return Json(new { success = false, message = "Error al eliminar la materia: " + ex.Message });
            }
        }
    }

    public ActionResult GenerarPDFMaterias(int carreraId)
    {
        var carrera = db.CARRERA.Find(carreraId);
        var materias = db.MATERIA
                        .Where(m => m.CICLO.carrera_id == carreraId)
                        .OrderBy(m => m.CICLO.nombre_ciclo)
                        .ThenBy(m => m.codigo_materia)
                        .ToList();

        // Configuración del documento PDF
        MemoryStream memoryStream = new MemoryStream();
        Document document = new Document(PageSize.A4, 20, 20, 20, 20);
        PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
        document.Open();

        // Agregar título (reducido de 16 a 14)
        var titulo = new Paragraph($"PLAN DE ESTUDIO - {carrera.nombre_carrera}", new Font(Font.FontFamily.HELVETICA, 15, Font.BOLD));
        titulo.Alignment = Element.ALIGN_CENTER;
        document.Add(titulo);

        // Espacio entre título y contenido
        document.Add(new Paragraph("\n"));

        // Crear una única tabla para todas las materias
        PdfPTable table = new PdfPTable(3) { WidthPercentage = 100 };
        table.SetWidths(new float[] { 20f, 40f, 40f });

        // Encabezados de tabla (reducido de 10 a 8)
        var headerFont = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);
        PdfPCell headerCell1 = new PdfPCell(new Phrase("Código", headerFont));
        PdfPCell headerCell2 = new PdfPCell(new Phrase("Correlativas", headerFont));
        PdfPCell headerCell3 = new PdfPCell(new Phrase("Materia", headerFont));

        // Centrar y dar formato a las celdas de encabezado
        headerCell1.HorizontalAlignment = Element.ALIGN_CENTER;
        headerCell2.HorizontalAlignment = Element.ALIGN_CENTER;
        headerCell3.HorizontalAlignment = Element.ALIGN_CENTER;
        headerCell1.VerticalAlignment = Element.ALIGN_MIDDLE;
        headerCell2.VerticalAlignment = Element.ALIGN_MIDDLE;
        headerCell3.VerticalAlignment = Element.ALIGN_MIDDLE;
        headerCell1.BackgroundColor = BaseColor.LIGHT_GRAY;
        headerCell2.BackgroundColor = BaseColor.LIGHT_GRAY;
        headerCell3.BackgroundColor = BaseColor.LIGHT_GRAY;
        headerCell1.Padding = 4;
        headerCell2.Padding = 4;
        headerCell3.Padding = 4;

        table.AddCell(headerCell1);
        table.AddCell(headerCell2);
        table.AddCell(headerCell3);

        // Agrupar materias por ciclos
        var materiasPorCiclo = materias.GroupBy(m => m.CICLO.nombre_ciclo);
        var contentFont = new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL); // Reducido de 9 a 7
        var cicloFont = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);    // Reducido de 10 a 8

        foreach (var ciclo in materiasPorCiclo)
        {
            // Agregar fila de ciclo
            PdfPCell cicloCell = new PdfPCell(new Phrase(ciclo.Key, cicloFont));
            cicloCell.Colspan = 3;
            cicloCell.BackgroundColor = new BaseColor(174, 214, 241);
            cicloCell.HorizontalAlignment = Element.ALIGN_CENTER;
            cicloCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cicloCell.Padding = 4;
            table.AddCell(cicloCell);

            // Añadir materias del ciclo
            foreach (var materia in ciclo)
            {
                // Crear celdas con el contenido
                PdfPCell celdaCodigo = new PdfPCell(new Phrase(materia.codigo_materia, contentFont));
                PdfPCell celdaCorrelativas = new PdfPCell(new Phrase(materia.correlativa_materia, contentFont));
                PdfPCell celdaNombre = new PdfPCell(new Phrase(materia.nombre_materia, contentFont));

                // Configurar alineación y padding para todas las celdas
                PdfPCell[] celdas = { celdaCodigo, celdaCorrelativas, celdaNombre };
                foreach (var celda in celdas)
                {
                    celda.HorizontalAlignment = Element.ALIGN_CENTER;
                    celda.VerticalAlignment = Element.ALIGN_MIDDLE;
                    celda.PaddingTop = 3;    // Reducido de 5 a 3
                    celda.PaddingBottom = 3; // Reducido de 5 a 3
                    celda.PaddingLeft = 3;   // Reducido de 4 a 3
                    celda.PaddingRight = 3;  // Reducido de 4 a 3
                    celda.MinimumHeight = 20f; // Reducido de 25f a 20f
                }

                table.AddCell(celdaCodigo);
                table.AddCell(celdaCorrelativas);
                table.AddCell(celdaNombre);
            }
        }

        document.Add(table);
        document.Close();
        byte[] pdfBytes = memoryStream.ToArray();

        // Descargar PDF
        return File(pdfBytes, "application/pdf", $"PLAN DE ESTUDIO - {carrera.nombre_carrera}.pdf");
    }
}
