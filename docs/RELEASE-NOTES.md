HDLSS 1.0.0 — component inventory update

- Replaced the ambiguous duplicate count with a SHA-256 comparison.
- Reports identical files in multiple locations, differing files, or an unavailable comparison.
- Check files and paths refreshes the inventory and shows the exact paths.
- Added the new control and comparison text in all six interface languages.
- Inventory limit failures are explicit instead of silently returning a partial list.
- Updated the internal English and Italian help.
- Retains the approved logo, centered covers and installation progress indicator.

Multiple DLL locations may be required by a game. No DLL is automatically removed.
Matching files do not certify runtime loading or game compatibility.
This build is version 1.0.0. Cross-hardware certification is incomplete.

English and Windows update
- A clean first launch uses English. Saved choices in all six languages are retained. Missing or invalid language values fall back to English.
- Coalesced queued language/theme refreshes to avoid repeated interface traversals in the same dispatcher cycle.
- Failed component scans clear stale path details.
- Windows x64 optimized build; .NET Framework 4.8. Existing per-monitor DPI and long-path declarations retained.
- This is not a hardware certification or an FPS benchmark. Feature support remains specific to GPU, driver, game, display and component build. ARM64 and 32-bit operation have not been validated.
- Passed clean/invalid/saved language tests, WPF UI smoke tests and DLL management regression tests on the available Windows machine.

Presentation update: DLSS 5 naming is consistent across app labels, screenshots, documentation, narration and subtitles. Original external product names and technical identifiers are retained. UI and DLL tests passed; all six languages rendered at windowed and maximized sizes.

Installation fix: the SHA-256-verified DLSS 5 Swapper control overlay no longer falsely blocks OptiScaler NR installation. Unknown/modified DLSS/NR add-ons and separate RenoDX NR renderers remain protected by conflict checks. The control overlay is preserved. Regression tests cover installation, updates, cleanup of third-party NR installations without an HDLSS manifest, and recovery; separate MFG/HDR/native DLLs remain untouched. Tested on isolated fixtures and a read-only Fatekeeper scan; in-game operation is not certified.
