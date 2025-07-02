using DunGen;
using DunGen.Graph;
using LethalFoundation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DunGen.Graph.DungeonFlow;
using Random = System.Random;

namespace LethalLevelLoader
{
    [System.Serializable]
    public class ExtendedDungeonFlowWithRarity
    {
        public ExtendedDungeonFlow extendedDungeonFlow;
        public DungeonFlow Flow => extendedDungeonFlow.DungeonFlow;
        public int rarity;

        public ExtendedDungeonFlowWithRarity(ExtendedDungeonFlow newExtendedDungeonFlow, int newRarity) { extendedDungeonFlow = newExtendedDungeonFlow; rarity = newRarity; }

        public bool UpdateRarity(int newRarity) { if (newRarity > rarity) { rarity = newRarity; return (true); } return (false); }
    }

    public static class DungeonLoader
    {
        internal static GameObject defaultKeyPrefab;
        
        internal static void SelectDungeon()
        {
            Refs.DungeonGenerator.DungeonFlow = null;
            if (Refs.IsServer)
                ExtendedNetworkManager.Instance.GetRandomExtendedDungeonFlowServerRpc();
        }

        internal static void PrepareDungeon()
        {
            Refs.DungeonGenerator.retryCount = 50; //I shouldn't really do this but I'm curious if it silently helps some custom interiors

            if (DungeonManager.CurrentExtendedDungeonFlow.OverrideTilePlacementBounds)
            {
                Refs.DungeonGenerator.RestrictDungeonToBounds = true;
                Refs.DungeonGenerator.TilePlacementBounds = new Bounds(Vector3.zero, DungeonManager.CurrentExtendedDungeonFlow.OverrideRestrictedTilePlacementBounds);
            }

            PatchFireEscapes();
            PatchDynamicGlobalProps();
        }

        public static float GetClampedDungeonSize()
        {
            if (Refs.CurrentDungeonFlow == null) return (0f);
            ExtendedDungeonFlow flow = DungeonManager.CurrentExtendedDungeonFlow;
            ExtendedLevel level = LevelManager.CurrentExtendedLevel;
            float mult = CalculateDungeonMultiplier(level, flow);
            if (flow.IsDynamicDungeonSizeRestrictionEnabled)
            {
                float val = mult > flow.DynamicDungeonSizeMinMax.y ? flow.DynamicDungeonSizeMinMax.y : flow.DynamicDungeonSizeMinMax.x;
                mult = Mathf.Lerp(mult, val, flow.DynamicDungeonSizeLerpRate);
                DebugHelper.Log("Current ExtendedLevel: " + level.NumberlessPlanetName + " ExtendedLevel DungeonSize Is: " + level.SelectableLevel.factorySizeMultiplier + " | Overriding DungeonSize To: " + mult, DebugType.User);
            }
            else
                DebugHelper.Log("CurrentLevel: " + level.NumberlessPlanetName + " DungeonSize Is: " + level.SelectableLevel.factorySizeMultiplier + " | Leaving DungeonSize As: " + mult, DebugType.User);

            return (mult);
        }

        public static float CalculateDungeonMultiplier(ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow)
        {
            foreach (IndoorMapType indoorMapType in Refs.DungeonFlowTypes)
                if (indoorMapType.dungeonFlow == extendedDungeonFlow.DungeonFlow)
                    return (extendedLevel.SelectableLevel.factorySizeMultiplier / indoorMapType.MapTileSize * Refs.MapSizeMultiplier);
            return 1f;
        }

        internal static void PatchFireEscapes()
        {
            if (Refs.CurrentDungeonFlow == null) return;
            string debugString = "Fire Exit Patch Report, Details Below;" + "\n" + "\n";

            List<EntranceTeleport> entrances = Refs.LevelRootObjects.SelectMany(r => r.GetComponentsInChildren<EntranceTeleport>()).OrderBy(o => o.entranceId).ToList();
            int amount = entrances.Count;

            foreach (EntranceTeleport entranceTeleport in entrances)
                entranceTeleport.entranceId = entrances.IndexOf(entranceTeleport);

            debugString += "EntranceTeleport's Found, " + LevelManager.CurrentExtendedLevel.NumberlessPlanetName + " Contains " + (amount) + " Entrances! ( " + (amount - 1) + " Fire Escapes) " + "\n";
            debugString += "Main Entrance: " + entrances[0].gameObject.name + " (Entrance ID: " + entrances[0].entranceId + ")" + "\n";
            foreach (EntranceTeleport entranceTeleport in entrances.Where(e => e.entranceId != 0))
                debugString += "Alternate Entrance: " + entranceTeleport.gameObject.name + " (Entrance ID: " + entranceTeleport.entranceId + ")" + "\n";

            foreach (GlobalPropSettings propSettings in Refs.CurrentDungeonFlow.GlobalProps.Where(p => p.ID == 1231))
            {
                debugString += "Found Fire Escape GlobalProp: (ID: 1231), Modifying Spawn rate Count From (" + propSettings.Count.Min + "," + propSettings.Count.Max + ") To (" + (amount - 1) + "," + (amount - 1) + ")" + "\n";
                propSettings.Count = new IntRange(amount - 1, amount - 1); //-1 Because .Count includes the Main Entrance.
            }

            DebugHelper.Log(debugString + "\n", DebugType.User);
        }

        public static void PatchDynamicGlobalProps()
        {
            foreach (GlobalPropCountOverride propOverride in Refs.CurrentDungeonFlow.AsExtended().GlobalPropCountOverridesList)
                foreach (GlobalPropSettings globalProp in Refs.CurrentDungeonFlow.GlobalProps)
                    if (propOverride.globalPropID == globalProp.ID)
                    {
                        globalProp.Count.Min = globalProp.Count.Min * Mathf.RoundToInt(Mathf.Lerp(1, (Refs.DungeonGenerator.LengthMultiplier / Refs.MapSizeMultiplier), propOverride.globalPropCountScaleRate));
                        globalProp.Count.Max = globalProp.Count.Max * Mathf.RoundToInt(Mathf.Lerp(1, (Refs.DungeonGenerator.LengthMultiplier / Refs.MapSizeMultiplier), propOverride.globalPropCountScaleRate));
                    }
        }
    }
}
