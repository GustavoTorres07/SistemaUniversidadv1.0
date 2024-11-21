using iTextSharp.text.pdf;
using iTextSharp.text;
using SistemaUniversidadv1._0.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace SistemaUniversidadv1._0.Controllers
{
    public class MateriaController : Controller
    {
        private UniversidadContext db = new UniversidadContext();

        public ActionResult Index()
        {
            var carreras = db.CARRERA.Where(c => c.estado_carrera).ToList();
            ViewBag.CarrerasActivas = carreras.Count > 0 ? carreras : new List<CARRERA>();
            return View();
        }

        public JsonResult CargarCiclosPorCarrera(int carreraId)
        {
            var ciclos = db.CICLO
                .Where(c => c.carrera_id == carreraId)
                .Select(c => new { c.id_ciclo, c.nombre_ciclo })
                .ToList();
            return Json(ciclos, JsonRequestBehavior.AllowGet);
        }

        // Nueva acción para obtener materias por carrera
        public JsonResult ObtenerMateriasPorCarrera(int carreraId)
        {
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

            return Json(materias, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CrearMateria(MateriaCLS materia)
        {
            try
            {
                // Add logging to track the incoming data
                System.Diagnostics.Debug.WriteLine($"Recibiendo datos: Ciclo={materia.ciclo_id}, Nombre={materia.nombre_materia}, Código={materia.codigo_materia}");

                if (ModelState.IsValid)
                {
                    var nuevaMateria = new MATERIA
                    {
                        ciclo_id = materia.ciclo_id,
                        nombre_materia = materia.nombre_materia,
                        codigo_materia = materia.codigo_materia,
                        correlativa_materia = materia.correlativa_materia
                    };

                    db.MATERIA.Add(nuevaMateria);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Materia creada correctamente." });
                }

                // Log ModelState errors
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                System.Diagnostics.Debug.WriteLine("Errores de ModelState: " + string.Join(", ", errors));

                return Json(new
                {
                    success = false,
                    message = "Datos inválidos.",
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al crear materia: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "Error al crear la materia: " + ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarMateria(int id_materia)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var materia = db.MATERIA.Find(id_materia);
                    if (materia == null)
                    {
                        return Json(new { success = false, message = "La materia no existe." });
                    }

                    // Eliminar la materia
                    db.MATERIA.Remove(materia);
                    db.SaveChanges();
                    transaction.Commit();

                    return Json(new { success = true, message = "Materia eliminada correctamente" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
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

}


