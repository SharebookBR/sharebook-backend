using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareBook.Repository.Migrations
{
    public partial class Alter_User_Cep_to_PostalCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cep",
                table: "Users",
                newName: "PostalCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "Users",
                newName: "Cep");
        }
    }
}
