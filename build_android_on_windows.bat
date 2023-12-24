@echo off
SET apkPath=%CD%\Releases\Android\NeonLadder.apk
SET buildDir=%CD%\Releases\Android

REM Clean up previous builds
echo Cleaning previous Android build files...
if exist "%buildDir%" (
    rmdir /S /Q "%buildDir%"
    echo Previous Android build files deleted.
)

REM Create build directory
mkdir "%buildDir%"

REM Run Unity Build for Android
echo Starting Unity Build for Android...
Unity.exe -batchmode -nographics -projectPath "%CD%" -executeMethod YourBuildClass.YourBuildMethod -quit
echo Unity Android Build Completed.

REM Move APK to build directory
if exist "%apkPath%" (
    move "%apkPath%" "%buildDir%"
    echo APK moved to build directory.
)

echo Android Build completed.
