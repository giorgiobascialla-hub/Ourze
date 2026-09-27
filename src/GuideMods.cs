using System;
namespace HdrPilot {
public static partial class Guide {
 static Topic[] NewTopics(){return new[]{
 new Topic("HDR / MFG",Appearance.Localize("Choose a game and open HDR / MFG. Install ReShade, then the matching RenoDX HDR mod or MFG Unlock. Close the game before applying changes and verify the add-on in the ReShade menu after launch."),
 Appearance.Localize("UE5 HDR and HDLSS overlay"),Appearance.Localize("The UE5 HDR editor, engine reader and HDLSS luminance overlay have been removed. Existing game INIs, mods and backups are preserved. HDR is configured through ReShade and a compatible mod; follow its author if engine HDR must be enabled manually.")),
 new Topic("MFG Unlock",Appearance.Localize(MfgAddon.Requirements),
 Appearance.Localize("Installation and updates"),Appearance.Localize("Install MFG Unlock checks the latest stable release from nefh/MFGAmpereUnlock-RenoDx, verifies its SHA256 and configures early loading in ReShade.ini. Other add-ons and their settings are preserved. Update MFG Unlock checks online again; a failed check never installs an old cached package."),
 Appearance.Localize("Runtime requirements"),Appearance.Localize("The author currently specifies DLSS-G 310.9.1 for the full feature set. Dynamic MFG requires a qualified Direct3D 12 / Streamline 2.14.1 path and driver 595.41 or newer. Do not replace legacy Streamline 1.x with 2.x. HDLSS leaves NVIDIA and Streamline files intact; consult MFG Unlock compatibility for the selected game."),
 Appearance.Localize("Removal"),Appearance.Localize("In the main game page, use the individual removal buttons. Mod updates and Check again are on the same page. Other components remain on disk.")),
 new Topic("ReShade / RenoDX",Appearance.Localize("Stable ReShade uses the latest official full add-on package; nightly uses the latest successful development build. RenoDX is matched against the current game catalog and checks recent published assets. Unsupported or ambiguous packages stop the installation. Local imports use the file you select and cannot certify that it is the latest release."),
 Appearance.Localize("Separate components"),Appearance.Localize("MFG Unlock is separate from HDR RenoDX and OptiScaler Frame Generation. Installed files are not proof of activation. Check the ReShade log and the MFG Unlock panel in the running game. MFG Unlock does not add native DLSS FG to a game that lacks it.")),
 new Topic("DLSS / OptiScaler",Appearance.Localize("The DLSS section uses vertical groups: Neural Rendering, Frame Generation, DLSS components and Customize. Buttons retain their original style. Install OptiScaler and Apply settings remain visible at the bottom; scroll to reach details and recovery tools.")),
 new Topic(Appearance.Localize("Manual add-on"),Appearance.Localize("Manual add-ons are listed separately from managed RenoDX HDR. Each has its own removal button. Nexus updates require the website download; unknown sources use Import update. Only the matching add-on is replaced, with backup.")),
 new Topic(Appearance.Localize("Add-on e shader ReShade"),Appearance.Localize("Select a package, then Install selected or Remove selected. Yellow means installed, not update available. Updates are listed separately.")),
 new Topic("ASI Loader",Appearance.Localize(AsiLoader.Help)),
 new Topic(Appearance.Localize("Removal"),Appearance.Localize("Remove one component at a time. Files are deleted with a recovery copy; other mods are preserved.")),
 new Topic(Appearance.Localize("Mod updates"),Appearance.Localize("Checks run at startup. Updates are applied only when you choose them, with backup.")),
 new Topic(Appearance.Localize("Clean duplicate loaders"),Appearance.Localize("Only byte-identical mod loaders in this folder are removed. One loader imported by the game is kept; native DLL copies in other folders are preserved.")),
 new Topic("Streamline",Appearance.Localize(Streamline.Help)),
 new Topic("UE4SS",Appearance.Localize("Install UE4SS for mods downloads the latest official stable runtime for a supported Unreal game, without the HDLSS telemetry reader. Existing UE4SS and mods are preserved. Check compatibility with the game and its mods before changing a runtime.")),
 new Topic(Appearance.Localize("Backups and updates"),Appearance.Localize("Keep the portable data folder when replacing the executable. Preferences, language and backups are reused. Downloads are checked online for each installation; no update is silently applied at game launch. Review the file plan, apply with backup, then test in game. Neither every game nor every GPU combination can be certified by file checks."))
 };}
}
}