# HDLSS5 1.2 — revision 3 (14 October 2026)

- Recognize protected Game Pass Unreal Shipping targets through matching x64 package manifests and launcher names; protected WindowsApps remain excluded.
- Place removal actions below installation actions for Neural Rendering, effects, REFramework and OptiScaler.
- Offer opt-in Engine.ini HDR configuration for detected UE5 games using RenoDX UE Extended, with backup and restore. Enabled native HDR is distinguished from unverified support; missing settings do not prove absent native support. HDR output must be checked in game.
- Update internal help and change notes in six languages, and refresh interface screenshots.

Download HDLSS5-Portable-1.2-r3.zip. Close the app, preserve data and backups, extract the package, and use HDLSS5.exe. Update shortcuts from HDLSS.exe.

Antivirus status: an earlier renamed build and its unchanged rebuild triggered Trojan:Win32/Bearfoos.A!ml. The cause remains unresolved and a false positive has not been confirmed. This revision has not received Microsoft clearance. Functional tests are not security clearance. No protection changes or exclusions are included.

Validation: generic UE5 eligibility, native HDR guard, INI preservation and restore tests; Game Pass target and negative checks; guide rendering in six languages; current UI screenshot rendering. No installation into real game folders was performed for these checks.
