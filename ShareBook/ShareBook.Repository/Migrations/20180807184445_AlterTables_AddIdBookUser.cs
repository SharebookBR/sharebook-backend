using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ShareBook.Repository.Migrations
{
    public partial class AlterTables_AddIdBookUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Users_UserId",
                table: "Books");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookUser",
                table: "BookUser");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "BookUser",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "BookUser",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Books",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookUser",
                table: "BookUser",
                columns: new[] { "Id", "BookId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookUser_BookId",
                table: "BookUser",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Users_UserId",
                table: "Books",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Users_UserId",
                table: "Books");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookUser",
                table: "BookUser");

            migrationBuilder.DropIndex(
                name: "IX_BookUser_BookId",
                table: "BookUser");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "BookUser");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "BookUser");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Books",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookUser",
                table: "BookUser",
                columns: new[] { "BookId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Users_UserId",
                table: "Books",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
