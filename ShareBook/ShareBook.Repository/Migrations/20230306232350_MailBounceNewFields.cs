using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Repository.Migrations
{
    public partial class MailBounceNewFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailSubject",
                table: "MailBounces",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "EmailBody",
                table: "MailBounces",
                newName: "Body");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "MailBounces",
                newName: "EmailSubject");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "MailBounces",
                newName: "EmailBody");
        }
    }
}
