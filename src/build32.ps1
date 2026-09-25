param([string]$Output=(Join-Path $PSScriptRoot '..\HDLSS.exe'))
# Compatibility entry point. HDLSS targets Windows x64.
& (Join-Path $PSScriptRoot 'build.ps1') -Output $Output
