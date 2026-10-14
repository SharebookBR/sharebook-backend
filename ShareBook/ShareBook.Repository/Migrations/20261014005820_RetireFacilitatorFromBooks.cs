using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RetireFacilitatorFromBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "FacilitatorNotes",
                table: "Books",
                newName: "AdminNotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdminNotes",
                table: "Books",
                newName: "FacilitatorNotes");

            migrationBuilder.AddColumn<Guid>(
                name: "UserIdFacilitator",
                table: "Books",
                type: "uuid",
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
                principalColumn: "Id");
        }
    }
}
