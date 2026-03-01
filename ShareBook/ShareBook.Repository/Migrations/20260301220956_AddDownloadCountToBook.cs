using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadCountToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DownloadCount",
                table: "Books",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadCount",
                table: "Books");
        }
    }
}
