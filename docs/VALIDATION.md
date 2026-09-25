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


```text
MFG release: 0.11 sha256:fddaead8ff15bfb270848d5ae1bbc7875b8c7299faf2601fa70d637e627a3948
ReShade: Previously verified official download
PASS: preserve escaped commas and existing add-ons
PASS: early-load idempotence
PASS: remove only MFG early-load entry
PASS: existing path and case respected
PASS: bad download rejected
PASS: live-shaped release parsed
PASS: draft rejected
PASS: prerelease rejected
PASS: foreign download rejected
PASS: missing digest rejected
PASS: missing ReShade rejected
PASS: duplicate MFG rejected
PASS: no NVIDIA, Streamline or engine INI writes
PASS: MFG present, not misidentified as HDR
PASS: UTF16 preserved
PASS: user multiplier preserved
PASS: ReShade cleanup protects MFG dependency
PASS: HDR installer can coexist with MFG
PASS: repeat update keeps original backup
PASS: byte-exact initial restore after update
PASS: restore preserves subsequent ReShade edits
PASS: stale preview rejected
PASS: stale preview writes no add-on
PASS: external pre-existing file restored
PASS: custom add-on directory respected
PASS: custom location detected
PASS: custom location restored
PASS: external shared add-on folder protected
PASS: disabled addon reported instead of false activation
PASS: locked INI install rejected
PASS: locked INI causes no partial install
PASS: changed addon blocks destructive restore

PASS it 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS it 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS en 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS en 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS es 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS es 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS fr 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS fr 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS de 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS de 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS pt 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS pt 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS 10129 assertions; 48 WPF renders; no overlay types in binary.

PASS: real ReShade stable/nightly archives extracted without execution; standalone and OptiScaler chain; nightly update and baseline restore; read-only INI restore; unrelated loaders preserved; RenoDX addon install/restore; duplicate/unknown loader and stale plan rejection; NVIDIA tag validation/order; embedded DLSS 5 section and slider roundtrip. No real game modifications.
PASS: third-party NR cleanup without manifest and recovery; preserved separate MFG/HDR/native DLSS; unknown and modified overlays blocked; separate NR renderer detected; verified overlay preserved when supplied; confined paths/ADS/device names; x64 fixtures; install/update and original baseline restore; readonly; preserved HDR/FG/UE4SS/ReShade; stale preview rejection; external Autopilot detection and reversible uninstall; original backup restore; protected game executable rejection; release filtering; invalid component rejection; GPU/driver checks; source mutation and locked-file rejection; recovery of foreign removal. No real game files changed.PASS it 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS it 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS en 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS en 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS es 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS es 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS fr 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS fr 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS de 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS de 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS pt 1180: home, mods, MFG action visible, guide, DLSS, navigation
PASS pt 3440: home, mods, MFG action visible, guide, DLSS, navigation
PASS 10345 assertions; 48 WPF renders; no overlay types in binary.

```
