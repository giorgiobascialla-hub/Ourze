## HDLSS 1.0.1

**DLSS 5 & Neural Rendering for compatible games.**

### Changelog
- Fixed the false NR installation conflict caused by the verified DLSS 5 Swapper control overlay. The overlay is preserved.
- Recognition checks SHA-256; unknown or modified DLSS/NR add-ons and separate NR renderers still trigger conflict checks.
- Added regression tests for third-party NR cleanup without an HDLSS manifest, recovery, and preservation of separate MFG/HDR/native game DLLs.
- Updated internal help with the limits of overlay recognition and cleanup.

### Download and update
Download **HDLSS-Portable-1.0.1.zip**. Close HDLSS, extract the package, and replace your existing **HDLSS.exe**, preserving **data**, **data-location.txt** and all backups. Windows x64 and .NET Framework 4.8 required. SHA256SUMS.txt contains the package checksum; source is included in the ZIP.

### Validation
Local regression tests passed, including a copy of the reported overlay and a read-only Fatekeeper scan. No real game files were changed by the tests. In-game behavior still depends on the selected game, GPU, driver and component build.

Cleanup removes recognized NR components from third-party installations with recovery copies. It is not a universal uninstaller: unknown files, external records and the control overlay can remain.

[Full changelog](https://github.com/giorgiobascialla-hub/Ourze/blob/v1.0.1/CHANGELOG.md) · [Existing presentation and media](https://github.com/giorgiobascialla-hub/Ourze/releases/tag/v1.0.0)
