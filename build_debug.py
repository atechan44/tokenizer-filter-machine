import subprocess
import sys

try:
    process = subprocess.run(
        ["dotnet", "build", "TurkishNLP.Desktop/TurkishNLP.Desktop.csproj"],
        capture_output=True,
        text=True,
        encoding='utf-8',
        errors='replace'
    )
    print("STDOUT:", process.stdout)
    print("STDERR:", process.stderr)
except Exception as e:
    print(e)
