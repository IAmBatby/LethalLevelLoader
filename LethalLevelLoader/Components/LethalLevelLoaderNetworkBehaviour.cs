using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public class LethalLevelLoaderNetworkBehaviour : NetworkBehaviour
    {
        public static LethalLevelLoaderNetworkBehaviour Instance;

        public void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        [ServerRpc]
        public void SetDungeonFlowServerRpc(int extendedLevelID)
        {
            DebugHelper.Log("Setting DungeonFlow!");

            ExtendedLevel extendedLevel = SelectableLevel_Patch.allLevelsList[extendedLevelID];

            RoundManager roundManager = RoundManager.Instance;

            Random levelRandom = RoundManager.Instance.LevelRandom;

            int randomisedDungeonIndex = -1;

            List<int> randomWeightsList = new List<int>();
            string debugString = "Current Level + (" + extendedLevel.NumberlessPlanetName + ") Weights List: " + "\n" + "\n";

            List<ExtendedDungeonFlowWithRarity> availableExtendedFlowsList = DungeonFlow_Patch.GetValidExtendedDungeonFlows(extendedLevel, debugString).ToList();

            foreach (ExtendedDungeonFlowWithRarity extendedDungeon in availableExtendedFlowsList)
                randomWeightsList.Add(extendedDungeon.rarity);

            randomisedDungeonIndex = roundManager.GetRandomWeightedIndex(randomWeightsList.ToArray(), levelRandom);

            foreach (ExtendedDungeonFlowWithRarity extendedDungeon in availableExtendedFlowsList)
            {
                debugString += extendedDungeon.extendedDungeonFlow.dungeonFlow.name + " | " + extendedDungeon.rarity;
                if (extendedDungeon.extendedDungeonFlow == availableExtendedFlowsList[randomisedDungeonIndex].extendedDungeonFlow)
                    debugString += " - Selected DungeonFlow" + "\n";
                else
                    debugString += "\n";
            }

            DebugHelper.Log(debugString + "\n");

            int extendedDungeonFlowID = DungeonFlow_Patch.allExtendedDungeonsList.IndexOf(availableExtendedFlowsList[randomisedDungeonIndex].extendedDungeonFlow);

            DebugHelper.Log("Host Chose ExtendedDungeonFlow: " + DungeonFlow_Patch.allExtendedDungeonsList[extendedDungeonFlowID].name + "Dungeon Index ID Was: " + extendedDungeonFlowID);

            SetDungeonFlowClientRpc(DungeonFlow_Patch.allExtendedDungeonsList.IndexOf(availableExtendedFlowsList[randomisedDungeonIndex].extendedDungeonFlow));
        }

        [ClientRpc]
        public void SetDungeonFlowClientRpc(int extendedDungeonFlowID)
        {
            if (NetworkManager.Singleton.IsServer == true)
                DebugHelper.Log("Setting DungeonFlow On Host");
            else
                DebugHelper.Log("Setting DungeonFlow On Client");

            RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow = DungeonFlow_Patch.allExtendedDungeonsList[extendedDungeonFlowID].dungeonFlow;
            DungeonLoader.HasSetDungeonFlow(true);
            RoundManager.Instance.dungeonGenerator.Generator.Generate();
        }
    }
}
