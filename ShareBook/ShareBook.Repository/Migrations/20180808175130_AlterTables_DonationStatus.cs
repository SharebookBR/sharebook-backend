using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareBook.Repository.Migrations
{
    public partial class AlterTables_DonationStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BookUser",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "BookUser");
        }
    }
}
