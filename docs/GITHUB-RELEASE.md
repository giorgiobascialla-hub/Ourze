# HDLSS 1.0.2

- Added installation of the official stable UE4SS end-user runtime and HDLSS reader, with a reviewed file plan and transaction recovery.
- Recognize UE4SS both beside the executable and in the ue4ss subfolder. Preserve existing runtime versions, mod settings and unrelated mods. Explain conflicts with custom overrides, duplicate runtimes or an occupied loader.
- Corrected the search root for manually added Unreal games: native DLSS DLLs in Engine/Plugins and project subdirectories can be detected and selected for updates.
- Native DLSS library updates no longer run the Neural Rendering installation conflict check.
- Corrected component names: DLSS Super Resolution, Ray Reconstruction and Frame Generation are separate from Neural Rendering.
- Separated native UE5 HDR configuration from ReShade/RenoDX: Apply HDR modifies only the two Unreal INIs and never downloads an injector. Existing mods remain untouched. SDR output settings are corrected when requesting HDR.
- Unified scene pixel measurements and optional fresh engine telemetry in the live overlay. Added sample age and explicit last-sample status. The overlay version follows the app version.

## Validation and limits
Installer, recovery, preserved mods, manual game DLL discovery, DLSS regression and UI checks passed on local fixtures. UE4SS compatibility must be checked per game using a fresh UE4SS.log; some games need a game-specific version/configuration. Custom override.txt installations require manual configuration. Native DLL replacement does not add features absent from the game.

Native HDR INI-only installation and recovery passed isolated tests with and without existing mod files, including SDR, scRGB and PQ configurations. Live Windows HDR capture passed on 4,953,600 pixels. This desktop check does not certify every game. The configured peak is a request to the engine, not a universal output clamp. The automatic ReShade peak filter has been removed from this build.

## Sources
- https://docs.ue4ss.com/installation-guide.html
- https://docs.ue4ss.com/dev/installation-guide.html
- https://github.com/UE4SS-RE/RE-UE4SS/releases
- https://github.com/NVIDIA/DLSS

## Download and update
Download HDLSS-Portable-1.0.2.zip. Close HDLSS and replace HDLSS.exe, preserving data, data-location.txt and all backups. Windows x64 and .NET Framework 4.8 required. Source is included.
