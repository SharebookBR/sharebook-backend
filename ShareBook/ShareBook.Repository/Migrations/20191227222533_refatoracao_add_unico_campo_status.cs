using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareBook.Infra.Data.Migrations
{
    public partial class refatoracao_add_unico_campo_status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Canceled",
                table: "Books");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Books",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Books");

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Books",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Canceled",
                table: "Books",
                nullable: false,
                defaultValue: false);
        }
    }
}
