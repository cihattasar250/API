-- MySQL için Üyeler tablosundan gereksiz kolonları kaldırma scripti (Güvenli versiyon)
-- Bu scripti MySQL Workbench veya MySQL komut satırında çalıştırın

-- ÖNEMLİ: Önce veritabanı adınızı kontrol edin ve gerekirse değiştirin
USE deneme; -- Veritabanı adınızı buraya yazın

-- Önce mevcut kolonları görmek için:
-- DESCRIBE Uyeler;
-- veya
-- SHOW COLUMNS FROM Uyeler;

-- Aşağıdaki komutları sadece var olan kolonlar için çalıştırın
-- Her komutu tek tek çalıştırıp hata alırsanız o kolon zaten yok demektir, devam edin

-- 1. AcilDurumIletisim kolonunu kaldır
ALTER TABLE Uyeler DROP COLUMN AcilDurumIletisim;

-- 2. UyelikTuru kolonunu kaldır
ALTER TABLE Uyeler DROP COLUMN UyelikTuru;

-- 3. UyelikUcreti kolonunu kaldır
ALTER TABLE Uyeler DROP COLUMN UyelikUcreti;

-- 4. PaketBaslangicTarihi kolonunu kaldır
ALTER TABLE Uyeler DROP COLUMN PaketBaslangicTarihi;

-- 5. PaketBitisTarihi kolonunu kaldır
ALTER TABLE Uyeler DROP COLUMN PaketBitisTarihi;

-- Sonuç kontrolü - Tablodaki kolonları göster
DESCRIBE Uyeler;

-- Beklenen kolonlar:
-- Id, Ad, Soyad, Email, Telefon, DogumTarihi, Cinsiyet, Adres, UyeNumarasi, Sifre, KayitTarihi, Aktif, SonGirisTarihi

