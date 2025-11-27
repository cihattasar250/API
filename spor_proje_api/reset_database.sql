-- Veritabanındaki tüm verileri sıfırlama scripti
-- Bu script tüm tablolardaki verileri siler ve ID'leri sıfırlar

-- Foreign key kısıtlamalarını geçici olarak devre dışı bırak
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

-- Tüm tablolardaki verileri sil
DELETE FROM PerformansTakibi;
DELETE FROM Hedefler;
DELETE FROM BeslenmeKayitlari;
DELETE FROM Performanslar;
DELETE FROM Antrenmanlar;
DELETE FROM Sporcular;
DELETE FROM Uyeler;
DELETE FROM Adminler;

-- IDENTITY sütunlarını sıfırla (ID'leri 1'den başlat)
DBCC CHECKIDENT ('Adminler', RESEED, 0);
DBCC CHECKIDENT ('Uyeler', RESEED, 0);
DBCC CHECKIDENT ('Sporcular', RESEED, 0);
DBCC CHECKIDENT ('Antrenmanlar', RESEED, 0);
DBCC CHECKIDENT ('Performanslar', RESEED, 0);
DBCC CHECKIDENT ('BeslenmeKayitlari', RESEED, 0);
DBCC CHECKIDENT ('Hedefler', RESEED, 0);
DBCC CHECKIDENT ('PerformansTakibi', RESEED, 0);

-- Foreign key kısıtlamalarını tekrar etkinleştir
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"

PRINT 'Veritabanı başarıyla sıfırlandı! Tüm veriler silindi ve ID''ler sıfırlandı.';
