"""
Turkish NLP Analyzer Module
Uses Stanza library for morphological analysis of Turkish words.
"""

import logging
import re
from typing import List, Dict, Any, Optional

import stanza

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


class TurkishAnalyzer:
    """
    Turkish word analyzer using Stanza NLP library.
    Provides morphological analysis including POS tagging, lemmatization, and feature extraction.
    """
    
    def __init__(self):
        """Initialize the analyzer and load the Turkish Stanza model."""
        self._nlp: Optional[stanza.Pipeline] = None
        self._is_loaded = False
        
    def load_model(self) -> bool:
        """
        Load the Stanza Turkish model.
        Downloads the model if not already present.
        
        Returns:
            bool: True if model loaded successfully, False otherwise.
        """
        try:
            logger.info("Downloading Stanza Turkish model (if not present)...")
            stanza.download('tr', processors='tokenize,pos,lemma', verbose=False)
            
            logger.info("Loading Stanza Turkish pipeline...")
            self._nlp = stanza.Pipeline(
                lang='tr',
                processors='tokenize,pos,lemma',
                verbose=False
            )
            self._is_loaded = True
            logger.info("Turkish NLP model loaded successfully.")
            return True
            
        except Exception as e:
            logger.error(f"Failed to load Stanza model: {e}")
            self._is_loaded = False
            return False
    
    @property
    def is_loaded(self) -> bool:
        """Check if the model is loaded."""
        return self._is_loaded
    
    def _validate_input(self, text: str) -> tuple[bool, str]:
        """
        Validate input text.
        
        Args:
            text: Input text to validate.
            
        Returns:
            Tuple of (is_valid, error_message).
        """
        if not text or not text.strip():
            return False, "Input text cannot be empty"
        
        # Allow Turkish characters, common punctuation, and spaces
        # Turkish specific chars: çÇğĞıİöÖşŞüÜ
        valid_pattern = r'^[\w\s\-çÇğĞıİöÖşŞüÜâÂîÎûÛ.,!?;:\'\"()]+$'
        if not re.match(valid_pattern, text.strip(), re.UNICODE):
            return False, "Input contains invalid characters"
        
        return True, ""
    
    def _parse_features(self, feats: Optional[str]) -> Dict[str, str]:
        """
        Parse morphological features string into a dictionary.
        
        Args:
            feats: Feature string in format "Key=Value|Key=Value"
            
        Returns:
            Dictionary of features.
        """
        if not feats:
            return {}
        
        features = {}
        for feat in feats.split('|'):
            if '=' in feat:
                key, value = feat.split('=', 1)
                features[key] = value
        return features
    
    def analyze_word(self, text: str) -> Dict[str, Any]:
        """
        Analyze a single word or short text.
        
        Args:
            text: The word or text to analyze.
            
        Returns:
            Dictionary containing analysis results:
            - word: Original input text
            - root: Lemma/root form
            - pos: Part of speech tag
            - features: Morphological features
            - all_analyses: List of all token analyses
        """
        # Validate input
        is_valid, error_msg = self._validate_input(text)
        if not is_valid:
            return {
                "word": text,
                "error": error_msg,
                "root": None,
                "pos": None,
                "features": {},
                "all_analyses": []
            }
        
        # Check if model is loaded
        if not self._is_loaded or self._nlp is None:
            return {
                "word": text,
                "error": "NLP model not loaded. Please initialize the analyzer first.",
                "root": None,
                "pos": None,
                "features": {},
                "all_analyses": []
            }
        
        try:
            # Process the text
            doc = self._nlp(text.strip())
            
            all_analyses = []
            primary_root = None
            primary_pos = None
            primary_features = {}
            
            # Extract analyses from all tokens
            for sentence in doc.sentences:
                for word in sentence.words:
                    analysis = {
                        "text": word.text,
                        "lemma": word.lemma,
                        "upos": word.upos,  # Universal POS tag
                        "xpos": word.xpos,  # Language-specific POS tag
                        "features": self._parse_features(word.feats)
                    }
                    all_analyses.append(analysis)
                    
                    # Use first word as primary analysis
                    if primary_root is None:
                        primary_root = word.lemma
                        primary_pos = word.upos
                        primary_features = self._parse_features(word.feats)
            
            return {
                "word": text,
                "root": primary_root,
                "pos": primary_pos,
                "features": primary_features,
                "all_analyses": all_analyses,
                "error": None
            }
            
        except Exception as e:
            logger.error(f"Error analyzing word '{text}': {e}")
            return {
                "word": text,
                "error": str(e),
                "root": None,
                "pos": None,
                "features": {},
                "all_analyses": []
            }
    
    def analyze_batch(self, words: List[str]) -> List[Dict[str, Any]]:
        """
        Analyze a batch of words.
        
        Args:
            words: List of words to analyze.
            
        Returns:
            List of analysis results for each word.
        """
        if not words:
            return []
        
        results = []
        for word in words:
            result = self.analyze_word(word)
            results.append(result)
        
        return results


# Singleton instance for the application
_analyzer_instance: Optional[TurkishAnalyzer] = None


def get_analyzer() -> TurkishAnalyzer:
    """
    Get or create the singleton TurkishAnalyzer instance.
    
    Returns:
        TurkishAnalyzer instance.
    """
    global _analyzer_instance
    if _analyzer_instance is None:
        _analyzer_instance = TurkishAnalyzer()
    return _analyzer_instance
