# 🇹🇷 Turkish NLP Analyzer & Dashboard

![Turkish NLP](https://img.shields.io/badge/Language-Turkish-red)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Python-blue)
![License](https://img.shields.io/badge/License-MIT-green)
![Status](https://img.shields.io/badge/Status-Active-success)

A powerful, dual-stack application designed for the morphological analysis, classification, and statistical visualization of Turkish words. It combines a robust **Python (FastAPI)** backend for NLP processing with a modern **.NET 8 (Windows Forms/DevExpress)** desktop interface.

---
![WhatsApp Image 2026-01-06 at 16 04 28](https://github.com/user-attachments/assets/f24d9f73-fcaf-42b5-a947-cf48ddd4dacb)

## 🌟 Key Features

### 🖥️ Modern Desktop Interface
- **Dashboard:** Real-time visualization of parsed data.
  - **Dynamic Pie Chart:** Custom GDI+ drawn chart with exploded slices and smart legends.
  - **KPI Cards:** Interactive, color-coded statistics cards (e.g., **Blue** for Nouns, **Green** for Verbs).
- **Theme Support:** 🌗 Fully integrated **Dark/Light** mode.
  - One-click toggle (🌙/☀️).
  - Smart coloring: High contrast text, adaptive backgrounds, and persistent color-coding for data inputs.
- **Grid Control:** Advanced data filtering, searching, and sorting capability using DevExpress Grid.

### 🧠 Intelligent Analysis (Backend)
- **NLP Engine:** Powered by Python ecosystem (e.g., `stanza` or custom lookup).
- **Morphological Parsing:** Detects Roots, suffixes, and Part-of-Speech (POS) tags.
- **REST API:** Fast and scalable communication via local HTTP/JSON endpoints.

### ⚡ Batch Processing
- **Bulk Import:** Process `.csv` or text files with thousands of words.
- **Progress Tracking:** Real-time feedback during analysis.
- **Database Integration:** Automatically archives results to a local SQLite database.

---

## 🏗️ System Architecture

The solution follows a clean **Client-Server** architecture:

```mermaid
graph LR
    A[Desktop UI (.NET 8)] <-->|HTTP JSON| B[Python Backend (FastAPI)]
    B <-->|Load/Process| C[NLP Models]
    A <-->|Read/Write| D[(SQLite Database)]
```

### 1. Backend (`/backend`)
- **Framework:** FastAPI (Uvicorn server).
- **Role:** Stateless compute engine. Receives words, analyzes them, and returns JSON objects with linguistic features.
- **Endpoints:**
  - `POST /analyze`: Single word analysis.
  - `POST /analyze-batch`: Bulk analysis.

### 2. Frontend (`/TurkishNLP.Desktop`)
- **Framework:** .NET 8, Windows Forms.
- **Libraries:** DevExpress (UI Controls), System.Text.Json, Microsoft.Data.Sqlite.
- **Role:** User interaction, Data presentation, Database management.
- **Custom Controls:**
  - `KpiCard`: Rounded, shadowed, interactive statistic blocks.
  - `PieChartPanel`: Custom drawing logic for high-performance charting.

---

## 🚀 Getting Started

### Prerequisites
1.  **Python 3.10+**: For the backend service.
2.  **.NET 8 SDK**: For the desktop application.
3.  **(Optional)** DevExpress License/Trial (Libraries are referenced locally or via NuGet).

### Step 1: Launch the Backend
The desktop app needs the backend to be running to perform analysis.

```bash
# Navigate to backend folder
cd backend

# Create virtual environment (Recommended)
python -m venv venv
.\venv\Scripts\activate  # Windows

# Install dependencies
pip install -r requirements.txt

# Start the server
uvicorn main:app --reload --port 8000
```
*You should see: `Uvicorn running on http://127.0.0.1:8000`*

### Step 2: Launch the Application
Open a new terminal for the desktop app.

```bash
# Navigate to Desktop app folder
cd TurkishNLP.Desktop

# Restore packages and run
dotnet restore
dotnet run
```

---

## 🎨 User Interface Guide

### Dashboard Tab
- **Overview:** Displays the distribution of words in your database (NOUN, VERB, ADJ, etc.).
- **Interactive Elements:** 
  - Hover over pie slices to see values.
  - Click the **Moon/Sun** icon top-right to switch themes.
  - **Color Codes:**
    - 🔵 **NOUN:** Blue
    - 🟢 **VERB:** Green
    - 🟠 **ADJ:** Orange
    - 🟣 **ADV:** Purple

### Word Analysis Tab
- **Input:** Type a Turkish word and press Enter.
- **Visual Result:** See the Breakdown (Root + Suffixes) and Metadata.
- **Action:** Click "Save" to add valid words to the localized database.

### Batch Processing Tab
- **Load File:** Import a list of words.
- **Process:** Watch as the system analyzes them via the API in real-time.
- **Save:** Commit valid results to the DB.

### Database Tab
- **Manage Data:** View all saved words.
- **Filter:** Show only specific types (e.g., "Only VERBs").
- **Search:** Instant text search.
- **Export:** Save your curated list to JSON.

---

## 🛠️ Development Notes

### Project Structure
- `TurkishNLP.Desktop/`
  - `Forms/MainForm.cs`: Core UI logic.
  - `Controls/`: Custom UI components (`KpiCard`).
  - `Services/`: HTTP Client and Database wrappers.
  - `Utils/ThemeManager.cs`: Centralized Color Palettes.
  - `Models/`: Data structures (`WordAnalysis`, `WordRoot`, etc.).

### Adding New POS Types
1.  Update the **Backend** logic to recognize the new tag.
2.  Update `WordRootFactory.cs` in Desktop to allow the tag.
3.  Add a generic color in `MainForm.cs` (Pie Chart logic) to visualize it.

---

## 📄 License
This project is open-source and available under the **MIT License**.
