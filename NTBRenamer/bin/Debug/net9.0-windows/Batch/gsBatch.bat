@echo off
setlocal

:: Set the path to Ghostscript executable
set "GSCMD=C:\Program Files\gs\gs10.05.1\bin\gswin64c.exe"

:: Loop through all .ps files in the current directory
for %%F in (*.ps) do (
    echo Converting %%F to %%~nF.pdf...
    "%GSCMD%" -dBATCH -dNOPAUSE -sDEVICE=pdfwrite -sOutputFile="%%~nF.pdf" "%%F"
)

echo Done.

