using LibraryApp.Models.Domain;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;

namespace LibraryApp.Helpers
{
    public class DataTableHelper
    {

        public static DataTable GenerateBooksDataTable(List<Book> books)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("Author");
            dataTable.Columns.Add("Author Age");
            dataTable.Columns.Add("Title");
            dataTable.Columns.Add("Description");
            dataTable.Columns.Add("Genre");
            dataTable.Columns.Add("Pages");

            foreach (var book in books)
            {
                DataRow tableRow = dataTable.NewRow();

                tableRow["Author"] = book.Author?.Name;
                tableRow["Author Age"] = book.Author?.Age;
                tableRow["Title"] = book.Title;
                tableRow["Description"] = book.Description;
                tableRow["Genre"] = book.BookGenre?.Name;
                tableRow["Pages"] = book.Pages;

                dataTable.Rows.Add(tableRow);
            }

            return dataTable;
        }        

    }
}
