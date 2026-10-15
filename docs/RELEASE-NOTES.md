# HDLSS5 1.3 — 15 October 2026

## Changes
- Fix ASI Loader installation for protected Game Pass executables by validating matching x64 package manifests and launcher mappings. Loader DLL validation remains separate.
- Keep removal commands below installation commands across the updated component panels.
- Extend optional HDR configuration to detected UE5 games using RenoDX UE Extended, with backups and selective restoration.
- Distinguish enabled native HDR from unverified support. Missing settings do not prove lack of native HDR; configured files do not confirm HDR output.
- Expand the internal guide in Italian, English, Spanish, French, German and Portuguese: preparation, executable selection, component installation, runtime verification, updates, removal and recovery.
- Update the internal changelog, executable version and interface screenshots.

## Install or update
Download HDLSS5-Portable-1.3.zip, close HDLSS5 and extract it into your application folder. Preserve data, data-location.txt and all backups. Launch HDLSS5.exe; update shortcuts if they still point to HDLSS.exe. Install one component at a time with the game closed, then restart the game and verify operation.

## Validation
Passed: protected Game Pass target and rejection cases, invalid ASI library rejection, UE5 HDR configuration/restore preservation, native HDR guard, guide rendering in six languages, current interface rendering. No real game files were modified by these checks.

## Antivirus status
Earlier renamed builds triggered Windows Defender Trojan:Win32/Bearfoos.A!ml. The cause remains unresolved; a false positive has not been confirmed. Version 1.3 has not received Microsoft clearance. Functional tests are not security clearance. The app does not disable protection or add exclusions.
