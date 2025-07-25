@echo off
dotnet publish -r win-x64 -c Release ^
  -p:PublishSingleFile=true ^
  -p:IncludeAllContentForSelfExtract=true ^
  -p:SelfContained=true ^
  -p:PublishTrimmed=false
pause
