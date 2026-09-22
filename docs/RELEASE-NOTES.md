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
