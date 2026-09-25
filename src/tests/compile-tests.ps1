$ErrorActionPreference='Stop'
$fx=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$wpf=Join-Path $fx 'WPF'
& (Join-Path $fx 'csc.exe') /nologo /target:exe /platform:x64 /codepage:65001 "/out:$PSScriptRoot\Tests.exe" "/r:$PSScriptRoot\HDLSS.exe" /r:System.Core.dll /r:System.Web.Extensions.dll "/r:$wpf\PresentationFramework.dll" "/r:$wpf\PresentationCore.dll" "/r:$wpf\WindowsBase.dll" "$PSScriptRoot\Tests.cs"
if($LASTEXITCODE -ne 0){throw 'Test compilation failed'}
