using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ShareBook.Repository.Migrations
{
    public partial class facilitator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserIdFacilitator",
                table: "Books",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_UserIdFacilitator",
                table: "Books",
                column: "UserIdFacilitator");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Users_UserIdFacilitator",
                table: "Books",
                column: "UserIdFacilitator",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Users_UserIdFacilitator",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_UserIdFacilitator",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "UserIdFacilitator",
                table: "Books");
        }
    }
}
