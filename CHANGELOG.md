# HDLSS5 1.4 — 18 October 2026

## Novità / Changes
- Installazione guidata: **1. ReShade → 2. Neural Rendering**. L’installazione NR include il modello **310.9.16.0** adatto alla GPU rilevata: RTX 4000 Compat FP16 oppure RTX 5000. Scelta tra add-on **8.5-model** e **ShortFuse**.
- Rilevamento automatico dell’eseguibile e dell’API quando verificabili; i casi ambigui richiedono verifica, senza imporre DirectX 12 a giochi incompatibili.
- Add-on opzionali RenoDX HDR, MFG e shader raggruppati; loader avanzati separati. Menu generale a ingranaggio, azioni del gioco nella barra e layout migliorato anche in 16:9.
- Cache dei download verificata e riutilizzata, ridotta all’avvio a circa 2 GB dove possibile. Backup, recuperi, sorgenti selezionate e modelli inclusi restano conservati.
- Destinazioni DLL automatiche e un comando di ripristino per componente. Libreria con apertura cartella, rimozione dalla libreria e copertine uniformi.
- MFG Unlock dal repository mavismmg/MFGAdaUnlock-RenoDx, con controllo delle future versioni stabili. Corretto il falso riconoscimento di altri loader come OptiScaler.
- Guida e changelog interni aggiornati in sei lingue; nuove schermate dell’interfaccia.

## English
Two-step ReShade/Neural Rendering setup now installs the bundled GPU-specific NR 310.9.16.0 model together with the selected 8.5-model or ShortFuse add-on. This release also improves automatic executable/API and DLL target detection, per-component restoration, library actions, cover presentation, optional add-on organization, settings access and widescreen layout. Verified download caching reduces repeated downloads and trims disposable cache at startup while preserving backups and recovery copies. MFG updates follow the upstream stable releases. The guide and in-app changelog are available in six languages.

## Install / update
Close HDLSS5 and extract **HDLSS5-Portable-1.4.zip** into the application folder. Run **HDLSS5.exe**. Keep `data`, `data-location.txt`, all backups and the included `nr-packages` folder. No developer tools are required. The bundled NR models account for most of the download size; they are not downloaded again by the guided installer. Close the selected game before installing components and verify activation in game after restarting.

## Validation
Build and installation/removal regression tests passed. Additional fixture checks covered GPU/model selection, add-on switching, backup preservation, cache integrity and graphics API detection. Interface rendering was checked at desktop widths including 1920 pixels. These checks do not guarantee compatibility or performance in every game.

---

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
