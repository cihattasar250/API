using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace spor_proje_api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueConstraintFromPerformansTakibi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PerformansTakibi_UyeId_Tarih",
                table: "PerformansTakibi");

            migrationBuilder.CreateIndex(
                name: "IX_PerformansTakibi_UyeId",
                table: "PerformansTakibi",
                column: "UyeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PerformansTakibi_UyeId",
                table: "PerformansTakibi");

            migrationBuilder.CreateIndex(
                name: "IX_PerformansTakibi_UyeId_Tarih",
                table: "PerformansTakibi",
                columns: new[] { "UyeId", "Tarih" },
                unique: true);
        }
    }
}
