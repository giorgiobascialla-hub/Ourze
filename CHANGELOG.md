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
