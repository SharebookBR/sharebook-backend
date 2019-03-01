using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ShareBook.Repository.Migrations
{
    public partial class BiggerAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "Addresses",
                type: "varchar(80)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Neighborhood",
                table: "Addresses",
                type: "varchar(50)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Addresses",
                type: "varchar(50)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Complement",
                table: "Addresses",
                type: "varchar(50)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Addresses",
                type: "varchar(50)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "Addresses",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(80)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Neighborhood",
                table: "Addresses",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Addresses",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Complement",
                table: "Addresses",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Addresses",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 30,
                oldNullable: true);
        }
    }
}
