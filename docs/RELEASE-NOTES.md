## New and improved
- Added ASI Loader installation and management, plus updates for existing NVIDIA Streamline integrations.
- Added background mod-version checks at startup and per-component update controls in the main game page. Downloads and installation remain user-triggered.
- Added direct removal buttons for ReShade, ASI Loader, RenoDX HDR and MFG Unlock. Individual manual add-ons and shader packages can also be removed, with verified file deletion and recovery copies.
- Recognize manually installed ReShade add-ons separately from managed RenoDX HDR. Control RR uses its Nexus Mods page and assisted download; unknown sources use an exact-filename local update.
- AutoHDR, Pumbo, Lilium, qUINT and SweetFX have Install selected and Remove selected actions.
- Added conservative duplicate-loader cleanup: only identical recognized loaders with a verifiable loading path are eligible. Native DLL copies in different folders are preserved.
- Restored and enlarged game thumbnails in HDR and DLSS views, retaining game names below them.
- Yellow buttons mean installed, not update available. Updates have separate controls. Updated internal help and six-language labels.

## Validation and limits
48 WPF renders across six languages and two window sizes; 18,181 UI/translation assertions. Component removal/recovery, manual add-on updates and all five shader/add-on removal paths were tested in fixture folders. Earlier Streamline, MFG and DLSS regression checks also passed. These tests do not certify every game, GPU, driver or mod combination. Nexus downloads require the website step; unavailable metadata is not reported as up to date.

## Download and update
Download **HDLSS-Portable-1.0.6.zip**, close HDLSS and replace HDLSS.exe. Preserve **data**, **data-location.txt** and all backups. Windows x64 / .NET Framework 4.8 required. Source and test results are included; verify the ZIP with the separate SHA-256 file.
