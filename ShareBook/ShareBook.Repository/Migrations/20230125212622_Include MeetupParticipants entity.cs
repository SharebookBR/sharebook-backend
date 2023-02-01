using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Repository.Migrations
{
    public partial class IncludeMeetupParticipantsentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeetupParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeetupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetupParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetupParticipants_Meetups_MeetupId",
                        column: x => x.MeetupId,
                        principalTable: "Meetups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetupParticipants_MeetupId",
                table: "MeetupParticipants",
                column: "MeetupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetupParticipants");
        }
    }
}
