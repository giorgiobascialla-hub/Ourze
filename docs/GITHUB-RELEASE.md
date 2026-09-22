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
