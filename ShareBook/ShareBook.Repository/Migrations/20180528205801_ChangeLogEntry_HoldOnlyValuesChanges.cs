using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareBook.Repository.Migrations
{
    public partial class ChangeLogEntry_HoldOnlyValuesChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalValues",
                table: "LogEntries");

            migrationBuilder.RenameColumn(
                name: "UpdatedValues",
                table: "LogEntries",
                newName: "ValuesChanges");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValuesChanges",
                table: "LogEntries",
                newName: "UpdatedValues");

            migrationBuilder.AddColumn<string>(
                name: "OriginalValues",
                table: "LogEntries",
                nullable: true);
        }
    }
}
