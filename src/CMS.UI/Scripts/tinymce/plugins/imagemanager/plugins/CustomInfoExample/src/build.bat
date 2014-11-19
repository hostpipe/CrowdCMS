@echo off
set path=%path%;%windir%\Microsoft.NET\Framework\v1.1.4322
csc /nologo /target:library /out:..\bin\CustomInfoExamplePlugin.dll /reference:..\..\..\bin\MCManager.dll csharp\*.cs
pause
