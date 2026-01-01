# Turkish NLP Analyzer

A comprehensive Turkish word analysis system combining a Python FastAPI backend with a Windows Forms desktop application.

![Turkish NLP Analyzer](https://img.shields.io/badge/Turkish-NLP-blue)
![Python](https://img.shields.io/badge/Python-3.10+-green)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)

## 📋 Overview

Turkish NLP Analyzer is a dual-component system that provides:
- **Morphological Analysis**: Analyze Turkish words to extract root forms, POS tags, and grammatical features
- **Word Classification**: Organize words by Part of Speech (NOUN, VERB, ADJ, etc.)
- **Batch Processing**: Process CSV files containing thousands of words
- **Statistics Dashboard**: Visual representation of word distribution

## 🏗️ Architecture

```
tokenizer-filter-machine/
├── backend/                    # Python FastAPI
│   ├── main.py                 # API endpoints
│   ├── analyzer.py            # Stanza NLP wrapper
│   └── requirements.txt       # Python dependencies
├── TurkishNLP.Desktop/        # C# Windows Forms
│   ├── Forms/                 # UI Forms
│   ├── Models/                # Data models (OOP)
│   ├── Services/              # Business logic
│   └── Program.cs             # Entry point
└── database/
    └── words.db               # SQLite database
```

## 🚀 Quick Start

### Prerequisites
- Python 3.10+
- .NET 8.0 SDK
- DevExpress WinForms (Trial or Licensed)

### 1. Start Python Backend

```bash
cd backend
pip install -r requirements.txt
python -m uvicorn main:app --reload --port 8000
```

The API will be available at `http://localhost:8000`
- Swagger UI: `http://localhost:8000/docs`
- Health Check: `http://localhost:8000/health`

### 2. Run Desktop Application

```bash
cd TurkishNLP.Desktop
dotnet restore
dotnet run
```

Or open in Visual Studio and press F5.

## 📦 Dependencies

### Python Backend
```
fastapi>=0.109.0
uvicorn>=0.27.0
stanza>=1.7.0
pydantic>=2.6.0
```

### C# Desktop
```
DevExpress.WindowsForms
Microsoft.Data.Sqlite
System.Text.Json
```

## 🎯 Features

### Dashboard
- 9 colored tiles showing word count per POS
- Pie chart visualization
- Auto-refresh every 30 seconds

### Word Analysis
- Single word morphological analysis
- Shows: Root, POS, Morphological Features
- Save to database

### Batch Processing
- Import CSV files
- Progress tracking
- Bulk save to database

### Database Viewer
- Filter by POS
- Search functionality
- Export to JSON
- Delete selected

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| F5 | Refresh Dashboard |
| Ctrl+O | Open CSV File |
| Ctrl+S | Export to JSON |
| F1 | About Dialog |
| ESC | Cancel Batch |

## 🔧 Configuration

Edit `TurkishNLP.Desktop/appsettings.json`:

```json
{
  "ApiBaseUrl": "http://localhost:8000",
  "DatabasePath": "words.db",
  "Theme": "The Bezier"
}
```

## 📊 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /analyze | Analyze single word |
| POST | /analyze-batch | Analyze multiple words |
| GET | /health | Health check |

### Example Request
```bash
curl -X POST "http://localhost:8000/analyze" \
  -H "Content-Type: application/json" \
  -d '{"word": "gidiyorum"}'
```

### Example Response
```json
{
  "word": "gidiyorum",
  "root": "git",
  "pos": "VERB",
  "features": {
    "Aspect": "Prog",
    "Number": "Sing",
    "Person": "1",
    "Tense": "Pres"
  }
}
```

## 📁 Sample Data

### CSV Format
```
kelime
kitap
ev
gitmek
güzel
```

### JSON Export Format
```json
{
  "NOUN": ["kitap", "ev"],
  "VERB": ["gitmek"],
  "ADJ": ["güzel"]
}
```

## 🧪 Testing Checklist

- [ ] Backend health check works
- [ ] Single word analysis works
- [ ] Batch CSV processing works
- [ ] Database CRUD operations work
- [ ] JSON export/import works
- [ ] Dashboard statistics update

## 📄 License

MIT License

## 👥 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request