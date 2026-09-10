using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookDownloadEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookDownloadEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DownloadedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookDownloadEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookDownloadEvents_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookDownloadEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookDownloadEvents_BookId_DownloadedAtUtc",
                table: "BookDownloadEvents",
                columns: new[] { "BookId", "DownloadedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BookDownloadEvents_DownloadedAtUtc_BookId",
                table: "BookDownloadEvents",
                columns: new[] { "DownloadedAtUtc", "BookId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookDownloadEvents_UserId_DownloadedAtUtc",
                table: "BookDownloadEvents",
                columns: new[] { "UserId", "DownloadedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookDownloadEvents");
        }
    }
}
