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

```text
PASS independent deletion + recovery: autohdr
PASS independent deletion + recovery: pumbo
PASS independent deletion + recovery: lilium
PASS independent deletion + recovery: quint
PASS independent deletion + recovery: sweetfx


PASS manual add-ons do not imply HDR installed
PASS MFG remains independent
PASS Nexus status does not invent latest version
PASS update targets only exact installed file
PASS update applied and other add-ons preserved
PASS update recovery restores original add-on
PASS wrong replacement filename rejected
PASS individual removal preserves other manual add-ons and MFG
PASS actual Nexus archive validates exact RR addon
PASS wrong Nexus archive rejected
PASS 10 add-on checks


PASS PE import table read without executing game
PASS keep loader actually imported by game
PASS exact duplicate deleted; separate folder preserved
PASS different bytes never merged
PASS direct ASI button removes all recognized local copies only
PASS group removal recovery restores both DLLs
PASS inventory detects individual installed components
PASS RenoDX-only deletes DLL and preserves MFG
PASS MFG-only deletes DLL and preserves other early-load entries
PASS ReShade-only leaves independent add-ons intact
PASS ReShade chain disabled on removal
PASS OptiScaler removed while ReShade becomes direct loader
PASS NR model preserved and Opti compatibility DLL removed
PASS shader uninstall removes files even without old baseline backup
PASS new recovery preserves modified shader content
PASS numeric comparison handles padded versions and no downgrade
PASS metadata-only check detects update
PASS offline checks never claim up-to-date
PASS startup check changes no game files
PASS 19 center checks


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
PASS 18181 assertions; 48 WPF renders with inline removal and updates; no overlay types in binary.

```