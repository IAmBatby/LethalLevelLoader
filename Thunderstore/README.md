**LethalLevelLoader**
--

**A Custom API to support the manual and dynamic integration of custom levels and dungeons in Lethal Company.**

**Thunderstore Link:**

**Discord Thread:**

**Description**
--

**PLEASE NOTE:** This Mod is currently very early in development. Use with the understanding that there will be issues that cannot be resolved until more practical testing and bug reporting occurs.

**LethalLevelLoader** is a custom API to support the manual and dynamic integration of custom levels and dungeons in Lethal Company. 
Mod Developers can provide LethalLevelLoader with their custom content via code or via automatic assetbundle detection, From there LethalLevelLoader will seamlessly load it into the game.

This API is dependant on **LethalLib**, As it was originaly intended to be a direct update to Evaisa & SkullCrusher's mod. 
It has temporaily been split into this secondary release to allow for more unstable releases, in order to stress test it's systems and collect bug reports from active developers and players.

Currently all Item & Scrap related systems have not been implemented in this mod and should use LethalLib's systems instead.

This Mod is Likely To Be Incompataible with **LethalExpansion**, Due To The inherit conflicts involved in changing the same systems.

**How To Use (Users / Players)**
--


  Simply install LethalLevelLoader and LethalLib.


  If a mod using **LethalLevelLoader** supplies a **.lethalbundle** file, **LethalLevelLoader** will automatically find and load it’s content as long as it’s in the /plugins/ folder (Subfolders will be detected)

**How To Use (Modders / Developers)**
--


  Please refer to the LethalLevelLoader Wiki for documentation on utalising this API for your custom content
  https://github.com/IAmBatby/LethalLevelLoader/wiki

**Features Currently Supported**
--
* Manual Custom SelectableLevel (Moon) Integration & Injection
* Automatic Custom SelectableLevel (Moon) Integration & Injection Via AssetBundles
* Manual Custom DungeonFlow (Dungeon) Integration & Injection
* Automatic TerminalKeyword & TerminalNode creation for Custom SelectableLevels’s
* Automatic Injection Of DungeonFlows Into SelectableLevel's Based On Level Tags, Author, Level Name & Route Price
* Controllable Overriding Of Dungeon Size Multiplier
* Automatic Custom SelectableLevel Content NetworkObject Registration
* Manual Custom DungeonFlow Content NetworkObject Registration
* Automatic Custom SelectableLevel Content Re-Assigning For References To Vanilla Content
* Automatic Custom DungeonFlow Content Re-Assigning For References To Vanilla Content
* Automatic Warmup Of Shaders Found In Custom SelectableLevel's And Custom DungeonFlow's

**Features Currently Unsupported (Upcoming)**
--
* Generation Of Multiple Dungeons In A Single SelectableLevel
* Dynamic Terminal >moons List Categories / Groups
* Dynamic Injection Of Custom Scrap Based On Tags, Author, Level Name & Route Price
* Asynchronous AssetBundle Loading
* Custom DunGen Archetype Injection
* Custom DunGen Line Injection
* Custom DunGen Node Injection
* Custom DunGen Tile Injection
* Custom Weather Integration
* More Config Options

**Known Issues**
--
* Automatic Generation Of Level >info TerminalNode's Is Currently Broken
* FirstEnterDungeonAudio's Are Possibly Broken
* Certain AudioMixerGroup's In Custom Content Are Possibly Misassigned.
  
**Credits**
--

* **Evaisa** *(This Mod is directly based from LethalLib's codebase and couldn't have been made without it's pre-existing foundations.)*
* **SkullCrusher** *(This Mod is directly based from SkullCrusher's LethalLib' Fork and couldn't have been made without it's pre-existing foundations.)*
* **HolographicWings** *(This Mod was inspired by LethalExpansion and couldn't have been made without HolographicWing's support and research.)*
* **KayNetsua** *(This Mod was internally tested using KayNetsua's "E Gypt" Custom Level and KayNetsua assisted in testing LethalLevelLoader's usage)*
* **Badhamknibb** *(This Mod was internally tested using Badhamknibb's "SCP Foundation" Custom Dungeon and Badhamknibb's assisted in testing LethalLevelLoader's usage)*
* **Scoopy** *(This Mod was internally tested using Scoopy's "LethalExtension Castle" Custom Dungeon and Scoopy assisted in testing LethalLevelLoader's usage)*
* **Xilo** *(Xilo provided multiple instances of Bepinex & Unity.Netcode related support during the development of this Mod.)*
* **Lordfirespeed** *(Lordfirespeed provided multiple instances of Bepinex & Unity.Netcode related support during the development of this Mod.)*
