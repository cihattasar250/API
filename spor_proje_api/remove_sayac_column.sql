-- Sayac kolonunu Uyeler tablosundan kaldır
-- Migration: RemoveSayacFromUye

-- Kolonun var olup olmadığını kontrol et ve kaldır
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'Sayac')
BEGIN
    ALTER TABLE [Uyeler] DROP COLUMN [Sayac];
    PRINT 'Sayac kolonu başarıyla kaldırıldı.';
END
ELSE
BEGIN
    PRINT 'Sayac kolonu zaten mevcut değil.';
END

