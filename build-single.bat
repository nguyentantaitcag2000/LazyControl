@echo off
setlocal enabledelayedexpansion

:: Đọc version từ Configuration.cs bằng findstr
for /f "tokens=2 delims==" %%a in ('findstr "VERSION = " Configuration.cs') do (
    set temp=%%a
    set temp=!temp: =!
    set temp=!temp:"=!
    set temp=!temp:;=!
    set VERSION=!temp!
)

:: Kiểm tra version
if "%VERSION%"=="" (
    echo Cannot find VERSION in Configuration.cs
    pause
    exit /b 1
)

echo Found version: %VERSION%

:: Tên project (thay LazyControl.csproj nếu khác)
set PROJECT_NAME=LazyControl
set CSPROJ=LazyControl.csproj
set OUTPUT_DIR=my-publish

:: Publish
echo Publishing application...
dotnet publish %CSPROJ% -r win-x64 -c Release ^
  -p:PublishSingleFile=true ^
  -p:IncludeAllContentForSelfExtract=true ^
  -p:SelfContained=true ^
  -p:PublishTrimmed=false

if %errorlevel% neq 0 (
    echo Publish failed!
    pause
    exit /b 1
)

:: Tìm và đổi tên file
set NEW_NAME=%PROJECT_NAME%-%VERSION%.exe

:: Tạo thư mục output nếu chưa tồn tại
if not exist "%OUTPUT_DIR%" (
    mkdir "%OUTPUT_DIR%"
)

:: Làm trống thư mục output
echo Cleaning output directory...
del /q "%OUTPUT_DIR%\*" >nul 2>&1
for /d %%i in ("%OUTPUT_DIR%\*") do rmdir /s /q "%%i" >nul 2>&1

:: Copy và đổi tên file
for /d %%d in (bin\Release\net*) do (
    if exist "%%d\win-x64\publish\%PROJECT_NAME%.exe" (
        echo Copying and renaming output file...
        copy "%%d\win-x64\publish\%PROJECT_NAME%.exe" "%OUTPUT_DIR%\%NEW_NAME%"
        goto :found
    )
)

echo Could not find output file!
pause
exit /b 1

:found
:: Tạo AutoUpdater.xml trong thư mục output
echo Creating AutoUpdater.xml...
(
echo ^<?xml version="1.0" encoding="UTF-8"?^>
echo ^<item^>
echo   ^<version^>%VERSION%^</version^>
echo   ^<url^>https://storage-test.lazycodet.com/products/lazycontrol/%NEW_NAME%^</url^>
echo   ^<changelog^>^</changelog^>
echo   ^<mandatory^>false^</mandatory^>
echo ^</item^>
) > "%OUTPUT_DIR%\AutoUpdater.xml"

echo.
echo ========================================
echo Build completed successfully!
echo Output file: %OUTPUT_DIR%\%NEW_NAME%
echo AutoUpdater.xml created with version %VERSION%
echo ========================================

pause
