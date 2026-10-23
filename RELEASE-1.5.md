# HDLSS5 1.5

## 1.5 — 2026-10-23

- Added OptiScaler F5 in place of NeuRotic Alpha, using stable upstream releases and preserving package LUTs, private Streamline files and licenses.
- Fixed ASI Loader 10.0.0 recognition while retaining PE x64, official-origin and checksum checks; existing proxy names and backups are preserved.
- Added animated button progress with percentages and fixed indicators remaining at 100%. Errors and unsupported states appear in red.
- Improved expandable section contrast, click targets, keyboard focus and scrollbar visibility. Clarified the legacy HDR filter command.
- Added explicit RenoDX catalog availability, including no matching mod, manual downloads and network-error states.
- Improved background game inspection and executable selection. Added per-game Vulkan layers for recognized configurations without global layer registration; native DLSS paths avoid unnecessary Feeder injection.
- Bounded downloads/extraction, preserved cache cleanup and separated production binaries from diagnostic test commands.
- Updated internal guide and changelog. Compatibility still requires verification in game; F5 SDR-to-HDR conversion does not support DLSS-G in v0.1.33.


Download **HDLSS5-Portable-1.5.zip**, extract it into a writable folder and start **HDLSS5.exe**. To update an existing copy, close the app first and preserve its `data` folder and backups. The portable package includes the RTX 4000/5000 NR models. SHA256 checksums are provided.

Validation: production and test builds, installation/recovery regression suite, official ASI 9.7.4 → 10.0.0 update and rollback, F5 package install/remove, WPF progress and menu tests. Game compatibility is not certified by these tests.
