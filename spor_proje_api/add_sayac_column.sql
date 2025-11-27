-- Sayac kolonunu Uyeler tablosuna ekle
-- Migration: AddSayacToUye

-- Eğer kolon zaten varsa hata vermemesi için kontrol et
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'Sayac')
BEGIN
    ALTER TABLE [Uyeler]
    ADD [Sayac] BIGINT NOT NULL DEFAULT 0;
    
    PRINT 'Sayac kolonu başarıyla eklendi.';
END
ELSE
BEGIN
    PRINT 'Sayac kolonu zaten mevcut.';
END
GO

