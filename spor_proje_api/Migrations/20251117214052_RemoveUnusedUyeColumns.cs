using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace spor_proje_api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedUyeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AcilDurumIletisim kolonunu kaldır
            migrationBuilder.DropColumn(
                name: "AcilDurumIletisim",
                table: "Uyeler");

            // UyelikTuru kolonunu kaldır
            migrationBuilder.DropColumn(
                name: "UyelikTuru",
                table: "Uyeler");

            // UyelikUcreti kolonunu kaldır (eğer varsa)
            migrationBuilder.DropColumn(
                name: "UyelikUcreti",
                table: "Uyeler");

            // PaketBaslangicTarihi kolonunu kaldır (eğer varsa)
            migrationBuilder.DropColumn(
                name: "PaketBaslangicTarihi",
                table: "Uyeler");

            // PaketBitisTarihi kolonunu kaldır (eğer varsa)
            migrationBuilder.DropColumn(
                name: "PaketBitisTarihi",
                table: "Uyeler");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma işlemleri
            migrationBuilder.AddColumn<string>(
                name: "AcilDurumIletisim",
                table: "Uyeler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UyelikTuru",
                table: "Uyeler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UyelikUcreti",
                table: "Uyeler",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

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
    }
}

