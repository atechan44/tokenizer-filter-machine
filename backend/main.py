"""
TurkMorph NLP API - FastAPI + Stanza Backend
Türkçe doğal dil işleme motoru
"""

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import stanza
import uvicorn
from typing import List, Optional
import re

# --- Model Tanımları (Pydantic DTO) ---
class AnalysisRequest(BaseModel):
    text: str

class AnalysisResult(BaseModel):
    word: str
    lemma: str      # Kök
    pos: str        # Kelime Türü (NOUN, VERB, ADJ vs.)
    feats: Optional[str] = None  # Morfolojik özellikler

class CleanResult(BaseModel):
    original: str
    cleaned: str

class HealthResponse(BaseModel):
    status: str
    stanza_loaded: bool

# --- Global Değişkenler ---
nlp = None
stanza_loaded = False

# --- FastAPI Uygulaması ---
app = FastAPI(
    title="TurkMorph NLP API",
    description="Türkçe metin analizi ve morfolojik işlemler için REST API",
    version="1.0.0"
)

@app.on_event("startup")
async def startup_event():
    """
    Uygulama başlangıcında Stanza modelini yükle.
    Bu işlem sadece bir kere yapılır ve model hafızada kalır.
    """
    global nlp, stanza_loaded
    
    print("⏳ Stanza Türkçe modeli yükleniyor... (İlk seferde indirme gerekebilir)")
    
    try:
        # Türkçe modelini indir (yoksa)
        stanza.download('tr', processors='tokenize,mwt,pos,lemma', verbose=False)
        
        # Pipeline'ı oluştur
        nlp = stanza.Pipeline(
            lang='tr',
            processors='tokenize,mwt,pos,lemma',
            verbose=False
        )
        stanza_loaded = True
        print("✅ Stanza hazır! API aktif.")
    except Exception as e:
        print(f"❌ Stanza yüklenirken hata: {e}")
        stanza_loaded = False

@app.get("/health", response_model=HealthResponse)
async def health_check():
    """
    Sunucu sağlık kontrolü.
    C# tarafı API'nin hazır olup olmadığını bu endpoint ile kontrol edebilir.
    """
    return HealthResponse(
        status="running" if stanza_loaded else "loading",
        stanza_loaded=stanza_loaded
    )

@app.post("/analyze/word", response_model=List[AnalysisResult])
async def analyze_word(request: AnalysisRequest):
    """
    Metni analiz et ve her kelime için:
    - word: Orijinal kelime
    - lemma: Kök hali
    - pos: Kelime türü (NOUN, VERB, ADJ, ADV, PRON, CONJ, NUM, ADP, DET)
    - feats: Morfolojik özellikler (Case, Number, Person vb.)
    """
    if not stanza_loaded or nlp is None:
        raise HTTPException(status_code=503, detail="Stanza modeli henüz yüklenmedi. Lütfen bekleyin.")
    
    if not request.text or len(request.text.strip()) == 0:
        return []
    
    try:
        doc = nlp(request.text)
        results = []
        
        for sent in doc.sentences:
            for word in sent.words:
                results.append(AnalysisResult(
                    word=word.text,
                    lemma=word.lemma if word.lemma else word.text,
                    pos=word.upos if word.upos else "X",
                    feats=word.feats if word.feats else "Kök Halde"
                ))
        
        return results
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Analiz hatası: {str(e)}")

@app.post("/clean", response_model=CleanResult)
async def clean_text(request: AnalysisRequest):
    """
    Metin temizleme pipeline'ı:
    - Rakamları sil
    - Noktalama işaretlerini sil
    - Küçük harfe çevir
    - Fazla boşlukları temizle
    """
    if not request.text:
        return CleanResult(original="", cleaned="")
    
    original = request.text
    cleaned = original
    
    # Rakamları sil
    cleaned = re.sub(r'[0-9]', '', cleaned)
    
    # Noktalama işaretlerini sil (Türkçe karakterleri koru)
    cleaned = re.sub(r'[^\w\s]', '', cleaned)
    
    # Küçük harfe çevir
    cleaned = cleaned.lower()
    
    # Fazla boşlukları temizle
    cleaned = re.sub(r'\s+', ' ', cleaned).strip()
    
    return CleanResult(original=original, cleaned=cleaned)

@app.post("/batch/analyze", response_model=List[List[AnalysisResult]])
async def batch_analyze(texts: List[str]):
    """
    Birden fazla metni toplu analiz et.
    Her metin için ayrı bir sonuç listesi döner.
    """
    if not stanza_loaded or nlp is None:
        raise HTTPException(status_code=503, detail="Stanza modeli henüz yüklenmedi.")
    
    all_results = []
    for text in texts:
        if not text or len(text.strip()) == 0:
            all_results.append([])
            continue
            
        doc = nlp(text)
        text_results = []
        
        for sent in doc.sentences:
            for word in sent.words:
                text_results.append(AnalysisResult(
                    word=word.text,
                    lemma=word.lemma if word.lemma else word.text,
                    pos=word.upos if word.upos else "X",
                    feats=word.feats if word.feats else "Kök Halde"
                ))
        
        all_results.append(text_results)
    
    return all_results

# --- Main Entry Point ---
if __name__ == "__main__":
    print("=" * 50)
    print("🚀 TurkMorph NLP API Başlatılıyor...")
    print("   URL: http://127.0.0.1:8000")
    print("   Swagger Docs: http://127.0.0.1:8000/docs")
    print("=" * 50)
    
    uvicorn.run(app, host="127.0.0.1", port=8000)
