using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryApp.Migrations
{
    /// <inheritdoc />
    public partial class AddingBookGenresTableInDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Books");

            migrationBuilder.AddColumn<int>(
                name: "BookGenreId",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookGenres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookGenres", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_BookGenreId",
                table: "Books",
                column: "BookGenreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_BookGenres_BookGenreId",
                table: "Books",
                column: "BookGenreId",
                principalTable: "BookGenres",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_BookGenres_BookGenreId",
                table: "Books");

            migrationBuilder.DropTable(
                name: "BookGenres");

            migrationBuilder.DropIndex(
                name: "IX_Books_BookGenreId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "BookGenreId",
                table: "Books");

            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
