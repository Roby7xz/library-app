using Azure;
using LibraryApp.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.Reflection.Metadata;
using System.Text;
using Aspose.Pdf;
using AsposeDocument = Aspose.Pdf.Document;
using AsposeTable = Aspose.Pdf.Table;

namespace LibraryApp.Helpers
{
    public class ExportReportHelper
    {
        public static byte[] GenerateBooksExcelReport(List<Book> books)
        {
            var dataTable = DataTableHelper.GenerateBooksDataTable(books);

            var excelWorkbook = new XSSFWorkbook();

            var excelSheet = excelWorkbook.CreateSheet("Report");

            var excelRow = excelSheet.CreateRow(0);

            int excelColumnIndex = 0;
            foreach (DataColumn column in dataTable.Columns)
            {
                excelRow.CreateCell(excelColumnIndex).SetCellValue(column.ColumnName);
                excelColumnIndex++;
            }

            int excelRowIndex = 1;
            foreach (DataRow row in dataTable.Rows)
            {
                excelRow = excelSheet.CreateRow(excelRowIndex);

                int excelColumnCellIndex = 0;
                foreach (DataColumn column in dataTable.Columns)
                {
                    excelRow.CreateCell(excelColumnCellIndex).SetCellValue(row[column].ToString());
                    excelColumnCellIndex++;
                }

                excelRowIndex++;
            }

            using (var stream = new MemoryStream())
            {
                excelWorkbook.Write(stream, true);
                return stream.ToArray();
            }
        }

        public static byte[] GenerateBooksPDFReport(List<Book> books)
        {
            var dataTable = DataTableHelper.GenerateBooksDataTable(books);

            var asposeDocument = new AsposeDocument
            {
                PageInfo = new PageInfo { Margin = new MarginInfo(28, 28, 28, 40)}
            };

            var pdfPage = asposeDocument.Pages.Add();

            AsposeTable asposeTable = new AsposeTable
            {
                ColumnWidths = "16% 16% 16% 16% 16% 16%",
                DefaultCellPadding = new MarginInfo(10, 5, 10, 5),
                Border = new BorderInfo(BorderSide.All, .5f, Color.Black),
                DefaultCellBorder = new BorderInfo(BorderSide.All, .2f, Color.Black),
            };

            asposeTable.ImportDataTable(dataTable, true, 0, 0);
            asposeDocument.Pages[1].Paragraphs.Add(asposeTable);

            using (var stream = new MemoryStream())
            {
                asposeDocument.Save(stream);
                return stream.ToArray();
            }
        }
    }
}
