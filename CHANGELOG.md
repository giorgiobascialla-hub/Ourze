# HDLSS 1.0.5

Release 1.0.5 â€” 2026-09-24.

## Added
- Separate MFG Unlock installer/updater for nefh/MFGAmpereUnlock-RenoDx, targeting RTX 2000, 3000 and 4000 in games with native NVIDIA DLSS Frame Generation.
- Query the latest stable release at every installation/update. Verify SHA-256, asset size, origin and x64 format. Never silently fall back to an older cached download.
- Merge ReShade early-loading settings while preserving other add-ons, escaped commas, encoding and user preferences. Support custom add-on folders inside the game directory; protect external shared folders and duplicate installations.
- Independent MFG backup and restoration. Preserve subsequent ReShade settings changes and stop if an add-on or backup changed unexpectedly.

## Removed
- UE5 HDR configuration editor, engine-parameter panel, HDLSS luminance overlay and its global hotkey registration.
- Scene capture and overlay window classes from the executable. No background scene capture is started.
- Automatic Engine.ini recipes during RenoDX installation. Existing game INIs, mods and backups are retained; follow the mod author's instructions when it requires native engine HDR.

## Changed
- HDR / MFG opens ReShade, RenoDX and MFG installation controls directly. Keep the three primary actions visible at compact window sizes.
- MFG add-ons are no longer classified as RenoDX HDR. Restore MFG before removing its ReShade dependency.
- UE4SS installation remains available for mods without the HDLSS telemetry reader. Existing UE4SS runtimes and mods are preserved.
- RenoDX uses the current game catalog and checks recent published assets for the matching add-on. ReShade stable/nightly, shader packages and MFG resolve downloads online. Local imports cannot certify the newest version.
- Update internal help and new interface text in Italian, English, Spanish, French, German and Portuguese.

## Verification
- Live official downloads: MFG Unlock 0.11 and ReShade 6.8.0 full add-on.
- 32 MFG checks: release validation, installation/update, exact and selective restoration, custom folders, duplicates, locked INI rollback, UTF-16, stale previews, external baselines and unrelated file preservation.
- 10,129 localization/UI assertions and 48 WPF renders in six languages at 1180Ã—740 and 3440Ã—1440. Primary MFG action visible without scrolling; navigation and guide checked.
- Existing DLSS transaction suite passed. ReShade stable/nightly extraction, standalone/OptiScaler loading chain, RenoDX installation, restoration and UI regression suite passed.
- No real game files changed by these tests.

## Limits
MFG was not verified in a running game on RTX 2000/3000/4000 hardware. Installing files does not prove that the add-on loads or that a multiplier is active. A game must already implement native DLSS FG; this is not a universal FG injector. API/runtime/driver requirements can change. Consult the author and the in-game ReShade log/panel. External shared add-on directories and Vulkan ReShade layer setup require manual installation. Updates are requested by the user, not silently applied at game launch.

Sources: [MFG Unlock](https://github.com/nefh/MFGAmpereUnlock-RenoDx), [ReShade early loading](https://github.com/crosire/reshade/blob/main/source/dll_main.cpp), [INI format](https://github.com/crosire/reshade/blob/main/source/ini_file.cpp).

### DLSS interface update â€” 2026-09-25
- Original button styling retained; settings arranged in vertical groups, including advanced options.
- Library covers hidden in the DLSS detail view; Back returns to the library.
- Six-language internal guide updated; 48 WPF renders and 10,345 checks passed.


## Download and update
Download HDLSS-Portable-1.0.5.zip below. Close HDLSS and replace HDLSS.exe, preserving data, data-location.txt and all backups. Windows x64 and .NET Framework 4.8 required. Source and test results are included.


---

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


---

# HDLSS 1.0.3

## Fixed
- Disable the retired HDLSS_PeakLimit technique left active by earlier builds when applying native HDR or using Play game. Version 1.0.2 stopped installing the filter but did not deactivate existing copies.
- Preserve ReShade, RenoDX, DLSS, other preset techniques and their settings. Save the previous preset in transaction recovery; repeated migration makes no additional change.
- Keep native UE5 HDR independent of ReShade. The overlay continues to display measured pixel values without forcing them down to the configured peak.

## Validation
The reporting user confirmed that colors and the reported peak returned to normal in Silent Hill Townfall after disabling the old filter. This is a user-reported game check, not universal HDR certification.
Local migration, unrelated-effect preservation, idempotence, exact recovery and DLSS regression tests passed. Engine HDR and mod compatibility remain game-specific.

## Update
Close HDLSS and replace HDLSS.exe with this build. Preserve data, data-location.txt and all backups. Start the game from HDLSS or apply native HDR while the game is closed to migrate an old active HDLSS filter. External presets require manual review. Recover operation can restore the previous preset from its transaction.json.
Windows x64 and .NET Framework 4.8 required. Source is included in HDLSS-Portable-1.0.3.zip.


# HDLSS 1.0.2

- Added installation of the official stable UE4SS end-user runtime and HDLSS reader, with a reviewed file plan and transaction recovery.
- Recognize UE4SS both beside the executable and in the ue4ss subfolder. Preserve existing runtime versions, mod settings and unrelated mods. Explain conflicts with custom overrides, duplicate runtimes or an occupied loader.
- Corrected the search root for manually added Unreal games: native DLSS DLLs in Engine/Plugins and project subdirectories can be detected and selected for updates.
- Native DLSS library updates no longer run the Neural Rendering installation conflict check.
- Corrected component names: DLSS Super Resolution, Ray Reconstruction and Frame Generation are separate from Neural Rendering.
- Separated native UE5 HDR configuration from ReShade/RenoDX: Apply HDR modifies only the two Unreal INIs and never downloads an injector. Existing mods remain untouched. SDR output settings are corrected when requesting HDR.
- Unified scene pixel measurements and optional fresh engine telemetry in the live overlay. Added sample age and explicit last-sample status. The overlay version follows the app version.

## Validation and limits
Installer, recovery, preserved mods, manual game DLL discovery, DLSS regression and UI checks passed on local fixtures. UE4SS compatibility must be checked per game using a fresh UE4SS.log; some games need a game-specific version/configuration. Custom override.txt installations require manual configuration. Native DLL replacement does not add features absent from the game.

Native HDR INI-only installation and recovery passed isolated tests with and without existing mod files, including SDR, scRGB and PQ configurations. Live Windows HDR capture passed on 4,953,600 pixels. This desktop check does not certify every game. The configured peak is a request to the engine, not a universal output clamp. The automatic ReShade peak filter has been removed from this build.

## Sources
- https://docs.ue4ss.com/installation-guide.html
- https://docs.ue4ss.com/dev/installation-guide.html
- https://github.com/UE4SS-RE/RE-UE4SS/releases
- https://github.com/NVIDIA/DLSS

## Download and update
Download HDLSS-Portable-1.0.2.zip. Close HDLSS and replace HDLSS.exe, preserving data, data-location.txt and all backups. Windows x64 and .NET Framework 4.8 required. Source is included.

# Changelog

## 1.0.1 â€” 2026-09-22

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

