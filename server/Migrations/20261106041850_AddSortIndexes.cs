using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSortIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CreatedAt",
                table: "Jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SalaryFrom",
                table: "Jobs",
                column: "SalaryFrom");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_CreatedAt",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_SalaryFrom",
                table: "Jobs");
        }
    }
}
