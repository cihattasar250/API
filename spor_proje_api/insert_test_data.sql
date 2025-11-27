-- Test verilerini eklemek için SQL script
-- Admin verileri
INSERT INTO Adminler (Ad, Soyad, Email, Telefon, AdminNumarasi, Sifre, KayitTarihi, Aktif) VALUES
('Admin', 'User', 'admin@sporproje.com', '0555-000-0001', 'ADMIN001', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMeRf7uFK8MjaIkhSg==', GETDATE(), 1),
('Sistem', 'Yöneticisi', 'sistem@sporproje.com', '0555-000-0002', 'ADMIN002', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMeRf7uFK8MjaIkhSg==', GETDATE(), 1);

-- Sporcu verileri
INSERT INTO Sporcular (Ad, Soyad, SporDali, Yas, Email, Telefon, KayitTarihi, Aktif) VALUES
('Ahmet', 'Yılmaz', 'Futbol', 25, 'ahmet.yilmaz@email.com', '0532-123-4567', GETDATE(), 1),
('Ayşe', 'Demir', 'Basketbol', 22, 'ayse.demir@email.com', '0533-234-5678', GETDATE(), 1),
('Mehmet', 'Kaya', 'Yüzme', 28, 'mehmet.kaya@email.com', '0534-345-6789', GETDATE(), 1);

-- Antrenman verileri
INSERT INTO Antrenmanlar (SporcuId, AntrenmanAdi, Aciklama, Tarih, Sure, AntrenmanTipi, KaloriYakilan, KalpAtisHizi, Notlar) VALUES
(1, 'Koşu Antrenmanı', '30 dakika tempolu koşu', GETDATE(), 30, 'Kardiyo', 300, 150, 'İyi performans'),
(1, 'Futbol Antrenmanı', 'Teknik çalışma', GETDATE(), 90, 'Teknik', 500, 140, 'Top kontrolü geliştirildi'),
(2, 'Basketbol Antrenmanı', 'Şut çalışması', GETDATE(), 60, 'Teknik', 400, 130, 'Şut yüzdesi arttı');

-- Performans verileri
INSERT INTO Performanslar (SporcuId, PerformansAdi, Tarih, Deger, Birim, Aciklama, Kategori) VALUES
(1, '100m Koşu', GETDATE(), 12.5, 'saniye', 'Kişisel rekor', 'Hız'),
(2, 'Serbest Atış', GETDATE(), 85, 'yüzde', '10 atıştan 8.5u başarılı', 'Şut'),
(3, '50m Serbest', GETDATE(), 25.8, 'saniye', 'Yeni kişisel rekor', 'Hız');

-- Beslenme verileri
INSERT INTO BeslenmeKayitlari (SporcuId, YemekAdi, Tarih, Ogun, Kalori, Protein, Karbonhidrat, Yag, Aciklama) VALUES
(1, 'Tavuk Göğsü', GETDATE(), 'Öğle', 250, 30, 0, 5, 'Izgara tavuk göğsü'),
(1, 'Pirinç Pilavı', GETDATE(), 'Öğle', 200, 4, 45, 1, 'Beyaz pirinç'),
(2, 'Yulaf Ezmesi', GETDATE(), 'Kahvaltı', 300, 10, 50, 6, 'Muz ve bal ile');

-- Hedef verileri
INSERT INTO Hedefler (SporcuId, HedefAdi, Aciklama, BaslangicTarihi, HedefTarihi, Tamamlandi, Kategori, HedefDeger, Birim) VALUES
(1, '100m Koşu Süresini İyileştir', '100m koşu süresini 12 saniyenin altına düşür', GETDATE(), DATEADD(day, 30, GETDATE()), 0, 'Performans', 12, 'saniye'),
(2, 'Serbest Atış Yüzdesini Artır', 'Serbest atış yüzdesini %90a çıkar', GETDATE(), DATEADD(day, 40, GETDATE()), 0, 'Şut', 90, 'yüzde'),
(3, '50m Serbest Rekoru', '50m serbest stil rekorunu kır', GETDATE(), DATEADD(day, 45, GETDATE()), 1, 'Rekor', 25, 'saniye');
