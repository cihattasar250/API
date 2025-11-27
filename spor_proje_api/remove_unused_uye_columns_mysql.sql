-- MySQL için Üyeler tablosundan gereksiz kolonları kaldırma scripti
-- Bu scripti MySQL'de çalıştırın

USE deneme; -- Veritabanı adınızı buraya yazın

-- AcilDurumIletisim kolonunu kaldır (eğer varsa)
ALTER TABLE Uyeler 
DROP COLUMN IF EXISTS AcilDurumIletisim;

-- UyelikTuru kolonunu kaldır (eğer varsa)
ALTER TABLE Uyeler 
DROP COLUMN IF EXISTS UyelikTuru;

-- UyelikUcreti kolonunu kaldır (eğer varsa)
ALTER TABLE Uyeler 
DROP COLUMN IF EXISTS UyelikUcreti;

-- PaketBaslangicTarihi kolonunu kaldır (eğer varsa)
ALTER TABLE Uyeler 
DROP COLUMN IF EXISTS PaketBaslangicTarihi;

-- PaketBitisTarihi kolonunu kaldır (eğer varsa)
ALTER TABLE Uyeler 
DROP COLUMN IF EXISTS PaketBitisTarihi;

-- Sonuç kontrolü - Tablodaki kolonları göster
DESCRIBE Uyeler;

