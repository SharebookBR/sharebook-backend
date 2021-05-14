using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareBook.Repository.Migrations
{
    public partial class AlterTable_Books_QuantityValidations_to_TotalAdminReviews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuantityValidatons",
                table: "Books",
                newName: "TotalAdminReviews");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalAdminReviews",
                table: "Books",
                newName: "QuantityValidatons");
        }
    }
}
