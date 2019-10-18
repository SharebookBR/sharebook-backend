using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareBook.Repository.Migrations
{
    public partial class PasswordUpdateDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordLastUpdate",
                table: "Users",
                nullable: false,
                defaultValue: new DateTime(2019, 10, 18, 14, 14, 0, 510, DateTimeKind.Local).AddTicks(8680));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordLastUpdate",
                table: "Users");
        }
    }
}
