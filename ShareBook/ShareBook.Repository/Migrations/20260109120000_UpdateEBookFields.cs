using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Infra.Data.Migrations
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
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FreightOption",
                table: "Books",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EBookPdfPath",
                table: "Books");

            migrationBuilder.AlterColumn<int>(
                name: "FreightOption",
                table: "Books",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

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
