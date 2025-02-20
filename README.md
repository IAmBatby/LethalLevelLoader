**LethalLevelLoader**
--

**A Custom API to support the manual and dynamic integration of all forms of custom content in Lethal Company.**

**Thunderstore Link:** *https://thunderstore.io/c/lethal-company/p/IAmBatby/LethalLevelLoader/*

**Discord Thread:** *https://discord.com/channels/1168655651455639582/1193461151636398080*

**Description**
--

### **1.4.11 for Lethal Company v69 Has Released!**

**LethalLevelLoader** is a custom API to support the manual and dynamic integration of custom levels and dungeons in Lethal Company.  
Mod Developers can provide LethalLevelLoader with their custom content via code or via automatic AssetBundle detection, and from there LethalLevelLoader will seamlessly load the content into the game.

This Mod is Likely To Be Incompatible with **LethalExpansion**, Due To The inherit conflicts involved in changing the same systems.

**How To Use (Users / Players)**
--


  Simply install LethalLevelLoader and it's dependencies.


  If a mod using **LethalLevelLoader** supplies a **.lethalbundle** file, **LethalLevelLoader** will automatically find and load it’s content as long as it’s in the /plugins/ folder (Subfolders will be detected)

**How To Use (Modders / Developers)**
--


  Please refer to the LethalLevelLoader Wiki for documentation on utilizing this API for your custom content:
  https://github.com/IAmBatby/LethalLevelLoader/wiki

**Features Currently Supported**
--
* Custom Moons
* Custom Interiors
* Custom Items (Scrap / Items)
* Custom Enemies (Enemies)
* Custom StoryLog's
* Custom Weather Effects (WIP)
* Custom Footstep Surfaces (WIP)

**Contributing To LethalLevelLoader**
--

### Setup
To start contributing to LLL, you can start by [forking](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/fork-a-repo) the repo.  
Then, follow these steps:  
1. Install [Unity Netcode Patcher](https://github.com/EvaisaDev/UnityNetcodePatcher) CLI tool: `dotnet tool install -g Evaisa.NetcodePatcher.Cli`  
2. It's recommended to set up a `csproj.user` file based on the [LethalLevelLoader.template.csproj.user](/LethalLevelLoader/LethalLevelLoader.template.csproj.user) file to automate copying the mod's DLL file over to your location of choosing. *This file will be gitignored.*

You should be now set up, and ready compile your fork of LethalLevelLoader on your machine!

**Credits**
--

* **Evaisa** *(This Mod is directly based from LethalLib's codebase and could have been made without it's pre-existing foundations.)*
* **SkullCrusher** *(This Mod is directly based from SkullCrusher's LethalLib' Fork and could not have been made without it's pre-existing foundations.)*
* **HolographicWings** *(This Mod was inspired by LethalExpansion and could not have been made without HolographicWing's support and research.)*
* **KayNetsua** *(This Mod was internally tested using KayNetsua's "E Gypt" Custom Level and KayNetsua assisted in testing LethalLevelLoader's usage)*
* **Badhamknibb** *(This Mod was internally tested using Badhamknibb's "SCP Foundation" Custom Dungeon and Badhamknibb's assisted in testing LethalLevelLoader's usage)*
* **Scoopy** *(This Mod was internally tested using Scoopy's "LethalExtension Castle" Custom Dungeon and Scoopy assisted in testing LethalLevelLoader's usage)*
* **Xilo** *(Xilo provided multiple instances of Bepinex & Unity.Netcode related support during the development of this Mod.)*
* **Lordfirespeed** *(Lordfirespeed provided multiple instances of Bepinex & Unity.Netcode related support during the development of this Mod.)*
* **onionymous** *(Onionymous provided a preview build of their Networked Scene Patcher API, allowing for dynamic, networked scene injection)*
* **Game-Icons.net** *(For the artwork used for the mod's logo)*
* **Maxwasunavailable** *(For creating LethalModDataLib and assisting me with it’s usage.)*
* **Mrov** *(For collaborating with me on initial Custom weather support related code, assisting in fixes related to the Config file and a bunch of miscellaneous assistance.)*
* **LadyRaphtalia**, **Xu Xiaolan**, **Badhamknibbs**, **Mrov**, **sfDesat**, **AboveFire**, **Autumnis The Everchanging**, **RosiePies**, **Drako** & **Audio Knight** *(For testing development on experimental 1.2.0 builds.)*
* **狐萝卜呀**, **tumbleweed**, **Corey**, **Ritskee**, **Altan**, **qxZap**, **Salamander**, **Chiseled Cactus**, **Phantom139**, **ImmaBawss**, **takeothewolf**, **zuzaratrust**, **Hackattack242**, **Mail Me Dabs**, **Kyros**, **SourceShard** & **Chupacabra** *(For playtesting and reporting bugs on experimental 1.2.0 builds.)*
* **Lunxara** *(For the heavy, rapid testing throughout all the experimental 1.2.0 builds.)*
* **Adi**, **Hamunii**, **Xu Xiaolan & **Wherget** *F(or various new contributions in the forms of direct suggestions and pull requests.)*
