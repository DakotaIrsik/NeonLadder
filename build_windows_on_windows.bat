@echo off
SET buildPath=%CD%\Releases\PC\Windows\NeonLadder.exe
SET buildDir=%CD%\Releases\PC\Windows
SET zipPath=%buildDir%\NeonLadder.zip

REM Clean up previous builds
echo Cleaning previous build files...
if exist "%buildDir%" (
    rmdir /S /Q "%buildDir%"
    echo Previous build files deleted.
)

REM Create build directory
mkdir "%buildDir%"

REM Build Asset Bundles here if needed

REM Run Unity Build
echo Starting Unity Build...
Unity.exe -batchmode -nographics -projectPath "%CD%" -buildWindows64Player "%buildPath%" -quit
echo Unity Build Completed.

REM Post-build compression using PowerShell
echo Compressing build...
powershell -command "Compress-Archive -Path '%buildDir%' -DestinationPath '%zipPath%'"
echo Compression Completed.

REM Delete all files and folders except the zip
echo Cleaning up...
for /F "delims=" %%i in ('dir "%buildDir%" /b /a') do (
    if /I not "%%i"=="NeonLadder.zip" (
        if exist "%buildDir%\%%i\*" (
            rmdir /S /Q "%buildDir%\%%i"
        ) else (
            del /Q "%buildDir%\%%i"
        )
    )
)
echo Clean up completed.
echo Build and post-processing completed.
