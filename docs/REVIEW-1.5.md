# HDLSS5 1.5 review — 2026-10-23

Reviewed changed install/update/restore paths, source matching, bounded downloads/extraction, UI session cleanup, per-game Vulkan environment, production build gates and portable contents.

Fixed during review:
- CI previously ran diagnostic commands against the production binary. Separate `-TestHooks` build now executes regression tests; production is tested for exit code 2 on diagnostic arguments.
- Button progress could clear during a long operation when the originating button remained disabled. Idle cleanup now requires an enabled button; terminal completion/error and unloaded views still clear the indicator.
- Removed local scans, editor scripts and temporary images from the source publication set. Runtime source additions are included explicitly in build.ps1.

Passed against the release candidate:
- Production and test compilation.
- Full --dlss-test installation, rollback and cleanup suite (exit 0).
- Production diagnostic-command isolation (exit 2; no output file).
- Official ASI 9.7.4 to 10.0.0 update under winmm.dll, byte-identical rollback; foreign/x86 PE rejection.
- F5 v0.1.33 official asset checksum, selection, bounded extraction, installation and removal of LUTs/private Streamline and licenses.
- WPF indicator completion on disabled buttons, long-running disabled state, failure/unload cleanup, red error and color reset.
- WPF XAML rendering, 48px expandable headers and open/close binding.

These checks do not certify every GPU/game/mod combination, Microsoft antivirus clearance, or publisher endorsement. No game files were modified by release tests.
