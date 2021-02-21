using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareBook.Repository.Migrations
{
    public partial class Add_EBook_Fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EBookDownloadLink",
                table: "Books",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EBookPdfFile",
                table: "Books",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EBookDownloadLink",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "EBookPdfFile",
                table: "Books");
        }
    }
}
