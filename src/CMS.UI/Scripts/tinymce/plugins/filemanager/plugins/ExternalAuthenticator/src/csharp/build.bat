@echo off
set path=%path%;%windir%\Microsoft.NET\Framework\v1.1.4322
csc /nologo /target:library /out:..\..\bin\ExternalAuthenticator.dll /reference:..\..\..\..\bin\MCManager.dll *.cs
pause
