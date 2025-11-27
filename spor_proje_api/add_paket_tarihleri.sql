-- Paket başlangıç ve bitiş tarihleri kolonlarını ekle
-- Migration: AddPaketTarihleri

ALTER TABLE [Uyeler]
ADD [PaketBaslangicTarihi] DATETIME2 NULL,
    [PaketBitisTarihi] DATETIME2 NULL;

