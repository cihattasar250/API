using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace spor_proje_api.Migrations
{
    /// <inheritdoc />
    public partial class AddPaketBilgileriToUye : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UyelikTuru",
                table: "Uyeler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaketBaslangicTarihi",
                table: "Uyeler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaketBitisTarihi",
                table: "Uyeler",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UyelikTuru",
                table: "Uyeler");

            migrationBuilder.DropColumn(
                name: "PaketBaslangicTarihi",
                table: "Uyeler");

            migrationBuilder.DropColumn(
                name: "PaketBitisTarihi",
                table: "Uyeler");
        }
    }
}

