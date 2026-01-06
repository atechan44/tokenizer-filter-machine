"""
Turkish NLP FastAPI Backend
Provides REST API endpoints for Turkish word analysis using Stanza.
"""

import logging
from typing import List, Optional
from contextlib import asynccontextmanager
from bs4 import BeautifulSoup
import requests
from readability import Document

from fastapi import FastAPI, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from pydantic import BaseModel, Field

from analyzer import TurkishAnalyzer, get_analyzer

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


# ============ Pydantic Models ============

class WordRequest(BaseModel):
    """Request model for single word analysis."""
    word: str = Field(..., min_length=1, description="The word to analyze")
    
    class Config:
        json_schema_extra = {
            "example": {"word": "gidiyorum"}
        }


class BatchRequest(BaseModel):
    """Request model for batch word analysis."""
    words: List[str] = Field(..., min_items=1, description="List of words to analyze")
    
    class Config:
        json_schema_extra = {
            "example": {"words": ["gidiyorum", "evler", "güzel"]}
        }


class AnalysisResult(BaseModel):
    """Response model for word analysis."""
    word: str
    root: Optional[str] = None
    pos: Optional[str] = None
    features: dict = {}
    all_analyses: List[dict] = []
    error: Optional[str] = None


class HealthResponse(BaseModel):
    """Response model for health check."""
    status: str
    model_loaded: bool
    message: str


# ============ Application Lifecycle ============

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager - handles startup and shutdown."""
    # Startup
    logger.info("Starting Turkish NLP API...")
    analyzer = get_analyzer()
    
    logger.info("Loading Stanza Turkish model (this may take a moment)...")
    if analyzer.load_model():
        logger.info("✓ Model loaded successfully!")
    else:
        logger.warning("⚠ Failed to load model. API will return errors for analysis requests.")
    
    yield
    
    # Shutdown
    logger.info("Shutting down Turkish NLP API...")


# ============ FastAPI Application ============

app = FastAPI(
    title="Turkish NLP API",
    description="Turkish word analysis API using Stanza NLP library",
    version="1.0.0",
    lifespan=lifespan
)

# CORS Configuration - Allow localhost on any port
app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "http://localhost:*",
        "http://127.0.0.1:*",
        "http://localhost:3000",
        "http://localhost:5000",
        "http://localhost:8080",
    ],
    allow_origin_regex=r"http://(localhost|127\.0\.0\.1)(:\d+)?",
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# ============ Exception Handlers ============

@app.exception_handler(Exception)
async def global_exception_handler(request, exc):
    """Global exception handler for unhandled errors."""
    logger.error(f"Unhandled error: {exc}")
    return JSONResponse(
        status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
        content={"error": "Internal server error", "detail": str(exc)}
    )


# ============ API Endpoints ============

@app.get("/health", response_model=HealthResponse, tags=["Health"])
async def health_check():
    """
    Health check endpoint.
    Returns the status of the API and whether the NLP model is loaded.
    """
    analyzer = get_analyzer()
    
    if analyzer.is_loaded:
        return HealthResponse(
            status="healthy",
            model_loaded=True,
            message="Turkish NLP API is running and model is loaded."
        )
    else:
        return HealthResponse(
            status="degraded",
            model_loaded=False,
            message="API is running but NLP model is not loaded."
        )


@app.post("/analyze", response_model=AnalysisResult, tags=["Analysis"])
async def analyze_word(request: WordRequest):
    """
    Analyze a single Turkish word.
    
    Returns morphological analysis including:
    - root: Lemma/root form of the word
    - pos: Part of speech (NOUN, VERB, ADJ, etc.)
    - features: Morphological features (case, number, tense, etc.)
    - all_analyses: Detailed analysis of all tokens
    """
    analyzer = get_analyzer()
    
    if not analyzer.is_loaded:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="NLP model is not loaded. Please try again later."
        )
    
    try:
        word = request.word.strip()
        
        if not word:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Word cannot be empty."
            )
        
        result = analyzer.analyze_word(word)
        
        if result.get("error"):
            logger.warning(f"Analysis error for '{word}': {result['error']}")
        
        return AnalysisResult(**result)
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error in analyze_word endpoint: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Analysis failed: {str(e)}"
        )


@app.post("/analyze-batch", response_model=List[AnalysisResult], tags=["Analysis"])
async def analyze_batch(request: BatchRequest):
    """
    Analyze a batch of Turkish words.
    
    Accepts a list of words and returns analysis results for each.
    More efficient for processing multiple words at once.
    """
    analyzer = get_analyzer()
    
    if not analyzer.is_loaded:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="NLP model is not loaded. Please try again later."
        )
    
    try:
        words = [w.strip() for w in request.words if w.strip()]
        
        if not words:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Word list cannot be empty."
            )
        
        # Limit batch size to prevent memory issues
        max_batch_size = 100
        if len(words) > max_batch_size:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail=f"Batch size exceeds maximum limit of {max_batch_size} words."
            )
        
        results = analyzer.analyze_batch(words)
        
        return [AnalysisResult(**r) for r in results]
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error in analyze_batch endpoint: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Batch analysis failed: {str(e)}"
        )


@app.post("/fetch-article", tags=["Tools"])
async def fetch_article(data: dict):
    """
    Fetch article content from URL and extract clean text.
    """
    url = data.get('url')
    
    if not url:
        return {"success": False, "error": "URL is required"}
    
    try:
        # Fetch the page
        headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
        }
        response = requests.get(url, headers=headers, timeout=10)
        response.raise_for_status()
        
        # Extract article content using readability
        doc = Document(response.text)
        title = doc.title()
        html_content = doc.summary()
        
        # Parse with BeautifulSoup to get clean text
        soup = BeautifulSoup(html_content, 'html.parser')
        
        # Remove script and style elements
        for script in soup(["script", "style"]):
            script.decompose()
        
        # Get text
        text = soup.get_text()
        
        # Clean up whitespace
        lines = (line.strip() for line in text.splitlines())
        chunks = (phrase.strip() for line in lines for phrase in line.split("  "))
        text = ' '.join(chunk for chunk in chunks if chunk)
        
        # Count words
        word_count = len(text.split())
        
        return {
            "success": True,
            "title": title,
            "text": text,
            "word_count": word_count,
            "url": url
        }
        
    except requests.RequestException as e:
        return {"success": False, "error": f"Failed to fetch URL: {str(e)}"}
    except Exception as e:
        return {"success": False, "error": f"Error processing article: {str(e)}"}


@app.get("/", tags=["Info"])
async def root():
    """Root endpoint with API information."""
    return {
        "name": "Turkish NLP API",
        "version": "1.0.0",
        "description": "Turkish word analysis using Stanza NLP",
        "endpoints": {
            "POST /analyze": "Analyze a single word",
            "POST /analyze-batch": "Analyze multiple words",
            "GET /health": "Health check"
        }
    }


# ============ Main Entry Point ============

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=8000,
        reload=True,
        log_level="info"
    )
