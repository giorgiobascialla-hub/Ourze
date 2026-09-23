# HDLSS 1.0.4

## Fixed
- Correct mixed-language text in the HDR panel and adjacent controls in Italian, English, Spanish, French, German and Portuguese.
- Translate complete monitor advice, file status and UE4SS reader messages, including dynamic values and nested errors. Preserve numbers and paths when switching languages.
- Display a duplicate UE4SS runtime error once instead of repeating it on two lines.
- Update the internal help for the translated HDR status messages.

## Validation
2,628 message and placeholder round-trip checks passed, together with nested-error, path-preservation, repeated-refresh and language-switching checks. Twelve WPF views were rendered across six languages at compact and maximized dimensions.
This translation update covers the HDR panel and adjacent controls; it does not claim full translation of every technical log or help page.

## Known issue
The intermittent 9999-nit scene-overlay report remains under investigation. This release does not change capture or tone mapping and does not claim to fix that report.

## Update
Download HDLSS-Portable-1.0.4.zip. Close HDLSS and replace HDLSS.exe, preserving data, data-location.txt and all backups. Windows x64 and .NET Framework 4.8 required. Source and SHA-256 checksum are included with the release.
