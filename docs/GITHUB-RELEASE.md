## Changes
- Inline version selectors for DLSS Super Resolution, Ray Reconstruction, Frame Generation, Streamline and Neural Rendering, with bundled catalog metadata and downloads on demand.
- Installation, progress, errors and recovery stay on the main page.
- ASI Loader management, startup update checks and individual removal of mods, manual add-ons and shader packages.
- Larger game thumbnails, game names and clear installed/update states.
- Shared GitHub cache and rate-limit handling; stable ReShade downloads through its official website.
- Fix large RenoDX JSON responses, corrupted caches and unknown versions incorrectly reported as up to date.
- Reject duplicate installation target paths and preserve explicitly imported DLL loaders during duplicate cleanup.
- Updated six-language help.

## Validation and compatibility
Local component, recovery, catalog, Streamline, API and UI regression checks passed. Large RenoDX metadata was verified against a real 17.5-million-character response. Compatibility depends on the game, GPU, driver and installed add-ons. The reported CONTROL Resonant add-on crash is still under diagnosis; this release does not claim to fix third-party add-on conflicts.

## Download and update
Download **HDLSS-Portable-1.0.7.zip** below. Close HDLSS and replace **HDLSS.exe**, preserving **data**, **data-location.txt** and all backups. Windows x64 and .NET Framework 4.8 required. Source is included; the separate SHA-256 file verifies the archive.
