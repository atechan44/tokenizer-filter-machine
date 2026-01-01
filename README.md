# TurkMorph - Türkçe Morfolojik Analiz

## 📋 Proje Hakkında

**TurkMorph**, Türkçe metinlerin morfolojik analizini yapan, OOP prensiplerini ve modern yazılım mimarisini kullanan bir masaüstü uygulamasıdır.

### 🎯 Öğrenilen Kavramlar

| Kavram | Uygulama |
|--------|----------|
| **Abstract Class** | `WordRoot.cs` - Soyut temel sınıf |
| **Inheritance** | `NounRoot`, `VerbRoot`, `AdjectiveRoot` türetilmiş sınıflar |
| **Polymorphism** | Her sınıfın kendi `Validate()` implementasyonu |
| **Interface** | `IWordValidator`, `INlpService` |
| **Factory Pattern** | `WordFactory.cs` - Nesne oluşturma fabrikası |
| **Repository Pattern** | `WordRootRepository.cs` - Veri erişim katmanı |
| **Async/Await** | Non-blocking API çağrıları |
| **Microservice** | Python NLP API + C# Client mimarisi |

---

## 🏗️ Mimari (5 Katman)

```
┌─────────────────────────────────────────┐
│     Layer 5: UI (DevExpress WinForms)   │
├─────────────────────────────────────────┤
│     Layer 4: Service (HTTP Client)      │
├─────────────────────────────────────────┤
│     Layer 3: Core OOP (Models)          │
├─────────────────────────────────────────┤
│     Layer 2: Database (SQLite/Dapper)   │
├─────────────────────────────────────────┤
│     Layer 1: Backend (Python/Stanza)    │
└─────────────────────────────────────────┘
```

---

## 📂 Proje Yapısı

```
tokenizer-filter-machine/
├── backend/
│   ├── main.py              # FastAPI + Stanza NLP API
│   └── requirements.txt     # Python bağımlılıkları
│
├── TurkMorph/
│   ├── TurkMorph.csproj     # Proje dosyası
│   ├── Program.cs           # Giriş noktası
│   │
│   ├── Models/              # OOP Sınıfları
│   │   ├── WordRoot.cs      # Abstract Class
│   │   ├── NounRoot.cs      # Inheritance
│   │   ├── VerbRoot.cs      # Polymorphism
│   │   └── AdjectiveRoot.cs
│   │
│   ├── Interfaces/
│   │   ├── IWordValidator.cs
│   │   └── INlpService.cs
│   │
│   ├── Services/
│   │   ├── NlpApiService.cs # HTTP Client
│   │   ├── WordFactory.cs   # Factory Pattern
│   │   └── DTOs/
│   │       └── AnalysisResult.cs
│   │
│   ├── Database/
│   │   ├── TurkMorphContext.cs
│   │   └── Repositories/
│   │       └── WordRootRepository.cs
│   │
│   └── Forms/
│       └── MainForm.cs      # DevExpress UI
│
└── turkmorph.db             # SQLite (runtime)
```

---

## 🚀 Kurulum ve Çalıştırma

### 1. Python Backend Kurulumu

```powershell
cd backend
pip install -r requirements.txt
```

### 2. Python Backend Başlatma

```powershell
python main.py
```

> **Not:** İlk çalıştırmada Stanza Türkçe modeli (~200MB) indirilecektir.

### 3. C# Projesi Derleme

```powershell
cd TurkMorph
dotnet restore
dotnet build
```

### 4. Uygulamayı Çalıştırma

```powershell
dotnet run
```

Veya Visual Studio'dan `TurkMorph.csproj` dosyasını açın ve F5 tuşuna basın.

---

## 📷 Kullanım

1. **Python API'yi başlatın** (`python backend/main.py`)
2. **C# uygulamasını çalıştırın**
3. **Metin girin** (örn: "Kitapları okudum ve çok beğendim")
4. **"Analiz Et" butonuna basın**
5. Grid'de kelimelerin kökü, türü ve özellikleri görüntülenir
6. **"Veritabanına Kaydet"** ile sonuçları SQLite'a kaydedin

---

## 🔧 API Endpoints

| Endpoint | Method | Açıklama |
|----------|--------|----------|
| `/health` | GET | API sağlık kontrolü |
| `/analyze/word` | POST | Kelime analizi |
| `/clean` | POST | Metin temizleme |
| `/batch/analyze` | POST | Toplu analiz |

### Örnek İstek

```bash
curl -X POST http://127.0.0.1:8000/analyze/word \
  -H "Content-Type: application/json" \
  -d '{"text": "kitapları okudum"}'
```

### Örnek Yanıt

```json
[
  {"word": "kitapları", "lemma": "kitap", "pos": "NOUN", "feats": "Case=Acc|Number=Plur"},
  {"word": "okudum", "lemma": "oku", "pos": "VERB", "feats": "Tense=Past|Person=1"}
]
```

---

## 📚 Teknoloji Stack

| Teknoloji | Kullanım Amacı |
|-----------|----------------|
| **Python 3.8+** | NLP Backend |
| **FastAPI** | REST API Framework |
| **Stanza** | Türkçe NLP (Stanford) |
| **.NET 8** | C# Runtime |
| **DevExpress** | Modern WinForms UI |
| **Dapper** | Micro ORM |
| **SQLite** | Veritabanı |

---

## 📝 Lisans

Bu proje eğitim amaçlı hazırlanmıştır.