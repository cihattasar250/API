# 🎯 Antrenman, Beslenme ve Hedef Yönetimi HTML Entegrasyonu

## 📋 Genel Bakış

Bu dokümantasyon, **Antrenman Yönetimi**, **Beslenme Takibi** ve **Hedef Yönetimi** bölümlerinin HTML sayfasına nasıl entegre edildiğini açıklar.

## 🔗 API Bağlantıları

### 1. API URL'leri

```javascript
const API_ANTRENMAN_URL = 'http://localhost:7043/api/Antrenman';
const API_BESLENME_URL = 'http://localhost:7043/api/Beslenme';
const API_HEDEF_URL = 'http://localhost:7043/api/Hedef';
```

### 2. Token Yönetimi

```javascript
// Token localStorage'dan alınır
let authToken = localStorage.getItem('token');

// Her API isteğinde Authorization header'ı eklenir
headers: {
    'Authorization': `Bearer ${authToken}`,
    'Content-Type': 'application/json'
}
```

---

## 🏋️ Antrenman Yönetimi

### HTML Yapısı

```html
<!-- Antrenman Yönetimi Bölümü -->
<div class="antrenman-section">
    <h3>🏋️ Antrenman Yönetimi</h3>
    
    <!-- Form -->
    <form id="antrenmanForm">
        <input type="text" id="antrenmanAdi" placeholder="Antrenman Adı *" required>
        <textarea id="antrenmanAciklama" placeholder="Açıklama"></textarea>
        <input type="number" id="antrenmanSure" placeholder="Süre (dakika)">
        <select id="antrenmanTipi">
            <option value="Kardiyo">Kardiyo</option>
            <option value="Kuvvet">Kuvvet</option>
            <!-- ... -->
        </select>
        <input type="date" id="antrenmanTarihi">
        <button type="submit">Antrenman Ekle</button>
    </form>
    
    <!-- Liste -->
    <div id="antrenmanListesi"></div>
</div>
```

### JavaScript Fonksiyonları

#### 1. Antrenman Ekleme

```javascript
async function addAntrenman() {
    // 1. Token kontrolü
    if (!authToken) {
        showMessage('Lütfen önce giriş yapın!', 'error');
        return;
    }
    
    // 2. Form verilerini al
    const antrenman = {
        AntrenmanAdi: document.getElementById('antrenmanAdi').value,
        Aciklama: document.getElementById('antrenmanAciklama').value,
        Sure: parseInt(document.getElementById('antrenmanSure').value) || null,
        AntrenmanTipi: document.getElementById('antrenmanTipi').value || null,
        Tarih: document.getElementById('antrenmanTarihi').value || new Date().toISOString().split('T')[0]
    };
    
    // 3. API isteği
    const response = await fetch(API_ANTRENMAN_URL, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${authToken}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(antrenman)
    });
    
    // 4. Sonuç işleme
    if (response.ok) {
        showMessage('✅ Antrenman başarıyla eklendi!', 'success');
        loadAntrenmanlar(); // Listeyi yenile
    } else {
        const error = await response.json();
        showMessage('❌ Hata: ' + error.message, 'error');
    }
}
```

#### 2. Antrenmanları Listeleme

```javascript
async function loadAntrenmanlar() {
    const response = await fetch(`${API_ANTRENMAN_URL}/Panel`, {
        headers: {
            'Authorization': `Bearer ${authToken}`
        }
    });
    
    if (response.ok) {
        const antrenmanlar = await response.json();
        renderAntrenmanListesi(antrenmanlar);
    }
}
```

#### 3. Form Event Listener

```javascript
document.getElementById('antrenmanForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    await addAntrenman();
});
```

---

## 🍎 Beslenme Takibi

### HTML Yapısı

```html
<!-- Beslenme Takibi Bölümü -->
<div class="beslenme-section">
    <h3>🍎 Beslenme Takibi</h3>
    
    <!-- Form -->
    <form id="beslenmeForm">
        <input type="text" id="yemekAdi" placeholder="Yemek Adı *" required>
        <input type="number" id="kalori" placeholder="Kalori">
        <input type="number" id="protein" placeholder="Protein (g)">
        <input type="number" id="karbonhidrat" placeholder="Karbonhidrat (g)">
        <input type="number" id="yag" placeholder="Yağ (g)">
        <select id="ogun">
            <option value="Sabah">Sabah</option>
            <option value="Öğle">Öğle</option>
            <option value="Akşam">Akşam</option>
            <option value="Ara Öğün">Ara Öğün</option>
        </select>
        <input type="date" id="beslenmeTarihi">
        <button type="submit">Beslenme Ekle</button>
    </form>
    
    <!-- Liste -->
    <div id="beslenmeListesi"></div>
</div>
```

### JavaScript Fonksiyonları

```javascript
async function addBeslenme() {
    const beslenme = {
        YemekAdi: document.getElementById('yemekAdi').value,
        Kalori: parseFloat(document.getElementById('kalori').value) || null,
        Protein: parseFloat(document.getElementById('protein').value) || null,
        Karbonhidrat: parseFloat(document.getElementById('karbonhidrat').value) || null,
        Yag: parseFloat(document.getElementById('yag').value) || null,
        Ogun: document.getElementById('ogun').value || null,
        Tarih: document.getElementById('beslenmeTarihi').value || new Date().toISOString().split('T')[0]
    };
    
    const response = await fetch(API_BESLENME_URL, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${authToken}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(beslenme)
    });
    
    if (response.ok) {
        showMessage('✅ Beslenme kaydı başarıyla eklendi!', 'success');
        loadBeslenme();
    }
}

async function loadBeslenme() {
    const response = await fetch(`${API_BESLENME_URL}/Panel`, {
        headers: {
            'Authorization': `Bearer ${authToken}`
        }
    });
    
    if (response.ok) {
        const beslenme = await response.json();
        renderBeslenmeListesi(beslenme);
    }
}
```

---

## 🎯 Hedef Yönetimi

### HTML Yapısı

```html
<!-- Hedef Yönetimi Bölümü -->
<div class="hedef-section">
    <h3>🎯 Hedef Yönetimi</h3>
    
    <!-- Form -->
    <form id="hedefForm">
        <input type="text" id="hedefAdi" placeholder="Hedef Adı *" required>
        <textarea id="hedefAciklama" placeholder="Açıklama"></textarea>
        <input type="date" id="hedefTarihi" placeholder="Hedef Tarihi">
        <input type="date" id="hedefBaslangicTarihi" placeholder="Başlangıç Tarihi">
        <select id="hedefKategori">
            <option value="">Kategori Seçin</option>
            <option value="Kilo">Kilo</option>
            <option value="Performans">Performans</option>
            <option value="Beslenme">Beslenme</option>
        </select>
        <input type="number" id="hedefDeger" placeholder="Hedef Değer">
        <input type="text" id="hedefBirim" placeholder="Birim (kg, cm, vb.)">
        <button type="submit">Hedef Ekle</button>
    </form>
    
    <!-- Liste -->
    <div id="hedefListesi"></div>
</div>
```

### JavaScript Fonksiyonları

```javascript
async function addHedef() {
    const hedef = {
        HedefAdi: document.getElementById('hedefAdi').value,
        Aciklama: document.getElementById('hedefAciklama').value || null,
        HedefTarihi: document.getElementById('hedefTarihi').value || null,
        BaslangicTarihi: document.getElementById('hedefBaslangicTarihi').value || null,
        Kategori: document.getElementById('hedefKategori').value || null,
        HedefDeger: parseFloat(document.getElementById('hedefDeger').value) || null,
        Birim: document.getElementById('hedefBirim').value || null
    };
    
    const response = await fetch(API_HEDEF_URL, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${authToken}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(hedef)
    });
    
    if (response.ok) {
        showMessage('✅ Hedef başarıyla eklendi!', 'success');
        loadHedefler();
    }
}

async function loadHedefler() {
    const response = await fetch(`${API_HEDEF_URL}/Panel`, {
        headers: {
            'Authorization': `Bearer ${authToken}`
        }
    });
    
    if (response.ok) {
        const hedefler = await response.json();
        renderHedefListesi(hedefler);
    }
}
```

---

## 🔄 Sayfa Yüklendiğinde Otomatik Çalıştırma

```javascript
document.addEventListener('DOMContentLoaded', function() {
    // Token kontrolü
    authToken = localStorage.getItem('token');
    
    if (!authToken) {
        window.location.href = 'uye_giris.html';
        return;
    }
    
    // Tüm listeleri yükle
    loadAntrenmanlar();
    loadBeslenme();
    loadHedefler();
    
    // Form event listener'ları ekle
    document.getElementById('antrenmanForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        await addAntrenman();
    });
    
    document.getElementById('beslenmeForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        await addBeslenme();
    });
    
    document.getElementById('hedefForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        await addHedef();
    });
});
```

---

## 📝 Önemli Notlar

### 1. Backend Model Uyumu

- **Antrenman**: `AntrenmanAdi`, `Aciklama`, `Sure`, `AntrenmanTipi`, `Tarih`
- **Beslenme**: `YemekAdi`, `Kalori`, `Protein`, `Karbonhidrat`, `Yag`, `Ogun`, `Tarih`
- **Hedef**: `HedefAdi`, `Aciklama`, `HedefTarihi`, `BaslangicTarihi`, `Kategori`, `HedefDeger`, `Birim`

### 2. Tarih Formatı

```javascript
// Tarihler YYYY-MM-DD formatında gönderilmeli
const tarih = document.getElementById('tarih').value; // "2025-01-15"
```

### 3. Hata Yönetimi

```javascript
try {
    const response = await fetch(url, options);
    
    if (!response.ok) {
        if (response.status === 401) {
            // Token süresi dolmuş
            localStorage.removeItem('token');
            window.location.href = 'uye_giris.html';
            return;
        }
        
        const error = await response.json();
        showMessage('❌ Hata: ' + (error.message || 'Bilinmeyen hata'), 'error');
        return;
    }
    
    const data = await response.json();
    // Başarılı işlem
    
} catch (error) {
    console.error('API Hatası:', error);
    showMessage('❌ Bağlantı hatası! Lütfen tekrar deneyin.', 'error');
}
```

### 4. Mesaj Gösterme

```javascript
function showMessage(message, type) {
    const messageDiv = document.getElementById('message');
    messageDiv.textContent = message;
    messageDiv.className = `message ${type}`;
    messageDiv.style.display = 'block';
    
    setTimeout(() => {
        messageDiv.style.display = 'none';
    }, 5000);
}
```

---

## 🚀 Hızlı Başlangıç

1. **HTML'e bölümleri ekleyin** (yukarıdaki HTML yapılarını kullanın)
2. **JavaScript fonksiyonlarını ekleyin** (yukarıdaki fonksiyonları kopyalayın)
3. **Event listener'ları ekleyin** (form submit olayları)
4. **Sayfa yüklendiğinde listeleri yükleyin** (`DOMContentLoaded` event'inde)

---

## ✅ Test Etme

1. Backend API'nin çalıştığından emin olun (`http://localhost:7043`)
2. Üye girişi yapın ve token alın
3. Her bölümde form doldurup "Ekle" butonuna tıklayın
4. Listelerin otomatik yüklendiğini kontrol edin
5. Browser console'u açın (F12) ve hataları kontrol edin

---

## 🔧 Sorun Giderme

### Token Bulunamadı
- `localStorage.getItem('token')` kontrol edin
- Giriş sayfasından tekrar giriş yapın

### 404 Not Found
- Backend API'nin çalıştığından emin olun
- API URL'lerini kontrol edin

### 401 Unauthorized
- Token süresi dolmuş olabilir
- Tekrar giriş yapın

### Veriler Kaydedilmiyor
- Browser console'u açın ve hataları kontrol edin
- Backend loglarını kontrol edin
- Form verilerinin doğru formatlandığından emin olun

