using SistemaUniversidadv1._0.Models;
using SistemaUniversidadv1._0.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

namespace SistemaUniversidadv1._0.Controllers
{
    public class InscripcionEstudianteMateriaController : Controller
    {
        private UniversidadContext db = new UniversidadContext(); // El contexto de la base de datos

        // Acción GET para inscribir materias a un estudiante
        public ActionResult InscribirEstudianteMateria(int estudianteId)
        {
            // Obtener la información del estudiante, incluyendo la carrera
            var estudiante = db.ESTUDIANTE
                .Include(e => e.CARRERA)
                .FirstOrDefault(e => e.id_estudiante == estudianteId);

            if (estudiante == null)
                return HttpNotFound();

            var ciclosConMaterias = db.CICLO
                .Where(c => c.carrera_id == estudiante.carrera_id)
                .Include(c => c.MATERIA)
                .Select(ciclo => new CicloInscripcionViewModel
                {
                    NombreCiclo = ciclo.nombre_ciclo,
                    Materias = ciclo.MATERIA.Select(m => new MateriaInscripcionViewModel
                    {
                        MateriaId = m.id_materia,
                        NombreMateria = m.nombre_materia,
                        codigo_materia = m.codigo_materia,
                        Estado = db.INSCRIPCIONESTUDIANTEMATERIA
                            .Any(i => i.estudiante_id == estudianteId && i.materia_id == m.id_materia)
                            ? "Inscrito"
                            : "No Inscrito",
                        FechaInscripcion = db.INSCRIPCIONESTUDIANTEMATERIA
                            .Where(i => i.estudiante_id == estudianteId && i.materia_id == m.id_materia)
                            .Select(i => i.fecha_inscripcion_estudiante_materia)
                            .FirstOrDefault() // Obtiene la fecha de inscripción
                    }).ToList()
                })
                .ToList();

            // Crear el ViewModel para la vista
            var viewModel = new ListadoInscripcionesViewModel
            {
                EstudianteId = estudianteId,
                NombreEstudiante = estudiante.nombre_estudiante + " " + estudiante.apellido_estudiante,
                Ciclos = ciclosConMaterias
            };

            return View(viewModel);
        }



        // Acción POST para inscribir materias a un estudiante
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InscribirEstudianteMateria(int estudianteId, List<int> MateriaIds)
        {
            try
            {
                // Obtener materias actualmente inscritas del estudiante
                var materiasInscritas = db.INSCRIPCIONESTUDIANTEMATERIA
                    .Where(i => i.estudiante_id == estudianteId)
                    .Select(i => new { i.materia_id, i.fecha_inscripcion_estudiante_materia })
                    .ToList();

                // Nuevas inscripciones: Materias seleccionadas que no están inscritas
                var nuevasInscripciones = MateriaIds.Except(materiasInscritas.Select(m => m.materia_id)).ToList();

                // Materias a desinscribir: Materias inscritas que no están seleccionadas
                var materiasDesinscribir = materiasInscritas
                    .Where(m => !MateriaIds.Contains(m.materia_id))
                    .Select(m => m.materia_id)
                    .ToList();

                // Fecha de inscripción: Se asigna la fecha actual para las nuevas inscripciones
                DateTime fechaInscripcion = DateTime.Now;

                // Agregar nuevas inscripciones con la fecha actual
                foreach (var materiaId in nuevasInscripciones)
                {
                    var inscripcion = new INSCRIPCIONESTUDIANTEMATERIA
                    {
                        estudiante_id = estudianteId,
                        materia_id = materiaId,
                        estado_inscripcion = true,
                        fecha_inscripcion_estudiante_materia = fechaInscripcion
                    };

                    db.INSCRIPCIONESTUDIANTEMATERIA.Add(inscripcion);
                }

                // Eliminar inscripciones desmarcadas
                foreach (var materiaId in materiasDesinscribir)
                {
                    var inscripcion = db.INSCRIPCIONESTUDIANTEMATERIA
                        .FirstOrDefault(i => i.estudiante_id == estudianteId && i.materia_id == materiaId);

                    if (inscripcion != null)
                    {
                        db.INSCRIPCIONESTUDIANTEMATERIA.Remove(inscripcion);
                    }
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Las materias fueron actualizadas correctamente.";
                return RedirectToAction("VerInscripciones", new { estudianteId = estudianteId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar las materias: " + ex.Message);
                return RedirectToAction("VerInscripciones", new { estudianteId = estudianteId });
            }
        }



        // Acción GET para ver las inscripciones de un estudiante
        [HttpGet]
        public ActionResult VerInscripciones(int estudianteId)
        {
            var estudiante = db.ESTUDIANTE
                .Include(e => e.CARRERA)
                .FirstOrDefault(e => e.id_estudiante == estudianteId);

            if (estudiante == null)
                return HttpNotFound();

            // Obtener las materias por ciclo para el estudiante
            var materiasPorCiclo = db.CICLO
                .Where(c => c.carrera_id == estudiante.carrera_id)
                .Include(c => c.MATERIA)
                .Select(ciclo => new CicloInscripcionViewModel
                {
                    NombreCiclo = ciclo.nombre_ciclo,
                    Materias = ciclo.MATERIA.Select(m => new MateriaInscripcionViewModel
                    {
                        MateriaId = m.id_materia,
                        NombreMateria = m.nombre_materia,
                        codigo_materia = m.codigo_materia,
                        Estado = db.INSCRIPCIONESTUDIANTEMATERIA
                            .Any(i => i.estudiante_id == estudianteId && i.materia_id == m.id_materia)
                            ? "Inscrito"
                            : "No Inscrito",
                        FechaInscripcion = db.INSCRIPCIONESTUDIANTEMATERIA
                            .Where(i => i.estudiante_id == estudianteId && i.materia_id == m.id_materia)
                            .Select(i => i.fecha_inscripcion_estudiante_materia)
                            .FirstOrDefault()
                    }).ToList()
                })
                .ToList();

            var viewModel = new ListadoInscripcionesViewModel
            {
                EstudianteId = estudiante.id_estudiante,
                NombreEstudiante = estudiante.nombre_estudiante + " " + estudiante.apellido_estudiante,
                Ciclos = materiasPorCiclo
            };

            return View(viewModel);
        }


        public ActionResult GenerarPDFInscripciones(int estudianteId)
        {
            var estudiante = db.ESTUDIANTE
                .Include(e => e.CARRERA)
                .FirstOrDefault(e => e.id_estudiante == estudianteId);

            if (estudiante == null)
                return HttpNotFound();

            // Obtener las materias inscritas del estudiante, agrupadas por ciclo
            var inscripcionesPorCiclo = db.CICLO
                .Where(c => c.carrera_id == estudiante.carrera_id)
                .Include(c => c.MATERIA)
                .Select(ciclo => new
                {
                    NombreCiclo = ciclo.nombre_ciclo,
                    Materias = ciclo.MATERIA.Select(m => new
                    {
                        m.id_materia,
                        m.nombre_materia,
                        m.codigo_materia,
                        Estado = db.INSCRIPCIONESTUDIANTEMATERIA
                            .Any(i => i.estudiante_id == estudianteId && i.materia_id == m.id_materia)
                            ? "Inscrito"
                            : "No Inscrito",
                        FechaInscripcion = db.INSCRIPCIONESTUDIANTEMATERIA
                            .Where(i => i.estudiante_id == estudianteId && i.materia_id == m.id_materia)
                            .Select(i => i.fecha_inscripcion_estudiante_materia)
                            .FirstOrDefault()
                    }).ToList()
                })
                .ToList();

            // Configuración del documento PDF
            MemoryStream memoryStream = new MemoryStream();
            Document document = new Document(PageSize.A4, 15, 15, 15, 15);
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            // Título del reporte
            var titulo = new Paragraph($"INSCRIPCIONES DE MATERIAS - {estudiante.nombre_estudiante} {estudiante.apellido_estudiante}",
                new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD));
            titulo.Alignment = Element.ALIGN_CENTER;
            titulo.SpacingAfter = 5f;
            document.Add(titulo);

            // Agregar nombre de la carrera
            var carrera = new Paragraph($"{estudiante.CARRERA.nombre_carrera}",
                new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL));
            carrera.Alignment = Element.ALIGN_CENTER;
            carrera.SpacingAfter = 10f;
            document.Add(carrera);

            // Crear una tabla para las inscripciones de materias (90% del ancho de página)
            PdfPTable table = new PdfPTable(4) { WidthPercentage = 90 };
            table.SetWidths(new float[] { 20f, 30f, 25f, 25f });

            // Encabezados de tabla con fuente más pequeña
            var headerFont = new Font(Font.FontFamily.HELVETICA, 11, Font.BOLD);
            PdfPCell headerCell1 = new PdfPCell(new Phrase("Código", headerFont));
            PdfPCell headerCell2 = new PdfPCell(new Phrase("Materia", headerFont));
            PdfPCell headerCell3 = new PdfPCell(new Phrase("Estado", headerFont));
            PdfPCell headerCell4 = new PdfPCell(new Phrase("Fecha Inscripción", headerFont));

            // Configurar celdas de encabezado
            foreach (PdfPCell cell in new[] { headerCell1, headerCell2, headerCell3, headerCell4 })
            {
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.BackgroundColor = new BaseColor(52, 152, 219); // Azul más oscuro para encabezados
                cell.Padding = 3;
                cell.MinimumHeight = 15f;
            }

            table.AddCell(headerCell1);
            table.AddCell(headerCell2);
            table.AddCell(headerCell3);
            table.AddCell(headerCell4);

            // Contenido con fuente más pequeña
            var contentFont = new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL);
            var cicloPhraseFont = new Font(Font.FontFamily.HELVETICA, 11, Font.BOLD, BaseColor.WHITE);

            foreach (var ciclo in inscripcionesPorCiclo)
            {
                // Agregar fila de ciclo con nuevo color
                PdfPCell cicloCell = new PdfPCell(new Phrase(ciclo.NombreCiclo, cicloPhraseFont));
                cicloCell.Colspan = 4;
                cicloCell.BackgroundColor = new BaseColor(26, 82, 118);
                cicloCell.HorizontalAlignment = Element.ALIGN_CENTER;
                cicloCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cicloCell.Padding = 3;
                cicloCell.MinimumHeight = 15f;
                table.AddCell(cicloCell);

                foreach (var materia in ciclo.Materias)
                {
                    // Crear celdas con el contenido de cada materia
                    PdfPCell[] celdas = {
                new PdfPCell(new Phrase(materia.codigo_materia, contentFont)),
                new PdfPCell(new Phrase(materia.nombre_materia, contentFont)),
                new PdfPCell(new Phrase(materia.Estado, contentFont)),
                new PdfPCell(new Phrase(materia.FechaInscripcion?.ToString("dd/MM/yyyy") ?? "-", contentFont))
            };

                    // Configurar todas las celdas
                    foreach (var celda in celdas)
                    {
                        celda.HorizontalAlignment = Element.ALIGN_CENTER;
                        celda.VerticalAlignment = Element.ALIGN_MIDDLE;
                        celda.Padding = 2;
                        celda.MinimumHeight = 15f;
                    }

                    // Agregar las celdas a la tabla
                    foreach (var celda in celdas)
                    {
                        table.AddCell(celda);
                    }
                }
            }

            // Añadir la tabla al documento
            document.Add(table);
            document.Close();
            byte[] pdfBytes = memoryStream.ToArray();

            return File(pdfBytes, "application/pdf", $"Inscripciones_Materias-{estudiante.dni_estudiante}-{estudiante.CARRERA.nombre_carrera}.pdf");
        }





    }
}
