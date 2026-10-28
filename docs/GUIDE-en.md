# HDLSS5 1.5.1

## 01 · Interface overview

HDLSS5 organizes component configuration by game. Select a game in the library before opening its component page.

### Main window

The cover and title identify the selected game. Search filters the library. Open folder opens its location. Remove from library hides the entry; it does not uninstall the game.

### Three component tabs

DLSS 5 · OptiScaler contains Standard and F5 · Janblade. DLSS 5 · ReShade contains the ReShade and Neural Rendering steps. DLL and Streamline manages libraries and models. Optional effects and advanced loaders are separate.

### General settings

Settings contains app-wide options such as language, theme and library management. Guide explains procedures; Backup concerns recovery. Details expands component-specific instructions. Example: to update a game's DLL, open DLL and Streamline rather than general settings.

## 02 · Colors, buttons and status

Read the component status before acting. Color supports the message and does not replace it.

### Colors

Magenta means an update has been detected. Blue indicates a normal action, installation or check. Yellow highlights an installed component or selection depending on the control. Red indicates an error, incompatibility or failed check. Dark, faded buttons are disabled but remain visible.

### Versions and progress

Installed version describes detected files. Available version comes from the check or catalog. A local/included catalog is not a fresh online verification. During an operation, a button may show progress and a percentage: wait for the result rather than assuming 100% means success.

### Example

If the status says Up to date and Update is dimmed, reinstallation is unnecessary. If verification failed, retry the check when the connection is available. Installed files do not prove an effect is active; verify in a playable scene.

## 03 · Library and executable

Select or add the game first. Close it before installing, updating or removing component files.

### Find games

Scan searches supported library locations. Add game lets you specify an installation that was not detected. Restore hidden games recovers hidden entries. Changing a cover affects the library presentation.

### Automatic executable selection

HDLSS5 tries to identify the rendering executable and destination. An ambiguous result can require verification or block installation. Executable and graphics is for advanced corrections; changing the declared API does not convert the game's renderer.

### Launchers and Game Pass

A launcher may start another process that renders the game. Protected files or unwritable folders can prevent installation. Example: check the proposed rendering executable instead of blindly choosing launcher.exe. Do not remove system protections to force installation.

## 04 · Choose an integration

OptiScaler and ReShade are separate paths. Choose according to the requested feature and game compatibility.

### OptiScaler path

Select Standard or F5 · Janblade in DLSS 5 · OptiScaler. Standard does not automatically include the F5 fork's NR features. Available options depend on the selected build.

### ReShade path

Follow 1. Install ReShade, then 2. Install Neural Rendering. HDR, MFG and shaders are optional. ASI Loader, REFramework and UE4SS are needed only by mods that require them.

### One change at a time

Avoid overlapping active NR integrations. Example: install ReShade, verify its menu, then add NR and compare the same scene. Preserve a known working configuration before further experiments.

## 05 · Standard and F5 · Janblade

Only OptiScaler Standard and F5 · Janblade are offered. Installed and latest checked versions are shown separately.

### Install and update

Select the build and use Install / change build. Check updates queries that build's release source. A newer F5 release makes its Update button magenta. A local package is not proof of the latest online version.

### Settings

Use the dedicated save/apply command after changing settings. NR multipass and HDR depend on the fork and version. A saved setting does not prove the feature is running. More passes can increase GPU cost rather than FPS.

### Example

Keep F5 selected when updating F5, run the check and use its update button. Restart the game and verify image quality and operation. Switching to Standard can remove access to fork-specific features.

## 06 · ReShade: step one

ReShade loads the add-ons and shaders used by this integration. Installing it alone does not activate NR or HDR.

### Procedure

1. Close and select the game. 2. Open DLSS 5 · ReShade. 3. Install ReShade. 4. Read the result. 5. Launch the game and open ReShade using its configured key. The key can be changed under ReShade Details.

### Channel and loader

Details contains stable/nightly selection, import and menu options. Use nightly when required or for a deliberate test. Clean duplicate loaders is maintenance, not a generic first response to every error; review the proposed changes.

### Example

If ReShade does not appear, check the rendering process, API and logs. Do not add duplicate loaders with different filenames. An existing DLL may belong to another mod and block replacement.

## 07 · Neural Rendering: step two

The ReShade NR installation prepares the selected add-on and GPU-specific model when hardware detection and requirements allow it.

### Procedure

1. Install ReShade with add-on support. 2. Check the native-DLSS or supported non-DLSS path. 3. Choose NR Lecram 10.8 or ShortFuse. 4. Install/update RenoDX NR. 5. Restart and enable/verify NR in the ReShade menu in a playable scene.

### Add-on versus model

The add-on integrates with ReShade; nvngx_dlssnr.dll is the model. Included 310.9.16.0 variants distinguish RTX 4000 Compat FP16 / CUDA 13.4.3 and RTX 5000 original. GPU detection selects the path; separate model management remains in DLL and Streamline. Resolve uncertain hardware or requirements first.

### Example

Choosing ShortFuse changes the add-on, not the GPU family. Start simple and compare FPS and image quality in the same scene. NR can lower performance and does not guarantee universal compatibility, HDR or FG.

## 08 · Non-DLSS, Vulkan and emulators

Compatibility depends on the rendering process and available integration, not simply the age of a game or use of a launcher.

### Feeder

Feeder is a separate path for supplying NR data in supported non-DLSS configurations. Do not automatically add it to a working native-DLSS path. Not every game without DLSS becomes compatible.

### Other graphics APIs

Vulkan integration is handled for recognized configurations. Selecting DirectX 12 does not convert DX11, DX9 or Vulkan. External bridges and mods have their own compatibility requirements.

### Example

With a launcher and emulator, identify the process loading ReShade and the component providing DLSS/FG. Test the launcher first without NR, then add one component at a time. Load a playable scene; the title menu alone is insufficient.

## 09 · Optional HDR, MFG and shaders

Optional add-ons are not required steps for the basic NR setup.

### RenoDX HDR

The app consults the RenoDX catalog and the mod's associated sources, which may include GitHub, download sites and Nexus. No matching mod differs from an unavailable catalog check. Review author and origin when confirmation is required. Nexus may need a browser download confirmation; local import accepts separately obtained packages.

### Game HDR

Configure HDR is offered only for recognized configurations. HDR configured describes prepared settings rather than a measured video output. Restart and verify the display, Windows HDR and mod in a scene. Avoid stacking HDR conversions without compatible instructions.

### MFG and shaders

MFG Unlock is separate from NR and requires a compatible FG path. It does not add FG to every game. Select Lilium or CAS under ReShade shaders. If CAS is included in Lilium, enable it there. Example: use CAS for sharpening without installing unrelated advanced loaders.

## 10 · DLL and Streamline

Super Resolution, Ray Reconstruction, Frame Generation, Streamline and NR models are different components.

### Update procedure

Close the game, open DLL and Streamline, inspect the installed version, select a version and install/update. The app determines the destination. Storage and backup copies are not game targets. Resolve executable ambiguity before proceeding.

### Catalog and limits

Included catalogs can show choices offline, but uncached files require downloading. Checking again refreshes availability. Replacing a DLL updates an existing integration; it does not add DLSS or FG to an unsupported game.

### Example

Update only Super Resolution first and test before changing another component. If it causes a problem, use Restore original DLLs in that component's card.

## 11 · Advanced mod loaders

Open Loaders for other mods only when a mod requires one.

### ASI Loader

It loads .asi files and does not install the mods themselves. Choose the DLL filename specified by the mod author. The app validates x64 packages and updates recognized loaders; duplicates or unknown DLLs may block the operation.

### REFramework and UE4SS

REFramework is offered for recognized RE Engine games; legacy packages can differ from current ones. UE4SS is a runtime for Unreal mods. Engine detection does not guarantee compatibility with every mod or runtime version.

### Example

If a mod requires winmm.dll, do not choose dinput8.dll merely because it appears in the menu. Identify anything already using the filename. External scripts and mods can require their own removal procedures.

## 12 · Restore, removal and backups

Remove deletes a managed component; Restore recovers a preserved copy. They serve different purposes.

### Original DLLs

Restore original DLLs recovers the initial backup saved by the app for that component and game, including managed copies in multiple locations. The initial backup may already contain pre-existing modifications. Without a backup, use the launcher's file verification to recover store-distributed files.

### Working configuration

Save a DLL configuration only after testing it in game. Restore saved configuration returns to that state, not necessarily the original installation. Preserve data and recovery copies when updating HDLSS5.

### Example

After a failed update, restore the last changed component first. Do not delete the entire game folder. Removing ReShade can leave add-ons on disk that no longer load; manage them through their respective cards.

## 13 · Updates and disk space

Checking updates and installing updates are separate actions. A check does not change game files.

### Status and controls

Update is magenta when an update is available. Unavailable actions are dimmed. Network errors, unknown versions and up-to-date components are distinct states. Test one change at a time and consult Recent activity for failures.

### Cache and models

Automatic cleanup limits managed download cache; referenced paths may be retained. This is not a size limit for the entire app folder. Bundled NR models and recovery backups occupy separate space. Keep nr-packages if using included models.

### Update HDLSS5

Close the app, extract into a writable folder and preserve data, backups and nr-packages. Example: when given only a new executable, replace it while the app is closed without deleting adjacent folders.

## 14 · Troubleshooting and verification

Use the exact message and affected component to choose the next step.

### Effect missing

Check the selected game, rendering executable, graphics API, GPU, loader and prerequisites. Open the component menu in a playable scene. Do not change a declared API merely to enable an installation button.

### Startup failure

Close the game, review Recent activity and component logs, then restore the last change. External mods may conflict and cannot all be detected. Anti-cheat may block changed DLLs; HDLSS5 does not disable anti-cheat or guarantee mod acceptance.

### Antivirus and support

The earlier Defender detection has not been confirmed resolved. Functional tests do not certify antivirus clearance. Keep the detection name, app version and file hash for vendor analysis. Support reports should include launcher, GPU, versions, action, relevant path and complete error, without credentials.

## 15 · Version 1.5.1 changes

28 October 2026. Rewritten guide follows the actual workflow: interface, library, configuration, components, restore and troubleshooting.

### Interface

Aligned installation, removal and Details controls; consistent typography and spacing. Shader descriptions moved to Details. Available updates are magenta; disabled actions are visibly dimmed.

### OptiScaler

Standard and F5 · Janblade are the only offered builds. F5 updates are more visible and versioned packages accept the v prefix. The guide no longer instructs users to select retired forks or manually choose DLL targets.

### Validation and limits

Build and regression tests verify application behavior; UI tests verify presentation and alignment. Game compatibility, performance and antivirus classification require separate checks.
