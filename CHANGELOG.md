**Changelog**
--

**<details><summary>Version 1.4.11</summary>**

**<details><summary>Fixes</summary>**

* Added additional check to hotloading system to avoid issues when leaving a lobby as a host (Thank you Zaggy)
* Added additional validation checks to ExtendedBuyableVehicle to avoid errors with incorrectly setup content (Thank you ScandalTheVandal)

</details>

</details>

**<details><summary>Version 1.4.10</summary>**

**<details><summary>Fixes</summary>**

* Small hotfix

</details>

</details>

**<details><summary>Version 1.4.9</summary>**

**<details><summary>Fixes</summary>**

* Made change to improve issue related to host leaving a multiplayer session
* Added log when client fails to import ExtendedLevel saved data to further troubleshoot ongoing issues

</details>

</details>

**<details><summary>Version 1.4.8</summary>**

**<details><summary>Fixes</summary>**

* Fixed issue with LethalLib SoftDependency

</details>

</details>

**<details><summary>Version 1.4.7</summary>**

**<details><summary>Fixes</summary>**

* Tweaked NetworkBundleManager code to improve realibility of the hot-reloading system and prevent unintentional soft-locks related to pulling the lever

</details>

</details>

**<details><summary>Version 1.4.6</summary>**

**<details><summary>Features</summary>**

* Added ExtendedLevel.IsRouteRemoved to indicate if Level has been disabled in the config

</details>

**<details><summary>Fixes</summary>**

* Fixed issue with AssetBundle hotloading when routing from a moon to a moon contained in the same AssetBundle
* Added additional safetey check when performing Audio related Asset restoration
* Changed ExtendedItem terminal registration to ensure accurate item Ids
* Fixed an issue with Locked ExtendedLevel's by setting it's accossiated TerminalNode.acceptEverything to false


</details>

</details>

**<details><summary>Version 1.4.5</summary>**

**<details><summary>Fixes</summary>**

* Fixed issue where SpawnSyncedObjects in Tile Injection TileSets were not being network registered

</details>

</details>

**<details><summary>Version 1.4.4</summary>**

**<details><summary>Fixes</summary>**

* Fixed additional networking issues related to AssetBundle Hotloading

</details>

</details>

**<details><summary>Version 1.4.3</summary>**

**<details><summary>Fixes</summary>**

* Fixed issue with onBundlesFinishedLoading callback not being invoked
* Re-wrote AssetBundle Hotloading networking to hopefully improve issues in multiplayer and pre-existing saves
* Fixed issue with custom content that is manually registered not being processed correctly

</details>

</details>

**<details><summary>Version 1.4.2</summary>**

**<details><summary>Features</summary>**

* Added OverrideNoun value to ExtendedLevel, allowing authors to optionally modify the word used when accessing the level from the Terminal

</details>

**<details><summary>Fixes</summary>**

* Fixed issue with ContentTags failing to be applied to Vanilla content

</details>

</details>

**<details><summary>Version 1.4.1</summary>**

**<details><summary>Fixes</summary>**

* Fixed issue with older mods not being registered correctly
* Improved stalling during AssetBundle unloading on initial load

</details>

</details>

**<details><summary>Version 1.4.0</summary>**

**<details><summary>Features</summary>**

* Overhauled AssetBundleLoading system
* Added Scene AssetBundle hot-reloading


</details>

</details>

**<details><summary>Version 1.3.8</summary>**

**<details><summary>Features</summary>**

* Added interior selection history to DayHistory

</details>

</details>

**<details><summary>Version 1.3.7</summary>**

**<details><summary>Fixes</summary>**

* Added additional safety checks to ExtendedFootstepSurface patches

</details>

</details>

**<details><summary>Version 1.3.6</summary>**

**<details><summary>Features</summary>**

* Added content restoration support for basegame water shader

</details>

</details>

**<details><summary>Version 1.3.5</summary>**

**<details><summary>Fixes</summary>**

* Added safety checks to new FootstepSurface Material cache system.
* Added safety checks to new ExtendedLevel override fog size feature.
* Changed ContentRestoring of EnemyType's to use ScriptableObject name rather than enemyName
* Fixed issue where synced audio clip that plays when previewing enemy beastiary file was not playing for custom enemies
* Fixed issue where custom Enemy beastiary files did not have Info as a default keyword

</details>

</details>

**<details><summary>Version 1.3.4</summary>**

**<details><summary>Fixes</summary>**

* Updated .csproj and thunderstore.toml
* Updated outdated README.md

</details>

</details>

**<details><summary>Version 1.3.3</summary>**

**<details><summary>Features</summary>**

* Implemented ExtendedFootstepSurface

</details>

</details>

**<details><summary>Version 1.3.2</summary>**

**<details><summary>Fixes</summary>**

* Updated onApparatusTaken ExtendedEvent

</details>

</details>

**<details><summary>Version 1.3.1</summary>**

**<details><summary>Fixes</summary>**

* Fixed issue regarding deprecated Level list.

</details>

</details>


**<details><summary>Version 1.3.0</summary>**

**<details><summary>Features</summary>**

* Updated mod for Lethal Company version 56
* Added initial ExtendedBuyableVehicle implementation
* Added LevelEvents.onShipLand ExtendedEvent
* Added LevelEvents.onShipLeave ExtendedEvent
* Added ExtendedLevel.OverrideDustStormVolumeSize Value
* Added ExtendedLevel.OverrideFoggyVolumeSize Value
* Added PatchedContent.TryGetExtendedContent() Function.
* Added public references to basegame manager instances to OriginalContent
* Updated ContentTag's for new Version 55/56 content

</details>

**<details><summary>Fixes</summary>**

* Fixed issue with onApparatusTaken event running at unintended moments
* Added safeguard to prevent multiple Lethalbundles with identical names from causing a gamebreaking error
* Moved LethalLib from a Hard Dependency to a Soft Dependency
* Fixed ExtendedItem's not correctly playing the purchased SFX when purchased from Terminal
* Merged various pull requests to improve the workflow and deployment of further LethalLevelLoader development

</details>

</details>

**<details><summary>Version 1.2.2</summary>**

**<details><summary>Fixes</summary>**

* Fixed issue where vanilla items were being incorrectly destroyed when playing multiple lobbies during the same game session
* Restored functionality of the ExtendedLevel.IsRouteLocked feature
* Added safety check to help prevent saves made in pre 1.2.0 LethalLevelLoader modpacks from corrupting when being used
* Fixed issues with ExtendedDungeonFlow.DynamicDungeonSize related settings incorrectly applying after version 50 changes
* Removed ExtendedMod.ContentTagAsStrings() function
* Added ExtendedMod.TryGetTag(string tag) function
* Added ExtendedMod.TryGetTag(string tag, out ContentTag contentTag) function
* Added ExtendedMod.TryAddTag(string tag) function

</details>

</details>

**<details><summary>Version 1.2.1</summary>**

**<details><summary>Fixes</summary>**

* Updated LICENSE
* Changed accessor for ExtendedDungeonFlow.GenerateAutomaticConfigurationOptions from internal to public
* Fixed issue where ExtendedDungeonFlow.GenerationAutomaticConfigurationOptions was defaulting to false
* Changed accessor for EnemyManager.RefreshDynamicEnemyTypeRarityOnAllExtendedLevels from internal to public
* Changed accessor for EnemyManager.InjectCustomEnemyTypesIntoLevelViaDynamicRarity from internal to public
* Changed accessor for ItemManager.RefreshDynamicItemRarityOnAllExtendedLevels from internal to public
* Changed accessor for ItemManager.InjectCustomItemsIntoLevelViaDynamicRarity from internal to public
* Changed ConfigLoader default dungeon binding to list current level matching values as default values
* Added "Killable" ContentTag to Forest Giant
* Added "Chargable" ContentTag to Jetpack
* Added "Weapon" ContentTag to Knife
* Added additional developer debug logging for the scene validation and selection process

</details>

</details>


**<details><summary>Version 1.2.0</summary>**

**<details><summary>Features</summary>**

* Updated mod for Lethal Company version 50

<details><summary>General</summary>

* Added ExtendedMod
* Added ExtendedEnemyType
* Added ExtendedItem
* Added ExtendedStoryLog
* Added ExtendedFootstepSurface (WIP)
* Added ExtendedWeatherEffect (WIP)
* Added LevelMatchingProperties
* Added DungeonMatchingProperties
* Added ContentTags

* Added Global LevelEvents Instance (Thanks mrov)
* Added Global DungeonEvents Instance (Thanks mrov)
* Added IsSetupComplete bool for modders to reference.
* Added onBeforeSetup event for modders to reference
* Added onSetupComplete event for modders to reference
* Revamped DebugLogs and provided a configurable debuglog setting in the config to allow Users to only receive relevant logs by default.
* Moved AssetBundleLoading earlier to help speed up load time
* Revamped debug logs when trying to load a level or simulate the loading of a level
* Revamped Moons Catalogue display to split custom moons into groups similar to the basegame moon listings.
* Revamped Moons Catalogue display to order custom moon groups by average risk level
* Revamped Moons Catalogue display to order custom moons inside groups by risk level
* Revamped Moons Catalogue display to prefer to group custom moons created by the same author
* Revamped Moons Catalogue display to dynamically adjust font size depending on the amount of Moons being displayed
* Probably a lot more!

</details>

<details><summary>ExtendedLevel</summary>

* Added string value to allow Authors to use custom route node display text to their levels
* Added string value to allow Authors to use custom route confirmation node display text to their levels
By default SelectableLevel.riskLevel is now automatically assigned using calculations and comparisons of SelectableLevel values between both Custom and Vanilla levels. This can be manually overridden.
* Added an OverrideQuicksandPrefab value to allow authors to modify the Quicksand used on their level
* Added ShipFlyToMoonClip & ShipFlyFromMoonClip AnimationClip values to allow authors to modify the AnimationClips used when the Ship lands to and from their level (Currently disabled until bug is resolved with Unity Assetrip Fixer)
* Overhauled the way Scene’s are correlated with Levels by implementing a new weight based system built into ExtendedLevel to allow authors to randomly switch between multiple variant scenes for a single level.

</details>

<details><summary>ExtendedDungeonFlow</summary>

* Added an OverrideKeyPrefab value to allow authors to modify the Key prefab used in their Dungeon
* Added a MapTileSize value to allow authors to set a correlated MapTileSize value that is used in new basegame functions implemented in Version 50.
* Added a new SpawnableMapObjects list value to allow authors to inject custom RandomMapObjects in their Dungeon

</details>

<details><summary>ExtendedItem</summary>

* Custom Item support has now been added.
* Added a PluralisedItemName string value to allow developers to change how their item name is parsed when being referenced as a plural (eg. when buying multiple of them from the store)

</details>

<details><summary>ExtendedEnemyType</summary>

* Custom Enemy support has now been added.

</details>

<details><summary>ExtendedStoryLog</summary>

* Custom StoryLog support has now been added.

</details>

<details><summary>ExtendedFootstepSurfaces</summary>

* Custom FootstepSurface support has now been added. (Currently disabled)

</details>

<details><summary>ExtendedWeatherEffect</summary>

* Custom WeatherEffect support has now been added. (Currently disabled)

</details>

<details><summary>ContentTags & MatchingProperties</summary>

* Created integrated ContentTag system that allows developers to put relevant string tags on all types of custom content (with an optional correlating colour). Developers can access groups of content based on a specific content tag as well as match their content with other pieces of content dynamically using the built in LevelMatchingProperties and DungeonMatchingProperties.
* All Vanilla content has been manually assigned Content Tags to allow developers to reference vanilla content via tags the same way they would custom content, You can find those tags here: https://docs.google.com/spreadsheets/d/1WO77KGJplIEC64qmBClOgfEEoFxrhMurCEqe9FKod8I/edit?usp=sharing


</details>

</details>

**<details><summary>Fixes</summary>**

* Fixed switch Terminal command incorrectly working
* Fixed Weather selection desyncing
* Fixed Dungeon selection desyncing
* Fixed Config duplicating entities (Credit to mrov)
* Added safety checks to correctly save and restore previously selected route and prevent previous routes to disabled levels from breaking
* Added safety checks to prevent invalid Foggy weather level values from breaking the game
* Added safety checks to prevent Levels & Dungeons having incorrect SpawnableMapObject setups from breaking the game
* Added safety check to prevent level missing MapPropsContainer tagged object from breaking the game
* Added safety check to prevent level with .SpawnScrapAndEnemies enabled and no spawnable scrap listed from breaking the game
* Fixed LevelEvents & DungeonEvents EntranceTeleport events behaving incorrectly (credit to mrov)
* Added custom code to optimize specific internal code used in DunGen generation (Credit to LadyRaphtalia)
* Made LogDayHistory function safer to allow DunGen generation in editor while using LethalLevelLoader to correctly work
* Fixed issue where specific special items (Shotgun, Shells, Hive, Knife) were not being collected
* Fixed issue where LethalLevelLoader was destroying assets in mods with multiple levels before it could correctly restore all those references first
* Probably a lot more!

</details>

</details>


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