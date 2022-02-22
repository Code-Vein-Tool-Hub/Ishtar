@if "%~1"=="" goto skip

@setlocal enableextensions
"UnrealPak.exe" "%~1" -extract "%~2"

:skip