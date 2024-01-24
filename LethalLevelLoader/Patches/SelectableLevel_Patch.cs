using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace LethalLevelLoader
{
    public class SelectableLevel_Patch
    {
        public static List<ExtendedLevel> allLevelsList = new List<ExtendedLevel>();
        public static List<ExtendedLevel> vanillaLevelsList = new List<ExtendedLevel>();
        public static List<ExtendedLevel> customLevelsList = new List<ExtendedLevel>();

        public static List<DayHistory> dayHistoryList = new List<DayHistory>();
        public static int daysTotal;
        public static int quotasTotal;

        internal static List<SelectableLevel> prePatchedLevelsList = new List<SelectableLevel>();
        internal static List<SelectableLevel> patchedLevelsList = new List<SelectableLevel>();
        internal static List<SelectableLevel> prePatchedMoonsCatalogueList = new List<SelectableLevel>();
        internal static List<SelectableLevel> patchedMoonsCatalogueList = new List<SelectableLevel>();

        internal static string injectionSceneName = "InitSceneLaunchOptions";

        internal static Scene deadScene;

        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        internal static void StartOfRound_Start()
        {
            /*if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
            {
                CreatePatchedLevelsList();
                CreatePatchedMoonsCatalogueList();

                foreach (ExtendedLevel customLevel in customLevelsList)
                    AssetBundleLoader.RestoreVanillaLevelAssetReferences(customLevel);

                foreach (ExtendedDungeonFlow customDungeonFlow in DungeonFlow_Patch.customDungeonFlowsList)
                    AssetBundleLoader.RestoreVanillaDungeonAssetReferences(customDungeonFlow);
            }

            PatchVanillaLevelLists();*/
        }

        [HarmonyPriority(350)]
        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        internal static void RoundManagerStart_Prefix()
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
            {
                CreatePatchedLevelsList();
                CreatePatchedMoonsCatalogueList();

                foreach (ExtendedLevel customLevel in customLevelsList)
                    AssetBundleLoader.RestoreVanillaLevelAssetReferences(customLevel);

                foreach (ExtendedDungeonFlow customDungeonFlow in DungeonFlow_Patch.customDungeonFlowsList)
                    AssetBundleLoader.RestoreVanillaDungeonAssetReferences(customDungeonFlow);

                Terminal_Patch.CreateMoonsFilterTerminalAssets();
                Terminal_Patch.CreateVanillaExtendedLevelGroups();
                Terminal_Patch.CreateCustomExtendedLevelGroups();

                LethalLevelLoaderPlugin.hasVanillaBeenPatched = true;
            }

            PatchVanillaLevelLists();

            TestDeadSceneLoad();
        }

        [HarmonyPriority(350)]
        [HarmonyPatch(typeof(animatedSun), "Start")]
        [HarmonyPrefix]
        internal static void AnimatedSun_Prefix()
        {
            Debug.Log("ANIMATED SUN OOO SPOOKY");
        }


        [HarmonyPriority(350)]
        [HarmonyPatch(typeof(System.Object), "Object", MethodType.Constructor)]
        [HarmonyPostfix]
        internal static void Object_Prefix(System.Object __instance)
        {
            Debug.Log("OBJECT: " + __instance);
            objects.Add(__instance);
        }

        public static List<System.Object> objects = new List<System.Object>();


        [HarmonyPriority(350)]
        [HarmonyPatch(typeof(NavMeshSurface), "OnEnable")]
        [HarmonyPrefix]
        internal static void NavMeshSurface_Prefix()
        {
            Debug.Log("NAVMESHSURFACE OOO SPOOKY");
        }

        internal static void TestDeadSceneLoad()
        {
            deadScene = SceneManager.GetSceneByName("Level4March");
            NetworkManager networkManager = NetworkManager.Singleton;

            DebugHelper.Log("Scene Count Is: " + SceneManager.sceneCount);

            DebugHelper.Log("Attempting To Load Dead Scene: March");
            AsyncOperation asyncSceneLoad = SceneManager.LoadSceneAsync("Level4March", LoadSceneMode.Additive);
            SceneManager.sceneLoaded += InstantKillScene;
            DebugHelper.Log("Loaded Dead Scene: Level4March");
            //asyncSceneLoad.allowSceneActivation = false;
            DebugHelper.Log("Force Stopped Scene Activation");

            DebugHelper.Log("Scene Count Is: " + SceneManager.sceneCount);
        }

        public static void InstantKillScene(Scene scene, LoadSceneMode loadSceneMode)
        {
            foreach (System.Object objectValue in  objects)
            {

                if (objectValue is UnityEngine.Object)
                {
                    UnityEngine.Object unityObject = (UnityEngine.Object)objectValue;
                    Debug.Log("UNITYOBJECT: " + unityObject.name);
                }
            }
            foreach (GameObject rootObject in scene.GetRootGameObjects())
                foreach (Transform childObject in rootObject.GetComponentsInChildren<Transform>())
                    childObject.gameObject.SetActive(false);
            Debug.Log("Trying To Kill March!");
            SceneManager.UnloadSceneAsync("Level4March");
        }

        internal static void AddSelectableLevel(ExtendedLevel extendedLevel)
        {
            if (extendedLevel.levelType == ContentType.Custom)
                customLevelsList.Add(extendedLevel);
            else
                vanillaLevelsList.Add(extendedLevel);

            allLevelsList.Add(extendedLevel);
        }

        internal static void CreatePatchedLevelsList()
        {
            prePatchedLevelsList = new List<SelectableLevel>(StartOfRound.Instance.levels.ToList());
            patchedLevelsList = new List<SelectableLevel>(prePatchedLevelsList);

            foreach (ExtendedLevel extendedLevel in customLevelsList)
                patchedLevelsList.Add(extendedLevel.selectableLevel);
        }

        internal static void CreatePatchedMoonsCatalogueList()
        {
            prePatchedMoonsCatalogueList = new List<SelectableLevel>(Terminal_Patch.Terminal.moonsCatalogueList);
            patchedMoonsCatalogueList = new List<SelectableLevel>(prePatchedMoonsCatalogueList);

            foreach (ExtendedLevel extendedLevel in customLevelsList)
                patchedMoonsCatalogueList.Add(extendedLevel.selectableLevel);
        }

        internal static void PatchVanillaLevelLists()
        {
            Terminal terminal = GameObject.FindAnyObjectByType<Terminal>();
            StartOfRound startOfRound = StartOfRound.Instance;

            startOfRound.levels = patchedLevelsList.ToArray();
            terminal.moonsCatalogueList = patchedMoonsCatalogueList.ToArray();
        }

        public static bool TryGetExtendedLevel(SelectableLevel selectableLevel, out ExtendedLevel returnExtendedLevel, ContentType levelType = ContentType.Any)
        {
            returnExtendedLevel = null;
            List<ExtendedLevel> extendedLevelsList = new List<ExtendedLevel>();

            switch (levelType)
            {
                case ContentType.Vanilla:
                    extendedLevelsList = vanillaLevelsList;
                    break;
                case ContentType.Custom:
                    extendedLevelsList = customLevelsList;
                    break;
                case ContentType.Any:
                    extendedLevelsList = allLevelsList;
                    break;
            }

            foreach (ExtendedLevel extendedLevel in extendedLevelsList)
                if (extendedLevel.selectableLevel == selectableLevel)
                    returnExtendedLevel = extendedLevel;

            return (returnExtendedLevel != null);
        }

        public static ExtendedLevel GetExtendedLevel(SelectableLevel selectableLevel)
        {
            ExtendedLevel returnExtendedLevel = null;

            foreach (ExtendedLevel extendedLevel in allLevelsList)
                if (extendedLevel.selectableLevel == selectableLevel)
                    returnExtendedLevel = extendedLevel;

            return (returnExtendedLevel);
        }

        public static void LogDayHistory()
        {
            DayHistory newDayHistory = new DayHistory();
            daysTotal++;

            newDayHistory.extendedLevel = GetExtendedLevel(StartOfRound.Instance.currentLevel);
            DungeonFlow_Patch.TryGetExtendedDungeonFlow(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow);
            newDayHistory.extendedDungeonFlow = extendedDungeonFlow;
            newDayHistory.day = daysTotal;
            newDayHistory.quota = TimeOfDay.Instance.timesFulfilledQuota;
            newDayHistory.weatherEffect = StartOfRound.Instance.currentLevel.currentWeather;

            DebugHelper.Log("Created New Day History Log! PlanetName: " + newDayHistory.extendedLevel.NumberlessPlanetName + " , DungeonName: " + newDayHistory.extendedDungeonFlow.dungeonDisplayName + " , Quota: " + newDayHistory.quota + " , Day: " + newDayHistory.day + " , Weather: " + newDayHistory.weatherEffect.ToString());

            dayHistoryList.Add(newDayHistory);
        }
    }

    public class DayHistory
    {
        public int quota;
        public int day;
        public ExtendedLevel extendedLevel;
        public ExtendedDungeonFlow extendedDungeonFlow;
        public LevelWeatherType weatherEffect;
    }

    public class MyClassIdea1
    {
        public int mainNumber;
        public bool mainSetting;
        public string mainName;

        public UnityEvent myEvent1;
        public UnityEvent myEvent2;
        public UnityEvent myEvent3;
        public UnityEvent myEvent4;
        public UnityEvent myEvent5;
    }

    public class MyClassIdea2
    {
        public int mainNumber;
        public bool mainSetting;
        public string mainName;

        public MyClassEvents classEvents = new MyClassEvents();
    }

    public class MyClassEvents
    {
        public UnityEvent myEvent1;
        public UnityEvent myEvent2;
        public UnityEvent myEvent3;
        public UnityEvent myEvent4;
        public UnityEvent myEvent5;
    }
}