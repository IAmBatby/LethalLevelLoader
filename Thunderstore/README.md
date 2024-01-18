**LethalLevelLoader**
--

**A Custom API to support the manual and dynamic integration of custom levels and dungeons in Lethal Company.**

**Thunderstore Link:** *https://thunderstore.io/c/lethal-company/p/IAmBatby/LethalLevelLoader/*

**Discord Thread:** *https://discord.com/channels/1168655651455639582/1193461151636398080*

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

* **Evaisa** *(This Mod is directly based from LethalLib's codebase and could have been made without it's pre-existing foundations.)*
* **SkullCrusher** *(This Mod is directly based from SkullCrusher's LethalLib' Fork and could have been made without it's pre-existing foundations.)*
* **HolographicWings** *(This Mod was inspired by LethalExpansion and could not have been made without HolographicWing's support and research.)*
* **KayNetsua** *(This Mod was internally tested using KayNetsua's "E Gypt" Custom Level and KayNetsua assisted in testing LethalLevelLoader's usage)*
* **Badhamknibb** *(This Mod was internally tested using Badhamknibb's "SCP Foundation" Custom Dungeon and Badhamknibb's assisted in testing LethalLevelLoader's usage)*
* **Scoopy** *(This Mod was internally tested using Scoopy's "LethalExtension Castle" Custom Dungeon and Scoopy assisted in testing LethalLevelLoader's usage)*
* **Xilo** *(Xilo provided multiple instances of Bepinex & Unity.Netcode related support during the development of this Mod.)*
* **Lordfirespeed** *(Lordfirespeed provided multiple instances of Bepinex & Unity.Netcode related support during the development of this Mod.)*
* **onionymous** *(Onionymous provided a preview build of their Networked Scene Patcher API, allowing for dynamic, networked scene injection)*

**Changelog**
--

**Version 1.0.7**

* *Overhauled Custom Level system to use dynamically injected scenes rather than dynamically injected prefabs (Thanks onionymous!)*

**Version 1.0.6**

* *Moved all logs from Unity.Debug() to BepInEx.ManualLogSauce.LogInfo()*
* *Modified Custom ExtendedLevel loading to initially disable all MeshColliders then reenable them asynchronously to vastly improve load times*
* *Slightly improved manualPlanetNameReferenceList comparison to improve suggested edgecases*
* *Fixed oversight were Terminal moonsListCatalogue was being displayed inaccurately compared to base game implementation*
* *Fixed issue were the NavMesh was incorrectly attempting to bake the Player Ship*

**Version 1.0.5**

* *Fixed issue related to SelectableLevel: March not being correctly loaded with it's intended DungeonFlow on additional visits*
* *Revamped manualPlanetNameReferenceList comparison to increase the likelyhood of user inputs working as intended*

**Version 1.0.4**

* *Updated LethalLib dependancy from 0.10.1 to 0.11.0*
* *Fixed issues related to SelectableLevel: March not being correctly loaded with it's intended DungeonFlow*
* *Fixed oversight were Custom DungeonFlow's were not having all SpawnSyncedObject's correctly restored*
* *Modified DungeonFlow_Patch levelTags check to increase odds of correctly matching user input*
* *Removed deprecated debug logs*

**Version 1.0.3**

* *Fixed issues caused by the v47 and v48 updates, specific changes will be listed below*
* *Fixed an oversight were ExtendedDungeonFlow dungeonID's were not being assigned correctly*
* *Changed ExtendedDungeonFlow.dungeonRarity variable name to ExtendedDungeonFlow.dungeonDefaultRarity for improved clarity*
* *Moved Prefix Patch Targets From RoundManager to StartOfRound to account for the order of execution changes made in v47*
* *Improved the EntranceTeleport patch to re-organise entranceID settings to avoid user error*
* *Fixed an oversight were PatchDungeonSize() incorrectly checked if the compared values were identical*
* *Moved a majority of public access modifiers to internal to prevent unintential use of internal classes*
* *Fixed an issue were DungeonFlow SpawnSyncObject's were failing to restore their Vanilla reference*
* *Changed ExtendedDungeonFlow.dungeonSizeMin and ExtendedDungeonFlow.dungeonSizeMaz to floats to improve usability*
* *Changed the way the basegame's internal variables are patched to resolve an issue where leaving the game would corrupt saves*
* *Improved debug logs for clarity*

**Version 1.0.2**

* *All Registering of Custom Content has been moved from the GameNetworkManager.Awake() Prefix to the GameNetworkManager.Start() Prefix to give developers safe access to Awake() if needed.*
* *AssetBundleLoader.specifiedFileExtension has now been changed to a public const to allow for improved referencing.*
* *ExtendedDungeonFlow's are now automatically registered with the Network when added using AssetBundleLoader.RegisterExtendedDungeonFlow()*
* *sourceName in ExtendedLevel and ExtendedDungeonFlow have been changed to contentSourceName, to improve clarity.*
* *Fixed an oversight where dungeonSizeMin was not being considered.*
* *Removed deprecated variables from ExtendedDungeonPreferences.*
* *Vector2WithRarity now correctly uses a Vector2, Allowing for improved usability in the Unity inspector.*
* *Variables in ExtendedDungeonPreferences have now been protected with properties, to allow for future validation options.*
* *Removed ExtendedDungeonPreferences, This has now been combined into ExtendedDungeonFlow for better usability and more streamlined referencing.*
* *Refactored ExtendedDungeonFlow to improve on visual organisation when viewed in the Unity inspector.*
* *Refactored ExtendedLevel to improve on visual organisation when viewed in the Unity inspector.*
* *Introduced ConfigHelper.ConvertToStringWithRarity() To assist with developers configeration creation.*
* *Cached Terminal.allTerminalKeywords for improved reference safetey.*
* *Adjusted Harmony Patch Priority Orders from 0 to 350.*

**Version 1.0.1**

* *Updated README*

**Version 1.0.0**

* *Initial Release*

