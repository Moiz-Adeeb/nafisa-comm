using System.Reflection;
using Application.Interfaces;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace Infrastructure.Services;

public class PdfService : IPdfService
{
    public byte[] WriteRows<T>(
        List<T> data,
        string headerText = "Report",
        Dictionary<string, string>? details = null
    )
    {
        if (data == null || !data.Any())
            return [];

        using (var memoryStream = new MemoryStream())
        {
            using (var pdfWriter = new PdfWriter(memoryStream))
            using (var pdfDocument = new PdfDocument(pdfWriter))
            using (var document = new Document(pdfDocument))
            {
                // Get property metadata for headers
                var properties = typeof(T).GetProperties(
                    BindingFlags.Public | BindingFlags.Instance
                );

                // Create table with number of columns = number of properties
                var table = new Table(properties.Length, true);
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Header row
                foreach (var prop in properties)
                {
                    table.AddHeaderCell(new Cell().Add(new Paragraph(prop.Name).SetFont(boldFont)));
                }

                // Data rows
                foreach (var record in data)
                {
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(record)?.ToString() ?? string.Empty;
                        table.AddCell(new Cell().Add(new Paragraph(value)));
                    }
                }
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var header = new Paragraph(headerText)
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(header);
                var date = new Paragraph($"Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm}")
                    .SetFont(normalFont)
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(date);
                document.Add(new Paragraph("\n")); // spacing
                if (details != null && details.Any())
                {
                    foreach (var keyValuePair in details)
                    {
                        var detail = new Paragraph($"{keyValuePair.Key}: {keyValuePair.Value}")
                            .SetFont(normalFont)
                            .SetFontSize(10)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                        document.Add(detail);
                    }
                    document.Add(new Paragraph("\n")); // spacing
                }
                document.Add(table);
            }

            return memoryStream.ToArray();
        }
    }

    public byte[] GenerateReport(Dictionary<string, bool> equipments, string shift, string date)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var pdfWriter = new PdfWriter(memoryStream))
            using (var pdfDocument = new PdfDocument(pdfWriter))
            using (var document = new Document(pdfDocument, iText.Kernel.Geom.PageSize.A4))
            {
                // Set smaller margins to maximize usable space
                document.SetMargins(20, 10, 20, 10);

                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Calculate row heights to fill the page
                float pageHeight = iText.Kernel.Geom.PageSize.A4.GetHeight();
                float usableHeight = pageHeight - 40; // accounting for margins (20 top + 20 bottom)
                float headerRowHeight = 25f;
                float subHeaderRowHeight = 20f;
                float contentRowHeight =
                    (usableHeight - headerRowHeight - subHeaderRowHeight) / 40f;

                // Create table with 11 columns
                var table = new Table(UnitValue.CreatePercentArray(11), false);
                table.UseAllAvailableWidth();

                // Row 1: Header row with merged cells
                // Columns 1-4: Title (colspan=4)
                var titleCell = new Cell(1, 5)
                    .Add(
                        new Paragraph("FICHE DE TRANSMISSION DES CHECKLIST")
                            .SetFont(boldFont)
                            .SetFontSize(12)
                    )
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(null);
                table.AddCell(titleCell);

                // Columns 5-7: Shift (colspan=3)
                var shiftCell = new Cell(1, 3)
                    .Add(new Paragraph($"SHIFT: {shift}").SetFont(boldFont).SetFontSize(10))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                table.AddCell(shiftCell);

                // Columns 8-11: Date (colspan=4)
                var dateCell = new Cell(1, 3)
                    .Add(new Paragraph($"DATE: {date}").SetFont(boldFont).SetFontSize(10))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                table.AddCell(dateCell);

                // Row 2: Sub-header row
                // Column 1: Number of Pieces
                var col1Header = new Cell(1, 1)
                    .Add(new Paragraph("Number of Pieces").SetFont(boldFont).SetFontSize(8))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                table.AddCell(col1Header);

                // Column 2: Depart Service
                var col2Header = new Cell(1, 1)
                    .Add(new Paragraph("Depart Service").SetFont(boldFont).SetFontSize(8))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                table.AddCell(col2Header);

                // Column 3: DESTINATAIRE
                var col3Header = new Cell(1, 1)
                    .Add(new Paragraph("DESTINATAIRE").SetFont(boldFont).SetFontSize(8))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                table.AddCell(col3Header);

                // Columns 4-11: EQUIPEMENTS CONTROLES (colspan=8)
                var equipmentHeader = new Cell(1, 8)
                    .Add(new Paragraph("EQUIPEMENTS CONTROLES").SetFont(boldFont).SetFontSize(10))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                table.AddCell(equipmentHeader);

                // Rows 3-42 (40 content rows)
                // First 3 columns have rowspan=40
                var col1Content = new Cell(40, 1)
                    .Add(new Paragraph("").SetFont(normalFont).SetFontSize(8))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                table.AddCell(col1Content);

                var col2Content = new Cell(40, 1)
                    .Add(new Paragraph("").SetFont(normalFont).SetFontSize(8))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                table.AddCell(col2Content);

                var col3Content = new Cell(40, 1)
                    .Add(new Paragraph("").SetFont(normalFont).SetFontSize(8))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                table.AddCell(col3Content);

                // Prepare equipment list
                var equipmentList = equipments?.ToList() ?? new List<KeyValuePair<string, bool>>();

                // Fill 40 rows × 4 equipment items per row = 160 cells
                for (int row = 0; row < 30; row++)
                {
                    // Each row has 4 equipment items (columns 4-11)
                    // Pattern: Equipment, Checkbox, Equipment, Checkbox, Equipment, Checkbox, Equipment, Checkbox
                    for (int item = 0; item < 4; item++)
                    {
                        int index = row * 4 + item;

                        if (index < equipmentList.Count)
                        {
                            var equipment = equipmentList[index];

                            // Equipment name cell
                            var equipmentNameCell = new Cell()
                                .Add(
                                    new Paragraph(equipment.Key).SetFont(normalFont).SetFontSize(7)
                                )
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                            table.AddCell(equipmentNameCell);

                            // Checkbox cell
                            var checkboxCell = new Cell()
                                .Add(
                                    new Paragraph(equipment.Value ? "X" : "")
                                        .SetFont(boldFont)
                                        .SetFontSize(8)
                                )
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                            table.AddCell(checkboxCell);
                        }
                        else
                        {
                            // Empty equipment name cell
                            table.AddCell(
                                new Cell().Add(new Paragraph("\u00A0").SetFont(normalFont))
                            );

                            // Empty checkbox cell
                            table.AddCell(
                                new Cell().Add(new Paragraph("\u00A0").SetFont(normalFont))
                            );
                        }
                    }
                }

                document.Add(table);
            }

            return memoryStream.ToArray();
        }
    }
}
