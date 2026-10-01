## Changes
- Add Configure HDR directly to the RenoDX section for The Blood of Dawnwalker, using the documented Unreal Engine Extended settings.
- Enable the HDR output configuration and real-time LUT updates required by this profile.
- Back up settings and restore only unchanged managed values when removing RenoDX. Preserve later user edits.
- Show configuration status separately from verified in-game HDR output, with a dedicated restore button.
- Update interface labels and help in all six languages.

## Validation and scope
55 targeted configuration/UI checks and 19 component regression checks passed. The user confirmed the Dawnwalker HDR setup works in game. This is not a universal UE5 profile: only Dawnwalker is supported by the new configuration action. Profiles are bundled, not refreshed remotely. The removed standalone UE5 HDR editor and luminance overlay remain removed.

## Installation
Close HDLSS and the game, then extract the portable package. Preserve your existing data folder and backups. For supported Dawnwalker installations, open HDR / MFG and use Configure HDR when required.

