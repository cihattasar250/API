# Ücret Artış API'si HTML Entegrasyonu

## API Endpoint'leri:
- **Base URL**: `http://localhost:7043/api/Ucret`
- **Ücret Artış Nedenleri**: `GET /artis-nedenleri`
- **Mevcut Ücret**: `GET /uye/{uyeId}/mevcut`
- **Ücret Güncelle**: `POST /guncelle`
- **Ücret Geçmişi**: `GET /uye/{uyeId}`

## HTML Form Örneği:

```html
<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Ücret Güncelleme</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .form-container { max-width: 600px; margin: 0 auto; }
        .form-group { margin-bottom: 15px; }
        label { display: block; margin-bottom: 5px; font-weight: bold; }
        input, select, textarea { width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px; }
        button { background: linear-gradient(45deg, #007bff, #6f42c1); color: white; padding: 10px 20px; border: none; border-radius: 4px; cursor: pointer; }
        button:hover { opacity: 0.9; }
        .current-fee { background: #f8f9fa; padding: 10px; border-radius: 4px; margin-bottom: 15px; }
        .error { color: red; margin-top: 5px; }
        .success { color: green; margin-top: 5px; }
    </style>
</head>
<body>
    <div class="form-container">
        <h2>Ücret Güncelleme Formu</h2>
        
        <!-- Mevcut Ücret Gösterimi -->
        <div class="form-group">
            <label>Mevcut Aylık Ücret:</label>
            <div class="current-fee" id="currentFee">Yükleniyor...</div>
        </div>

        <form id="feeUpdateForm">
            <!-- Üye ID -->
            <div class="form-group">
                <label for="uyeId">Üye ID:</label>
                <input type="number" id="uyeId" name="uyeId" value="1" required>
            </div>

            <!-- Yeni Ücret -->
            <div class="form-group">
                <label for="yeniUcret">Yeni Aylık Ücret (₺):</label>
                <input type="number" id="yeniUcret" name="yeniUcret" step="0.01" min="0" required>
            </div>

            <!-- Artış Nedeni -->
            <div class="form-group">
                <label for="artisNedeni">Ücret Artış Nedeni:</label>
                <select id="artisNedeni" name="artisNedeni" required>
                    <option value="">Seçiniz...</option>
                </select>
            </div>

            <!-- Açıklama -->
            <div class="form-group">
                <label for="aciklama">Açıklama:</label>
                <textarea id="aciklama" name="aciklama" rows="4" placeholder="Ücret artışı hakkında detaylı açıklama..."></textarea>
            </div>

            <!-- Geçerlilik Tarihi -->
            <div class="form-group">
                <label for="gecerlilikTarihi">Geçerlilik Tarihi:</label>
                <input type="date" id="gecerlilikTarihi" name="gecerlilikTarihi" required>
            </div>

            <!-- Submit Button -->
            <button type="submit">Ücreti Güncelle</button>
        </form>

        <!-- Sonuç Mesajları -->
        <div id="message"></div>

        <!-- Ücret Geçmişi -->
        <div id="feeHistory" style="margin-top: 30px;">
            <h3>Ücret Geçmişi</h3>
            <div id="historyContent">Yükleniyor...</div>
        </div>
    </div>

    <script>
        const API_BASE_URL = 'http://localhost:7043/api/Ucret';
        let currentUyeId = 1;

        // Sayfa yüklendiğinde çalışacak fonksiyonlar
        document.addEventListener('DOMContentLoaded', function() {
            loadArtisNedenleri();
            loadCurrentFee();
            loadFeeHistory();
            
            // Geçerlilik tarihini bugünden itibaren ayarla
            const today = new Date().toISOString().split('T')[0];
            document.getElementById('gecerlilikTarihi').value = today;
        });

        // Ücret artış nedenlerini yükle
        async function loadArtisNedenleri() {
            try {
                const response = await fetch(`${API_BASE_URL}/artis-nedenleri`);
                const nedenler = await response.json();
                
                const select = document.getElementById('artisNedeni');
                nedenler.forEach(neden => {
                    const option = document.createElement('option');
                    option.value = neden;
                    option.textContent = neden;
                    select.appendChild(option);
                });
            } catch (error) {
                console.error('Artış nedenleri yüklenemedi:', error);
                showMessage('Artış nedenleri yüklenemedi', 'error');
            }
        }

        // Mevcut ücreti yükle
        async function loadCurrentFee() {
            try {
                const uyeId = document.getElementById('uyeId').value;
                const response = await fetch(`${API_BASE_URL}/uye/${uyeId}/mevcut`);
                const ucret = await response.json();
                
                document.getElementById('currentFee').textContent = `${ucret} ₺`;
            } catch (error) {
                console.error('Mevcut ücret yüklenemedi:', error);
                document.getElementById('currentFee').textContent = 'Yüklenemedi';
            }
        }

        // Ücret geçmişini yükle
        async function loadFeeHistory() {
            try {
                const uyeId = document.getElementById('uyeId').value;
                const response = await fetch(`${API_BASE_URL}/uye/${uyeId}`);
                const gecmis = await response.json();
                
                const historyContent = document.getElementById('historyContent');
                if (gecmis.length === 0) {
                    historyContent.innerHTML = '<p>Henüz ücret güncellemesi yapılmamış.</p>';
                    return;
                }

                let html = '<table border="1" style="width: 100%; border-collapse: collapse;">';
                html += '<tr><th>Tarih</th><th>Eski Ücret</th><th>Yeni Ücret</th><th>Neden</th><th>Açıklama</th></tr>';
                
                gecmis.forEach(kayit => {
                    html += `<tr>
                        <td>${new Date(kayit.guncellemeTarihi).toLocaleDateString('tr-TR')}</td>
                        <td>${kayit.eskiUcret} ₺</td>
                        <td>${kayit.yeniUcret} ₺</td>
                        <td>${kayit.artisNedeni}</td>
                        <td>${kayit.aciklama || '-'}</td>
                    </tr>`;
                });
                
                html += '</table>';
                historyContent.innerHTML = html;
            } catch (error) {
                console.error('Ücret geçmişi yüklenemedi:', error);
                document.getElementById('historyContent').textContent = 'Yüklenemedi';
            }
        }

        // Form submit işlemi
        document.getElementById('feeUpdateForm').addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const formData = {
                uyeId: parseInt(document.getElementById('uyeId').value),
                yeniUcret: parseFloat(document.getElementById('yeniUcret').value),
                artisNedeni: document.getElementById('artisNedeni').value,
                aciklama: document.getElementById('aciklama').value,
                gecerlilikTarihi: document.getElementById('gecerlilikTarihi').value + 'T00:00:00'
            };

            try {
                const response = await fetch(`${API_BASE_URL}/guncelle`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(formData)
                });

                if (response.ok) {
                    const result = await response.json();
                    showMessage('Ücret başarıyla güncellendi!', 'success');
                    
                    // Formu temizle ve verileri yenile
                    document.getElementById('feeUpdateForm').reset();
                    document.getElementById('gecerlilikTarihi').value = new Date().toISOString().split('T')[0];
                    
                    // Mevcut ücreti ve geçmişi yenile
                    loadCurrentFee();
                    loadFeeHistory();
                } else {
                    const error = await response.text();
                    showMessage(`Hata: ${error}`, 'error');
                }
            } catch (error) {
                console.error('Ücret güncelleme hatası:', error);
                showMessage('Ücret güncellenirken bir hata oluştu', 'error');
            }
        });

        // Üye ID değiştiğinde verileri yenile
        document.getElementById('uyeId').addEventListener('change', function() {
            currentUyeId = this.value;
            loadCurrentFee();
            loadFeeHistory();
        });

        // Mesaj gösterme fonksiyonu
        function showMessage(message, type) {
            const messageDiv = document.getElementById('message');
            messageDiv.textContent = message;
            messageDiv.className = type;
            
            // 5 saniye sonra mesajı temizle
            setTimeout(() => {
                messageDiv.textContent = '';
                messageDiv.className = '';
            }, 5000);
        }
    </script>
</body>
</html>
```

## JavaScript Fetch API Örnekleri:

### 1. Ücret Artış Nedenlerini Getir:
```javascript
fetch('http://localhost:7043/api/Ucret/artis-nedenleri')
    .then(response => response.json())
    .then(data => console.log(data));
```

### 2. Mevcut Ücreti Getir:
```javascript
fetch('http://localhost:7043/api/Ucret/uye/1/mevcut')
    .then(response => response.json())
    .then(ucret => console.log(ucret));
```

### 3. Ücret Güncelle:
```javascript
const updateData = {
    uyeId: 1,
    yeniUcret: 150.00,
    artisNedeni: "Ek Hizmet Eklendi",
    aciklama: "Yeni fitness programı eklendi",
    gecerlilikTarihi: "2025-01-01T00:00:00"
};

fetch('http://localhost:7043/api/Ucret/guncelle', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
    },
    body: JSON.stringify(updateData)
})
.then(response => response.json())
.then(result => console.log(result));
```

### 4. Ücret Geçmişini Getir:
```javascript
fetch('http://localhost:7043/api/Ucret/uye/1')
    .then(response => response.json())
    .then(gecmis => console.log(gecmis));
```

## CORS Ayarları:
API'de CORS zaten aktif (`AllowAll` policy). HTML dosyasını doğrudan tarayıcıda açabilirsiniz.

## Test Verileri:
- **Üye ID**: 1
- **Mevcut Ücret**: 100₺
- **Test Ücreti**: 150₺, 200₺ gibi değerler deneyebilirsiniz

Bu HTML dosyasını kaydedip tarayıcıda açarak ücret güncelleme API'sini test edebilirsiniz!
