#!/bin/bash

# Define paths
ipaPath="$(pwd)/Releases/iOS/NeonLadder.ipa"
buildDir="$(pwd)/Releases/iOS"

# Clean up previous builds
echo "Cleaning previous iOS build files..."
if [ -d "$buildDir" ]; then
    rm -rf "$buildDir"
    echo "Previous iOS build files deleted."
fi

# Create build directory
mkdir -p "$buildDir"

# Run Unity Build for iOS
echo "Starting Unity Build for iOS..."
/Applications/Unity/Hub/Editor/[Unity_Version]/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath "$(pwd)" -executeMethod YourBuildClass.YourBuildMethod -quit
echo "Unity iOS Build Completed."

# Additional steps can be added here for further processing, like moving the IPA or further compiling with Xcode

echo "iOS Build completed."
