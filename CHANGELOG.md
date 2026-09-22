# Changelog

## 1.0.1 — 2026-09-22

### Fixed
- OptiScaler Neural Rendering installation no longer mistakes the verified DLSS 5 Swapper control overlay for a second NR renderer. The overlay is preserved.
- The exception checks the file's SHA-256, not just its name. Unknown or modified DLSS/NR add-ons and separate RenoDX NR renderers still trigger conflict checks.

### Validation and help
- Added regression tests for cleanup of recognized third-party NR installations without an HDLSS manifest, backup recovery, and preservation of separate MFG, HDR and native game DLLs.
- Added tests for unknown and modified overlays, a separate NR renderer alongside the verified overlay, and overlay preservation across installation, updates and recovery.
- Updated the internal help to explain the distinction between a control overlay and an NR renderer.
- Verified locally with isolated installation/cleanup fixtures and a read-only Fatekeeper scan. In-game behavior and universal hardware compatibility are not certified.

### Updating
Close HDLSS and replace HDLSS.exe with the 1.0.1 build. Preserve data, data-location.txt and existing backups. Windows x64 and .NET Framework 4.8 are required. There is no need to delete the verified control overlay or reinstall unrelated HDR/MFG components.

## 1.0.0

Initial public portable release with DLSS 5 / Neural Rendering management, HDR tools, component inventory, backups and recovery. See the archived 1.0.0 release for its original package and presentation.
