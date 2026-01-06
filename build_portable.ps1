# Build C# project
Write-Host "Building C# application..."
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true

# Create output folder
$outputDir = "TurkishNLP-v1.0"
if (Test-Path $outputDir) {
    Remove-Item $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $outputDir

# Copy files
$publishPath = "TurkishNLP.Desktop/bin/Release/net8.0-windows/win-x64/publish"
Copy-Item "$publishPath/TurkishNLP.Desktop.exe" "$outputDir/"
# Copy dependencies if not single-file (or just to be safe if single file didn't bundle everything, though with SingleFile=true it usually does except native libs)
# But user script just copies the EXE. With SingleFile=true --self-contained false, it might need runtime or local dlls?
# Wait, user script provided: Copy-Item "bin/Release/..." 
# The user's script assumes running from root? Yes.
# BUT the path in user script is "bin/Release...". Desktop project is in "TurkishNLP.Desktop/" folder.
# I need to adjust the path in the script to "TurkishNLP.Desktop/bin/Release..." OR cd into TurkishNLP.Desktop.
# User's command in Step 1043 was run from Root.
# The dotnet publish output says: -> TurkishNLP.Desktop\bin\Release\net8.0-windows\win-x64\publish\
# So the path is indeed `TurkishNLP.Desktop/bin/...`
# The user's provided script says `Copy-Item "bin/Release/..."`. This works ONLY IF run inside `TurkishNLP.Desktop` folder.
# BUT the `Copy-Item "database/words.db"` implies root context.
# So I should PROBABLY fix the path to `TurkishNLP.Desktop/bin/...`.
# Let's verify.
# User provided:
# dotnet publish ... (Works from root if solution is there. Solution IS in root: TurkishNLP.sln)
# Copy-Item "bin/Release/..." -> usually this would be "TurkishNLP.Desktop/bin/Release/..." if run from root.
# I will use "TurkishNLP.Desktop/bin/Release/..." to be safe and correct.

Copy-Item "TurkishNLP.Desktop/bin/Release/net8.0-windows/win-x64/publish/*.exe" "$outputDir/"
Copy-Item "TurkishNLP.Desktop/bin/Release/net8.0-windows/win-x64/publish/*.dll" "$outputDir/"  # Just in case
Copy-Item "TurkishNLP.Desktop/bin/Release/net8.0-windows/win-x64/publish/*.json" "$outputDir/" # deps.json, runtimeconfig.json

# Copy database
Copy-Item "database/words.db" "$outputDir/"

# Copy backend (if exists)
if (Test-Path "backend/dist/TurkishNLP-Backend.exe") {
    New-Item -ItemType Directory -Force -Path "$outputDir/backend"
    Copy-Item "backend/dist/TurkishNLP-Backend.exe" "$outputDir/backend/"
}

# Create launcher batch file
@"
@echo off
title Turkish NLP Analyzer
echo Starting Turkish NLP Analyzer...
echo.

REM Start backend if exists
if exist backend\TurkishNLP-Backend.exe (
    echo Starting backend...
    start /B backend\TurkishNLP-Backend.exe
    timeout /t 3 /nobreak > nul
)

REM Start main application
echo Starting application...
start TurkishNLP.Desktop.exe

exit
"@ | Out-File -FilePath "$outputDir/CALISTIR.bat" -Encoding ASCII

# Create README
@"
Turkish NLP Analyzer v1.0
==========================

NASIL CALISTIRILIR:
1. CALISTIR.bat dosyasina cift tiklayin
2. Program otomatik acilacaktir

GEREKSINIMLER:
- Windows 10 veya uzeri
- .NET 8 Runtime (otomatik indirilir/kurulu olmalidir)

OZELLIKLER:
- Turkce kelime analizi (Stanza)
- Toplu CSV isleme
- Veritabani yonetimi
- JSON export

Gelistirici: Atakan
Tarih: $(Get-Date -Format "dd.MM.yyyy")
"@ | Out-File -FilePath "$outputDir/README.txt" -Encoding UTF8

Write-Host "✅ Build complete! Package: $outputDir"
