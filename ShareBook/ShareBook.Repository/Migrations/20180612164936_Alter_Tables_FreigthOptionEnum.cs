using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareBook.Repository.Migrations
{
    public partial class Alter_Tables_FreigthOptionEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FreightOption",
                table: "Books",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FreightOption",
                table: "Books",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
