using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Repository.Migrations
{
    public partial class MailBounceDbIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "MailBounces",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MailBounces_Email",
                table: "MailBounces",
                column: "Email");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MailBounces_Email",
                table: "MailBounces");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "MailBounces",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
