using System;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;

namespace Hospital_magnment_system.Helpers
{
    public class ExportManager
    {
        public void ExportToExcel(DataView data, string reportName)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"{reportName}_{DateTime.Now:yyyyMMdd}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Report");

                // Add headers
                for (int i = 0; i < data.Table.Columns.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = data.Table.Columns[i].ColumnName;
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                }

                // Add data
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data.Table.Columns.Count; j++)
                    {
                        worksheet.Cell(i + 2, j + 1).Value = data[i][j].ToString();
                    }
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(saveDialog.FileName);
            }
        }

        public void ExportToPdf(DataView data, string reportName)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"{reportName}_{DateTime.Now:yyyyMMdd}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                using var fs = new FileStream(saveDialog.FileName, FileMode.Create);
                var document = new Document(PageSize.A4, 25, 25, 30, 30);
                var writer = PdfWriter.GetInstance(document, fs);

                document.Open();

                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph(reportName, titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // Create table
                var table = new PdfPTable(data.Table.Columns.Count);
                table.WidthPercentage = 100;

                // Add headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                foreach (DataColumn column in data.Table.Columns)
                {
                    var cell = new PdfPCell(new Phrase(column.ColumnName, headerFont));
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);
                }

                // Add data
                var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (DataRowView row in data)
                {
                    foreach (var item in row.Row.ItemArray)
                    {
                        var cell = new PdfPCell(new Phrase(item.ToString(), dataFont));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.Padding = 5;
                        table.AddCell(cell);
                    }
                }

                document.Add(table);
                document.Close();
            }
        }
    }
} 