**LethalLevelLoader**
--

**A Custom API to support the manual and dynamic integration of custom levels and dungeons in Lethal Company.**

**Thunderstore Link:** *https://thunderstore.io/c/lethal-company/p/IAmBatby/LethalLevelLoader/*

**Discord Thread:** *https://discord.com/channels/1168655651455639582/1193461151636398080*

**Description**
--

### **1.1.0 Has Released! Read The Devlog Here:** *https://github.com/IAmBatby/LethalLevelLoader/wiki/Dev-Logs*

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
* Dynamic Injection Of Custom Scrap Based On Tags, Author, Level Name & Route Price
* Custom DunGen Archetype Injection
* Custom DunGen Line Injection
* Custom DunGen Node Injection
* Custom DunGen Tile Injection
* Custom Weather Integration
* More Config Options

**Known Issues**
--
* Custom ItemGroups will not be found when parsed via the dynamic config.
* Issues with March may exist.
  
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
* **Game-Icons.net** *(For the artwork used for the mod's logo)*

**Changelog**
--


**<details><summary>Version 1.1.0</summary>**

**<details><summary>Features</summary>**

<details><summary>Terminal >preview Keyword</summary>
* *LethalLevelLoader now has a new feature added to the Terminal which allows users to change what information is previewed adjacent to each Moon listed in the `MoonsCatalogue`. This can be toggled via the `preview` verb keyword followed by one of the following options. (LethalLevelLoader also includes a configuration option to set which information type is used by default.)*

* * `preview weather`
* * `preview difficulty`
* * `preview history`
* * `preview all`
* * `preview none`
* * `preview vanilla`
</details>


<details><summary>Terminal >sort Keyword</summary>
* *LethalLevelLoader now has a new feature added to the Terminal which allows users to decide how Moons are sorted when listed in the `MoonsCatalogue`. This can be toggled via the `sort` verb keyword followed by one of the following options. (LethalLevelLoader also includes a configuration option to set which sorting type is used by default.)*

* * `sort price`
* * `sort difficulty`
* * `sort tag`
* * `sort quota`
* * `sort run`
* * `sort none`
</details>

<details><summary>Terminal >filter Keyword</summary>
* *LethalLevelLoader now has a new feature added to the Terminal which allows users to decide which Moons are listed in the `MoonsCatalogue`. This can be toggled via the `filter` verb keyword followed by one of the following options. (LethalLevelLoader also includes a configuration option to set which filtering type is used by default.)*

* * `filter price`
* * `filter weather`
* * `filter tag`
* * `filter last travelled`
* * `filter none`
</details>

<details><summary>Terminal >simulate Keyword</summary>
* *LethalLevelLoader now has a new feature added to the Terminal which allows users to "Simulate" landing on a Moon. This provides a presentable, lore friendly way to view the possible `DungeonFlow` choices with accurate rarity via the Terminal. To use this feature, use `simulate` and a Moon's name, the same way you would use `route`. LethalLevelLoader now includes a configuration option to switch between viewing the `DungeonFlow`'s rarity via raw value or calculated percentage.*
</details>

<details><summary>LevelHistory</summary>
* *LethalLevelLoader now has an experimental `LevelHistory` feature that stores notable information regarding each day in the current save. This includes information such as the Level, DungeonFlow, Weather and more. This feature allows modders and future updates to LethalLevelLoader to create mechanics and systems dependant on the history of the current play session.*
</details>

<details><summary>ExtendedDungeonFlow: Host Decides DungeonFlow & DungeonSize</summary>
* *LethalLevelLoader now modifies the way Lethal Company selects the random `DungeonFlow` and it's dungeon size so only the Host client selects these values which is then sent to the remaining non host clients. This is to help prevent game-breaking dungeon desync when players have mismatching dungeon configuration settings.*
</details>

<details><summary>ExtendedDungeonFlow: Dynamic Weather Rarity Injection</summary>
* *ExtendedDungeonFlow's now contain a `StringWithRarity` list which allows dungeon developers to dynamically inject their dungeon into the current `SelectableLevel`'s possible `DungeonFlow` options.*
</details>

<details><summary>ExtendedDungeonFlow: GlobalProp Dynamic Scaling</summary>
* *ExtendedDungeonFlow's now contain a `GlobalPropCountOverride` list which allows dungeon developers to dynamically increase or increase a `GlobalProp`'s minimum and maximum values based on the currently used dungeon size.*
</details>

<details><summary>ExtendedLevel: MoonCataloguePages & ExtendedLevelGroups</summary>
* *LethalLevelLoader now completely overhauls how the `MoonsCatalogue` TerminalNode functions internally. `ExtendedLevel`'s are now stored in groups via a class named `ExtendedLevelGroup`, These `ExtendedLevelGroup`'s are then stored in groups via a class named `MoonsCataloguePage`. This overhaul allows other mods and future updates to LethalLevelLoader to control and store `ExtendedLevel`s in many ways that were previously limited.*
</details>

<details><summary>ExtendedLevel: Lock Route</summary>
* *ExtendedLevel's now contain a `isLocked` bool and `lockedNodeText` string that controls whether the Level can currently be routed to via the Terminal. When locked the Terminal will display the `lockedNodeText` string as failed routing response on the Terminal (Or a generic response if the string is left empty)*
</details>

<details><summary>ExtendedLevel: Hide Level</summary>
* *ExtendedLevel's now contain a `isHidden` bool that controls whether the Level is displayed in the >Moons Terminal page*
</details>

<details><summary>ExtendedLevel: New Story Log Support</summary>
* *ExtendedLevel's can now add their own custom Story Log's, Without the need of custom code. ExtendedLevel's now contain a `List<StoryLogData>` that takes in a level-dependent `storyLogID` int, a `terminalWord`string, a `storyLogTitle` string and a `storyLogDescription` string.*
</details>

<details><summary>ExtendedLevel: Provide Level Info Description</summary>
* *By default ExtendedLevel's have their >info display text generated using their `SelectableLevel.LevelDescription` string, ExtendedLevel's now have an optional `infoNodeDescription` string if they wish to write their text manually.*
</details>

<details><summary>ExtendedLevel Events</summary>
* *ExtendedLevel's now contain gameplay specific `ExtendedEvent`'s that will Invoke when these events happen while playing the relevant ExtendedLevel.*

* * `onLevelLoaded`
* * `onDaytimeEnemySpawn(EnemyAI)`
* * `onNighttimeEnemySpawn(EnemyAI)`
* * `onStoryLogCollected(StoryLog)`
* * `onApparatusTaken(LungProp)`
* * `onPlayerEnterDungeon(EntranceTeleport, PlayerControllerB)`
* * `onPlayerExitDungeon(EntranceTeleport, PlayerControllerB)`
* * `onPowerSwitchToggle(bool)`
</details>

<details><summary>ExtendedDungeonFlow Events</summary>
* *ExtendedLevel's now contain gameplay specific `ExtendedEvent`'s that will Invoke when these events happen while playing the relevant ExtendedLevel.*


* * `onBeforeDungeonGenerate(RoundManager)`
* * `onSpawnedSyncedObjects(List<GameObject>)`
* * `onSpawnedMapObjects(List<GameObject>)`
* * `onSpawnedScrapObjects(List<GrabbableObject>)`
* * `onEnemySpawnedFromVent(EnemyVent, EnemyAI)`
* * `onApparatusTaken(LungProp)`
* * `onPlayerEnterDungeon(EntranceTeleport, PlayerControllerB)`
* * `onPlayerExitDungeon(EntranceTeleport, PlayerControllerB)`
* * `onPowerSwitchToggle(bool)`
</details>

<details><summary>Default Configuration Options</summary>
* *LethalLevelLoader now provides five new global configuration options.*


* `Default PreviewInfo Toggle`
* * *Controls which Preview Info setting is used when previewing moons via the Terminal `MoonCatalogue`.*
* `Default SortInfo Toggle`
* * *Controls which Sort Info setting is used when previewing moons via the Terminal `MoonCatalogue`.*
* `Default FilterInfo Toggle`
* * *Controls which Filter Info setting is used when previewing moons via the Terminal `MoonCatalogue`.*
* `Default SimulateInfo Toggle`
* * *Controls whether rarity is displayed as it's raw value or a calculated percentage while using the >simulate Terminal keyword.*
* `All DungeonFlows Require Matching`
* * *Experimental setting that forces `DungeonFlow`'s requested by a `SelectableLevel` to have a valid dynamic match. false by default.*
</details>

<details><summary>ExtendedLevel Automatic Configuration Options</summary>
* *LethalLevelLoader now provides automatically generated configuration options for all `ExtendedLevel`'s. This can be disabled by the author of the `ExtendedLevel` if they wish to provide these options themselves.*

* * `enableContentConfiguration`
* * `routePrice`
* * `daySpeedMultiplier`
* * `enablePlanetTime`
* * `isLevelHidden`
* * `isLevelRegistered`
* * `minimumScrapItemSpawnsCount`
* * `maxiumumScrapItemSpawnsCount`
* * `scrapSpawnsList`
* * `maximumInsideEnemyPowerCount`
* * `maxiumumOutsideDaytimeEnemyPowerCount`
* * `maximumOutsideNighttimeEnemyPowerCount`
* * `insideEnemiesList`
* * `outsideDaytimeEnemiesList`
* * `outsideNighttimeEnemiesList`
</details>

<details><summary>ExtendedDungeonFlow Automatic Configuration Options</summary>
* *LethalLevelLoader now provides automatically generated configuration options for all `ExtendedDungeonFlow`'s. This can be disabled by the author of the `ExtendedDungeonFlow` if they wish to provide these options themselves.*
* 
* * `EnableContentConfiguration`
* * `manualContentSourceNameReferenceList`
* * `manualPlanetNameReferenceList`
* * `dynamicLevelTagsReferenceList`
* * `dynamicRoutePriceReferenceList`
* * `enableDynamicDungeonSizeRestriction`
* * `minimumDungeonSizeMultiplier`
* * `maximumDungeonSizeMultiplier`
* * `restrictDungeonSizeScaler`
</details>

<details><summary>Content Config Helper Functions</summary>
* *LethalLevelLoader now provides a variety of helper functions for parsing configuration strings into usuable data. These are used in the `ExtendedLevel` and `ExtendedDungeonFlow` automatic configuration options to ensure standardization.*

* * `List<StringWithRarity> ConvertToStringWithRarityList(string inputString, Vector2 clampRarity)`
* * `List<Vector2WithRarity> ConvertToVector2WithRarityList(string inputString, Vector2 clampRarity)`
* * `List<SpawnableEnemyWithRarity> ConvertToSpawnableEnemyWithRarityList(string inputString, Vector2 clampRarity)`
* * `List<SpawnableItemWithRarity> ConvertToSpawnableItemWithRarityList(string inputString, Vector2 clampRarity)`
* * `(string, string) SplitStringByIndexSeperator(string inputString)`
* * `(string, string) SplitStringByKeyPairSeperator(string inputString)`
* * `(string, string) SplitStringByVectorSeperator(string inputString)`
</details>

<details><summary>Extensions</summary>
* *LethalLevelLoader now provides a variety of helper extensions to assist in creating content in Lethal Company.*

* * `DungeonFlow` `List<Tile>GetTiles()`
* * `DungeonFlow` `List<RandomMapObject>GetRandomMapObjects()`
* * `DungeonFlow` `List<SpawnSyncedObject>GetSpawnSyncedObjects()`

* * `CompatibleNoun` `AddReferences(TerminalKeyword, TerminalNode)`
* * `TerminalKeyword` `AddCompatibleNoun(TerminalKeyword, TerminalNode)`
* * `TerminalNode` `AddCompatibleNoun(TerminalKeyword, TerminalNoun)`
</details>


<details><summary>Async AssetBundle Loading</summary>
* *LethalLevelLoader now loads `.lethalbundle`s asynchronously to improve load times while starting Lethal Company. The progress of the AssetBundle loading can be viewed on the initial game launch options screen.*
</details>

</details>

**<details><summary>Fixes</summary>**

* *The entire codebase has been refactored to streamline functionality, improve stability and reduce errors.*
* *As a safety fallback, LethalLevelLoader will now select the Facility DungeonFlow if there are no DungeonFlow's for the game to select from.*
* *Fixed a Lethal Company bug where game breaks for all clients if a client doesn't finish generating Dungen in one frame*
* *LethalLevelLoader now correctly restores references to base game ItemGroup's found in Custom DungeonFlow's*
* *LethalLevelLoader now correctly restores references to base game ReverbPresets's found in Custom SelectableLevel's and DungeonFlow's*
* *LethalLevelLoader now correctly restores references to base game AudioMixers's found in Custom SelectableLevel's and DungeonFlow's*
* *LethalLevelLoader now correctly restores references to base game AudioMixerController's found in Custom SelectableLevel's and DungeonFlow's*
* *LethalLevelLoader now correctly restores references to base game AudioMixerSnapshots's found in Custom SelectableLevel's and DungeonFlow's*
* *LethalLevelLoader now injects it's random DungeonFlow selection into Lethal Company's random DungeonFlow selection function to improve natural compatibility with other mods. (Thank you BananaPatcher714)*
* *LethalLevelLoader now injects custom DungeonFlow's into Lethal Company's DungeonFlowTypes array to improve natural compatibility with other mods. (Thank you BananaPatcher714)*
* *LethalLevelLoader now injects custom firstTimeDungeonAudio's into Lethal Company's DungeonAudios array to improve natural compatibility with other mods. (Thank you BananaPatcher714)*
* *LethalLevelLoader's dynamic dungeon rarity matching system was overhauled to ensure the highest matching rarity is used, rather than the first matching rarity.*
* *ExtendedLevel's `routePrice` value is now automatically synced with it's associated TerminalNode to ensure dynamic updates to route price are correctly set and reflected on the Terminal.*
* *After references to base game content are restored by LethalLevelLoader, they are now destroyed to avoid issues with other mods obtaining assets via `Resources.FindObjectsOfType()`*
* *Fixed an issue where Custom ExtendedLevel's failed to integrate into the game due to lacking `"FAUNA"` and `"CONDITIONS"` in their `SelectableLevel.LevelDescription`*
* *Modified how LethalLevelLoader accesses the Terminal in order to improve safety and stability in larger modpacks.*
* *Modified how LethalLevelLoader accesses TerminalNode's to avoid errors when playing Lethal Company in different languages. (Thanks Paradox75831004)*
* *Fixed a Lethal Company bug where AudioSource's unintentionally log harmless AudioSpatializer related warnings in the console.
* *Fixed an issue where LethalLevelLoader's dynamic dungeon size clamping was unintentionally being applied.*
* *Fixed an oversight where LethalLevelLoader was logging via Unity rather than Bepinex.*
* *Fixed an issue where `GetTiles()` could potentially trigger null reference exception errors.*
* *Fixed major oversight where Game-Icons.net was not correctly attributed for LethalLevelLoader's logo*

</details>

</details>



**<details><summary>Version 1.0.7</summary>**

* *Overhauled Custom Level system to use dynamically injected scenes rather than dynamically injected prefabs (Thanks onionymous!)*

</details>

**<details><summary>Version 1.0.6</summary>**

* *Moved all logs from Unity.Debug() to BepInEx.ManualLogSauce.LogInfo()*
* *Modified Custom ExtendedLevel loading to initially disable all MeshColliders then reenable them asynchronously to vastly improve load times*
* *Slightly improved manualPlanetNameReferenceList comparison to improve suggested edgecases*
* *Fixed oversight were Terminal moonsListCatalogue was being displayed inaccurately compared to base game implementation*
* *Fixed issue were the NavMesh was incorrectly attempting to bake the Player Ship*

</details>

**<details><summary>Version 1.0.5</summary>**

* *Fixed issue related to SelectableLevel: March not being correctly loaded with it's intended DungeonFlow on additional visits*
* *Revamped manualPlanetNameReferenceList comparison to increase the likelyhood of user inputs working as intended*

</details>

**<details><summary>Version 1.0.4</summary>**

* *Updated LethalLib dependancy from 0.10.1 to 0.11.0*
* *Fixed issues related to SelectableLevel: March not being correctly loaded with it's intended DungeonFlow*
* *Fixed oversight were Custom DungeonFlow's were not having all SpawnSyncedObject's correctly restored*
* *Modified DungeonFlow_Patch levelTags check to increase odds of correctly matching user input*
* *Removed deprecated debug logs*

</details>

**<details><summary>Version 1.0.3</summary>**

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

</details>

**<details><summary>Version 1.0.2</summary>**

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

</details>

**<details><summary>Version 1.0.1</summary>**

* *Updated README*

</details>

**<details><summary>Version 1.0.0</summary>**

* *Initial Release*

</details>

