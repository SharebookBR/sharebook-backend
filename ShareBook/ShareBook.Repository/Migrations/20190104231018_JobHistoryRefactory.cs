using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ShareBook.Repository.Migrations
{
    public partial class JobHistoryRefactory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookUser_Jobs_JobId",
                table: "BookUser");

            migrationBuilder.DropForeignKey(
                name: "FK_JobHistories_Jobs_JobId",
                table: "JobHistories");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_JobHistories_JobId",
                table: "JobHistories");

            migrationBuilder.DropIndex(
                name: "IX_BookUser_JobId",
                table: "BookUser");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "JobHistories");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "BookUser");

            migrationBuilder.AlterColumn<string>(
                name: "LastResult",
                table: "JobHistories",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "JobHistories",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "JobName",
                table: "JobHistories",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobName",
                table: "JobHistories");

            migrationBuilder.AlterColumn<string>(
                name: "LastResult",
                table: "JobHistories",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "JobHistories",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "JobHistories",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "BookUser",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: true),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    Interval = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobHistories_JobId",
                table: "JobHistories",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_BookUser_JobId",
                table: "BookUser",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookUser_Jobs_JobId",
                table: "BookUser",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobHistories_Jobs_JobId",
                table: "JobHistories",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
