@echo off
SET buildPath=%CD%\Releases\Mac\NeonLadder.app
SET buildDir=%CD%\Releases\Mac
SET zipPath=%buildDir%\NeonLadder.zip

REM Clean up previous builds
echo Cleaning previous build files...
if exist "%buildDir%" (
    rmdir /S /Q "%buildDir%"
    echo Previous build files deleted.
)

REM Create build directory
mkdir "%buildDir%"

REM Run Unity Build for macOS
echo Starting Unity Build for macOS...
Unity.exe -batchmode -nographics -projectPath "%CD%" -buildOSXUniversalPlayer "%buildPath%" -quit
echo Unity macOS Build Completed.

REM Post-build compression using PowerShell
echo Compressing macOS build...
powershell -command "Compress-Archive -Path '%buildDir%\*' -DestinationPath '%zipPath%'"
echo Compression Completed.

REM Delete all files and folders except the zip after compression
echo Cleaning up after compression...
if exist "%buildDir%" (
    for /F "delims=" %%i in ('dir "%buildDir%" /b /a') do (
        if /I not "%%i"=="NeonLadder.zip" (
            if exist "%buildDir%\%%i\*" (
                rmdir /S /Q "%buildDir%\%%i"
            ) else (
                del /Q "%buildDir%\%%i"
            )
        )
    )
)
echo Clean up completed.
echo macOS Build and post-processing completed.
