using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ShareBook.Repository.Migrations
{
    public partial class AlterTable_ForgotPassword : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashCodePassword",
                table: "Users",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HashCodePasswordExpiryDate",
                table: "Users",
                type: "datetime2(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashCodePassword",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HashCodePasswordExpiryDate",
                table: "Users");
        }
    }
}
