@if "%~1"=="" goto skip

@setlocal enableextensions
"UnrealPak.exe" "%~1" -extract "Temp"

:skip