using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareBook.Repository.Migrations
{
    public partial class RequestParentAproval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ParentAproved",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentEmail",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentHashCodeAproval",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentAproved",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ParentEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ParentHashCodeAproval",
                table: "Users");
        }
    }
}
