@echo off
set path=%path%;%windir%\Microsoft.NET\Framework\v1.1.4322
vbc /nologo /target:library /out:..\..\bin\TemplatePlugin.dll /reference:..\..\..\..\bin\MCManager.dll,System.dll *.vb
pause
