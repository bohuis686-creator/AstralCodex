$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot
$framework = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
$output = Join-Path $projectRoot 'build/AstralCodex'
New-Item -ItemType Directory -Force $output | Out-Null
$references = @('WPF/WindowsBase.dll', 'WPF/PresentationCore.dll', 'WPF/PresentationFramework.dll', 'System.Web.Extensions.dll', 'System.Xaml.dll', 'System.Windows.Forms.dll', 'System.Drawing.dll') |
    ForEach-Object { '/reference:' + (Join-Path $framework $_) }
& (Join-Path $framework 'csc.exe') /nologo /target:winexe /optimize+ /codepage:65001 "/out:$output\AstralCodex.exe" "/win32manifest:$projectRoot\source\app.manifest" $references "$projectRoot\source\Native.cs" "$projectRoot\source\Launcher.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
Copy-Item -LiteralPath "$projectRoot/media" -Destination $output -Recurse -Force
New-Item -ItemType Directory -Force "$output/scripts" | Out-Null
Copy-Item -LiteralPath "$projectRoot/scripts/integrate-entrypoints.ps1" -Destination "$output/scripts" -Force
Copy-Item "$projectRoot/Install-Desktop.cmd", "$projectRoot/Restore-Desktop.cmd" $output -Force
Copy-Item "$projectRoot/LICENSE", "$projectRoot/THIRD_PARTY_NOTICES.md", "$projectRoot/README.md" $output -Force
Copy-Item "$projectRoot/README.en.md", "$projectRoot/CONTRIBUTING.md", "$projectRoot/RELEASE_NOTES.md" $output -Force
Copy-Item -LiteralPath "$projectRoot/docs" -Destination $output -Recurse -Force
if (!(Test-Path "$output/launcher.json")) { Copy-Item "$projectRoot/launcher.json" $output }
@'
@echo off
cd /d "%~dp0"
start "" "AstralCodex.exe" --preview
'@ | Set-Content "$output/Preview.cmd" -Encoding Ascii
$result = Start-Process -FilePath "$output\AstralCodex.exe" -ArgumentList '--self-test' -WindowStyle Hidden -Wait -PassThru
if ($result.ExitCode -ne 0) { throw "Self-test failed. See $output/error.txt" }
Get-Content "$output/self-test.json"
Write-Output "Build ready at $output"
