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
