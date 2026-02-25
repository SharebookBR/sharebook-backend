using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Repository.Migrations
{
    public partial class UpdateEBookFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EBookDownloadLink",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "EBookPdfFile",
                table: "Books");

            migrationBuilder.AddColumn<string>(
                name: "EBookPdfPath",
                table: "Books",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EBookPdfPath",
                table: "Books");

            migrationBuilder.AddColumn<string>(
                name: "EBookDownloadLink",
                table: "Books",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EBookPdfFile",
                table: "Books",
                type: "text",
                nullable: true);
        }
    }
}
