
Installer, recovery, preserved mods, manual game DLL discovery, DLSS regression and UI checks passed on local fixtures. UE4SS compatibility must be checked per game using a fresh UE4SS.log; some games need a game-specific version/configuration. Custom override.txt installations require manual configuration. Native DLL replacement does not add features absent from the game.

Native HDR INI-only installation and recovery passed isolated tests with and without existing mod files, including SDR, scRGB and PQ configurations. Live Windows HDR capture passed on 4,953,600 pixels. This desktop check does not certify every game. The configured peak is a request to the engine, not a universal output clamp. The automatic ReShade peak filter has been removed from this build.

## Sources
- https://docs.ue4ss.com/installation-guide.html
- https://docs.ue4ss.com/dev/installation-guide.html
- https://github.com/UE4SS-RE/RE-UE4SS/releases
- https://github.com/NVIDIA/DLSS
