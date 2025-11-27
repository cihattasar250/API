-- MySQL için Üyeler tablosundan gereksiz kolonları kaldırma scripti (Manuel versiyon)
-- Eğer DROP COLUMN IF EXISTS çalışmazsa bu versiyonu kullanın

USE deneme; -- Veritabanı adınızı buraya yazın

-- Önce hangi kolonların var olduğunu kontrol edin
-- DESCRIBE Uyeler; komutunu çalıştırın

-- AcilDurumIletisim kolonunu kaldır
-- ALTER TABLE Uyeler DROP COLUMN AcilDurumIletisim;

-- UyelikTuru kolonunu kaldır
-- ALTER TABLE Uyeler DROP COLUMN UyelikTuru;

-- UyelikUcreti kolonunu kaldır
-- ALTER TABLE Uyeler DROP COLUMN UyelikUcreti;

-- PaketBaslangicTarihi kolonunu kaldır
-- ALTER TABLE Uyeler DROP COLUMN PaketBaslangicTarihi;

-- PaketBitisTarihi kolonunu kaldır
-- ALTER TABLE Uyeler DROP COLUMN PaketBitisTarihi;

-- Yukarıdaki komutların başındaki -- işaretlerini kaldırıp sadece var olan kolonlar için çalıştırın

