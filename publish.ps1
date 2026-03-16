#!/usr/bin/env pwsh

$AssemblyVersion = "9.9"

if ((Test-Path env:CLI_VERSION) -And $env:CLI_VERSION.StartsWith("refs/tags/v")) {
    $AssemblyVersion = $env:CLI_VERSION.Substring(11)
}

Write-Output "version: $AssemblyVersion"

if ((Test-Path env:SKIP_WINDOWS) -And $env:SKIP_WINDOWS.ToUpper() -eq "TRUE") {
    Write-Output "Skipping Windows build because SKIP_WINDOWS is set"
}
else {
    dotnet publish src/gl2gh/gl2gh.csproj -c Release -o dist/win-x64/ -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=partial --self-contained true /p:DebugType=None /p:IncludeNativeLibrariesForSelfExtract=true /p:VersionPrefix=$AssemblyVersion

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Compress-Archive -Path ./dist/win-x64/gl2gh.exe -DestinationPath ./dist/gl2gh.$AssemblyVersion.win-x64.zip -Force

    if (Test-Path -Path ./dist/win-x64/gl2gh-windows-amd64.exe) {
        Remove-Item ./dist/win-x64/gl2gh-windows-amd64.exe
    }

    Copy-Item ./dist/win-x64/gl2gh.exe ./dist/win-x64/gl2gh-windows-amd64.exe

    dotnet publish src/gl2gh/gl2gh.csproj -c Release -o dist/win-x86/ -r win-x86 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=partial --self-contained true /p:DebugType=None /p:IncludeNativeLibrariesForSelfExtract=true /p:VersionPrefix=$AssemblyVersion

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Compress-Archive -Path ./dist/win-x86/gl2gh.exe -DestinationPath ./dist/gl2gh.$AssemblyVersion.win-x86.zip -Force

    if (Test-Path -Path ./dist/win-x86/gl2gh-windows-386.exe) {
        Remove-Item ./dist/win-x86/gl2gh-windows-386.exe
    }

    Copy-Item ./dist/win-x86/gl2gh.exe ./dist/win-x86/gl2gh-windows-386.exe
}

if ((Test-Path env:SKIP_LINUX) -And $env:SKIP_LINUX.ToUpper() -eq "TRUE") {
    Write-Output "Skipping Linux build because SKIP_LINUX is set"
}
else {
    dotnet publish src/gl2gh/gl2gh.csproj -c Release -o dist/linux-x64/ -r linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=partial --self-contained true /p:DebugType=None /p:IncludeNativeLibrariesForSelfExtract=true /p:VersionPrefix=$AssemblyVersion

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    tar -cvzf ./dist/gl2gh.$AssemblyVersion.linux-x64.tar.gz -C ./dist/linux-x64 gl2gh

    if (Test-Path -Path ./dist/linux-x64/gl2gh-linux-amd64) {
        Remove-Item ./dist/linux-x64/gl2gh-linux-amd64
    }

    Copy-Item ./dist/linux-x64/gl2gh ./dist/linux-x64/gl2gh-linux-amd64
}

if ((Test-Path env:SKIP_MACOS) -And $env:SKIP_MACOS.ToUpper() -eq "TRUE") {
    Write-Output "Skipping MacOS build because SKIP_MACOS is set"
}
else {
    dotnet publish src/gl2gh/gl2gh.csproj -c Release -o dist/osx-x64/ -r osx-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=partial --self-contained true /p:DebugType=None /p:IncludeNativeLibrariesForSelfExtract=true /p:VersionPrefix=$AssemblyVersion

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    tar -cvzf ./dist/gl2gh.$AssemblyVersion.osx-x64.tar.gz -C ./dist/osx-x64 gl2gh

    if (Test-Path -Path ./dist/osx-x64/gl2gh-darwin-amd64) {
        Remove-Item ./dist/osx-x64/gl2gh-darwin-amd64
    }

    Copy-Item ./dist/osx-x64/gl2gh ./dist/osx-x64/gl2gh-darwin-amd64
}
