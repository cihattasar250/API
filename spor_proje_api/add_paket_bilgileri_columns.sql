-- Paket bilgileri kolonlarını Uyeler tablosuna ekle
-- Migration: AddPaketBilgileriToUye

-- Kolonların var olup olmadığını kontrol et ve ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'UyelikTuru')
BEGIN
    ALTER TABLE [Uyeler] ADD [UyelikTuru] INT NULL;
    PRINT 'UyelikTuru kolonu eklendi.';
END
ELSE
BEGIN
    PRINT 'UyelikTuru kolonu zaten mevcut.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'PaketBaslangicTarihi')
BEGIN
    ALTER TABLE [Uyeler] ADD [PaketBaslangicTarihi] DATETIME2 NULL;
    PRINT 'PaketBaslangicTarihi kolonu eklendi.';
END
ELSE
BEGIN
    PRINT 'PaketBaslangicTarihi kolonu zaten mevcut.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'PaketBitisTarihi')
BEGIN
    ALTER TABLE [Uyeler] ADD [PaketBitisTarihi] DATETIME2 NULL;
    PRINT 'PaketBitisTarihi kolonu eklendi.';
END
ELSE
BEGIN
    PRINT 'PaketBitisTarihi kolonu zaten mevcut.';
END

PRINT 'Paket bilgileri kolonları kontrol edildi ve eklendi.';

