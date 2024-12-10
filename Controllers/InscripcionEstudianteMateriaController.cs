﻿using SistemaUniversidadv1._0.Models;
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
                .Include(e => e.CARRERA) // Incluye los datos relacionados con la carrera del estudiante
                .FirstOrDefault(e => e.id_estudiante == estudianteId); // Busca al estudiante por su ID

            if (estudiante == null)
                return HttpNotFound(); // Si no se encuentra al estudiante, retorna un error 404

            // Obtener los ciclos asociados a la carrera del estudiante
            var ciclosConMaterias = db.CICLO
                .Where(c => c.carrera_id == estudiante.carrera_id) // Filtra los ciclos según la carrera del estudiante
                .Include(c => c.MATERIA) // Incluye las materias asociadas a cada ciclo
                .Select(ciclo => new CicloInscripcionViewModel
                {
                    NombreCiclo = ciclo.nombre_ciclo, // Asigna el nombre del ciclo
                    Materias = ciclo.MATERIA.Select(m => new MateriaInscripcionViewModel
                    {
                        MateriaId = m.id_materia, // Asigna el ID de la materia
                        NombreMateria = m.nombre_materia, // Asigna el nombre de la materia
                        codigo_materia = m.codigo_materia, // Asigna el código de la materia
                                                           // Determina el estado de la inscripción: "Inscrito" o "No Inscrito"
                        Estado = db.INSCRIPCIONESTUDIANTEMATERIA
                            .Any(i => i.estudiante_id == estudianteId && i.materia_id == m.id_materia)
                            ? "Inscrito" // Si ya está inscrito, muestra "Inscrito"
                            : "No Inscrito", // Si no está inscrito, muestra "No Inscrito"
                                             // Obtiene la fecha de inscripción de la materia
                        FechaInscripcion = db.INSCRIPCIONESTUDIANTEMATERIA
                            .Where(i => i.estudiante_id == estudianteId && i.materia_id == m.id_materia)
                            .Select(i => i.fecha_inscripcion_estudiante_materia)
                            .FirstOrDefault() // Devuelve la primera fecha de inscripción o null si no está inscrito
                    }).ToList()
                })
                .ToList(); // Convierte los ciclos con sus materias en una lista

            // Crear el ViewModel para la vista
            var viewModel = new ListadoInscripcionesViewModel
            {
                EstudianteId = estudianteId, // Asigna el ID del estudiante
                NombreEstudiante = estudiante.nombre_estudiante + " " + estudiante.apellido_estudiante, // Asigna el nombre completo del estudiante
                Ciclos = ciclosConMaterias // Asigna la lista de ciclos con materias
            };

            return View(viewModel); // Retorna la vista con el modelo de datos
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InscribirEstudianteMateria(int estudianteId, List<int> MateriaIds)
        {
            try
            {
                // Verificar si no se seleccionó ninguna materia
                if (MateriaIds == null || !MateriaIds.Any())
                {
                    // Eliminar todas las materias inscritas para el estudiante
                    var materiasInscritas = db.INSCRIPCIONESTUDIANTEMATERIA
                        .Where(i => i.estudiante_id == estudianteId) // Filtra las inscripciones del estudiante por su ID
                        .ToList(); // Convierte los resultados en una lista

                    foreach (var inscripcion in materiasInscritas)
                    {
                        db.INSCRIPCIONESTUDIANTEMATERIA.Remove(inscripcion); // Elimina cada inscripción
                    }

                    db.SaveChanges(); // Guarda los cambios en la base de datos
                    TempData["SuccessMessage"] = "Todas las inscripciones fueron eliminadas correctamente."; // Mensaje de éxito
                    return RedirectToAction("VerInscripciones", new { estudianteId = estudianteId }); // Redirige a la vista de inscripciones
                }

                // Obtener materias actualmente inscritas del estudiante
                var materiasInscritasActuales = db.INSCRIPCIONESTUDIANTEMATERIA
                    .Where(i => i.estudiante_id == estudianteId) // Filtra las inscripciones del estudiante
                    .Select(i => new { i.materia_id }) // Selecciona solo el ID de la materia
                    .ToList(); // Convierte los resultados en una lista

                // Nuevas inscripciones: Materias seleccionadas que no están inscritas
                var nuevasInscripciones = MateriaIds.Except(materiasInscritasActuales.Select(m => m.materia_id)).ToList();
                // Materias a desinscribir: Materias inscritas que no están seleccionadas
                var materiasDesinscribir = materiasInscritasActuales
                    .Where(m => !MateriaIds.Contains(m.materia_id)) // Filtra las materias que no están en la lista seleccionada
                    .Select(m => m.materia_id) // Selecciona solo los IDs de las materias
                    .ToList();

                // Fecha de inscripción: Se asigna la fecha actual para las nuevas inscripciones
                DateTime fechaInscripcion = DateTime.Now;

                // Agregar nuevas inscripciones
                foreach (var materiaId in nuevasInscripciones)
                {
                    var inscripcion = new INSCRIPCIONESTUDIANTEMATERIA
                    {
                        estudiante_id = estudianteId, // Asigna el ID del estudiante
                        materia_id = materiaId, // Asigna el ID de la materia
                        estado_inscripcion = true, // Marca la inscripción como activa
                        fecha_inscripcion_estudiante_materia = fechaInscripcion // Asigna la fecha de inscripción
                    };

                    db.INSCRIPCIONESTUDIANTEMATERIA.Add(inscripcion); // Agrega la inscripción a la base de datos
                }

                // Eliminar inscripciones desmarcadas
                foreach (var materiaId in materiasDesinscribir)
                {
                    var inscripcion = db.INSCRIPCIONESTUDIANTEMATERIA
                        .FirstOrDefault(i => i.estudiante_id == estudianteId && i.materia_id == materiaId); // Busca la inscripción para eliminar

                    if (inscripcion != null)
                    {
                        db.INSCRIPCIONESTUDIANTEMATERIA.Remove(inscripcion); // Elimina la inscripción de la base de datos
                    }
                }

                db.SaveChanges(); // Guarda los cambios en la base de datos
                TempData["SuccessMessage"] = "Las materias fueron actualizadas correctamente."; // Mensaje de éxito
                return RedirectToAction("VerInscripciones", new { estudianteId = estudianteId }); // Redirige a la vista de inscripciones
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar las materias: " + ex.Message); // Si hay un error, agrega el mensaje al modelo
                return RedirectToAction("VerInscripciones", new { estudianteId = estudianteId }); // Redirige a la vista de inscripciones
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