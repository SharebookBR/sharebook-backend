using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareBook.Infra.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: true),
                    JobName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    IsSuccess = table.Column<bool>(nullable: false),
                    LastResult = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Details = table.Column<string>(type: "varchar(1000)", nullable: true),
                    TimeSpentSeconds = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: true),
                    UserId = table.Column<Guid>(nullable: true),
                    EntityName = table.Column<string>(nullable: true),
                    EntityId = table.Column<Guid>(nullable: false),
                    Operation = table.Column<string>(nullable: true),
                    LogDateTime = table.Column<DateTime>(nullable: false),
                    ValuesChanges = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PasswordSalt = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    HashCodePassword = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    HashCodePasswordExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastLogin = table.Column<DateTime>(nullable: false),
                    Linkedin = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    Profile = table.Column<int>(nullable: false),
                    Active = table.Column<bool>(nullable: false, defaultValueSql: "1"),
                    AllowSendingEmail = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: true),
                    Street = table.Column<string>(type: "varchar(80)", maxLength: 50, nullable: true),
                    Number = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    Complement = table.Column<string>(type: "varchar(50)", maxLength: 30, nullable: true),
                    Neighborhood = table.Column<string>(type: "varchar(50)", maxLength: 30, nullable: true),
                    PostalCode = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    City = table.Column<string>(type: "varchar(50)", maxLength: 30, nullable: true),
                    State = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    Country = table.Column<string>(type: "varchar(50)", maxLength: 30, nullable: true),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: true),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 50, nullable: false),
                    Author = table.Column<string>(type: "varchar(200)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ImageSlug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    FreightOption = table.Column<int>(nullable: false),
                    UserId = table.Column<Guid>(nullable: true),
                    UserIdFacilitator = table.Column<Guid>(nullable: true),
                    FacilitatorNotes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    CategoryId = table.Column<Guid>(nullable: false),
                    Approved = table.Column<bool>(nullable: false),
                    Canceled = table.Column<bool>(nullable: false),
                    ChooseDate = table.Column<DateTime>(nullable: true),
                    Synopsis = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    TrackingNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Books_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Books_Users_UserIdFacilitator",
                        column: x => x.UserIdFacilitator,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    BookId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: true),
                    NickName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Note = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    Reason = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookUser", x => new { x.Id, x.BookId, x.UserId });
                    table.ForeignKey(
                        name: "FK_BookUser_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookUser_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_CategoryId",
                table: "Books",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_UserId",
                table: "Books",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_UserIdFacilitator",
                table: "Books",
                column: "UserIdFacilitator");

            migrationBuilder.CreateIndex(
                name: "IX_BookUser_BookId",
                table: "BookUser",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookUser_UserId",
                table: "BookUser",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "BookUser");

            migrationBuilder.DropTable(
                name: "JobHistories");

            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
