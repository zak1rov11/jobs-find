using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JobsApi.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Spheres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spheres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Company = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SalaryFrom = table.Column<int>(type: "INTEGER", nullable: false),
                    SalaryTo = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRemote = table.Column<bool>(type: "INTEGER", nullable: false),
                    LogoFileName = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SphereId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Spheres_SphereId",
                        column: x => x.SphereId,
                        principalTable: "Spheres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Spheres",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "IT" },
                    { 2, "Продажи" },
                    { 3, "Маркетинг" },
                    { 4, "Финансы" },
                    { 5, "Образование" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "UserName" },
                values: new object[] { 1, "100000.cGV0c2FwaS1zYWx0LTE2Yg==.H00b89fjNi8gz2R6e4k8nUTV3j60z15myD/PZj/LDXk=", "demo" });

            migrationBuilder.InsertData(
                table: "Jobs",
                columns: new[] { "Id", "Company", "CreatedAt", "IsRemote", "LogoFileName", "OwnerId", "SalaryFrom", "SalaryTo", "SphereId", "Title" },
                values: new object[,]
                {
                    { 1, "TechCorp", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, 1, 8000000, 15000000, 1, "Backend-разработчик" },
                    { 2, "Retail Group", new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc), false, null, 1, 4000000, 7000000, 2, "Менеджер по продажам" },
                    { 3, "Brand Studio", new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Utc), true, null, 1, 3000000, 5000000, 3, "SMM-специалист" },
                    { 4, "FinService", new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), false, null, 1, 5000000, 8000000, 4, "Бухгалтер" },
                    { 5, "Edu Center", new DateTime(2026, 10, 20, 0, 0, 0, 0, DateTimeKind.Utc), false, null, 1, 3500000, 6000000, 5, "Преподаватель английского" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OwnerId",
                table: "Jobs",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SphereId",
                table: "Jobs",
                column: "SphereId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Spheres");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
