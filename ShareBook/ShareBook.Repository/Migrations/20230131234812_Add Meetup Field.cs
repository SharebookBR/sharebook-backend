using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Repository.Migrations
{
    public partial class AddMeetupField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetupParticipants_Meetups_MeetupId",
                table: "MeetupParticipants");

            migrationBuilder.AddColumn<bool>(
                name: "IsParticipantListSynced",
                table: "Meetups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "MeetupId",
                table: "MeetupParticipants",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetupParticipants_Meetups_MeetupId",
                table: "MeetupParticipants",
                column: "MeetupId",
                principalTable: "Meetups",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetupParticipants_Meetups_MeetupId",
                table: "MeetupParticipants");

            migrationBuilder.DropColumn(
                name: "IsParticipantListSynced",
                table: "Meetups");

            migrationBuilder.AlterColumn<Guid>(
                name: "MeetupId",
                table: "MeetupParticipants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetupParticipants_Meetups_MeetupId",
                table: "MeetupParticipants",
                column: "MeetupId",
                principalTable: "Meetups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
