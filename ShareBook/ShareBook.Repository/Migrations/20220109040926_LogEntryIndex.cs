using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Repository.Migrations
{
    public partial class LogEntryIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<bool>(
            //    name: "ParentAproved",
            //    table: "Users",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<string>(
            //    name: "ParentEmail",
            //    table: "Users",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "ParentHashCodeAproval",
            //    table: "Users",
            //    type: "nvarchar(max)",
            //    nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "LogEntries",
                type: "varchar(64)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_EntityName_EntityId",
                table: "LogEntries",
                columns: new[] { "EntityName", "EntityId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LogEntries_EntityName_EntityId",
                table: "LogEntries");

            //migrationBuilder.DropColumn(
            //    name: "ParentAproved",
            //    table: "Users");

            //migrationBuilder.DropColumn(
            //    name: "ParentEmail",
            //    table: "Users");

            //migrationBuilder.DropColumn(
            //    name: "ParentHashCodeAproval",
            //    table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "LogEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldNullable: true);
        }
    }
}
