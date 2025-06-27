using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace LethalLevelLoader
{
    public static class Extensions
    {
        public static List<Tile> GetTiles(this DungeonFlow dungeonFlow)
        {
            List<Tile> tilesList = new List<Tile>();

            foreach (GraphNode dungeonNode in dungeonFlow.Nodes)
                foreach (TileSet dungeonTileSet in dungeonNode.TileSets)
                    if (dungeonTileSet != null)
                        tilesList.AddRange(GetTilesInTileSet(dungeonTileSet));

            foreach (TileInjectionRule tileInjectionRule in dungeonFlow.TileInjectionRules)
                tilesList.AddRange(GetTilesInTileSet(tileInjectionRule.TileSet));

            foreach (GraphLine dungeonLine in dungeonFlow.Lines)
                foreach (DungeonArchetype dungeonArchetype in dungeonLine.DungeonArchetypes)
                {
                    foreach (TileSet dungeonTileSet in dungeonArchetype.BranchCapTileSets)
                        tilesList.AddRange(GetTilesInTileSet(dungeonTileSet));

                    foreach (TileSet dungeonTileSet in dungeonArchetype.TileSets)
                        tilesList.AddRange(GetTilesInTileSet(dungeonTileSet));
                }

            foreach (Tile tile in new List<Tile>(tilesList))
                if (tile == null)
                    tilesList.Remove(tile);

            return (tilesList);
        }

        public static List<Tile> GetTilesInTileSet(TileSet tileSet)
        {
            List<Tile> tilesList = new List<Tile>();
            if (tileSet.TileWeights != null && tileSet.TileWeights.Weights != null)
                foreach (GameObjectChance dungeonTileWeight in tileSet.TileWeights.Weights)
                    foreach (Tile dungeonTile in dungeonTileWeight.Value.GetComponentsInChildren<Tile>())
                        tilesList.Add(dungeonTile);
            return (tilesList);
        }

        public static List<RandomMapObject> GetRandomMapObjects(this DungeonFlow dungeonFlow)
        {
            List<RandomMapObject> returnList = new List<RandomMapObject>();

            foreach (Tile dungeonTile in dungeonFlow.GetTiles())
                foreach (RandomMapObject randomMapObject in dungeonTile.gameObject.GetComponentsInChildren<RandomMapObject>())
                    returnList.Add(randomMapObject);

            return (returnList);
        }

        public static List<SpawnSyncedObject> GetSpawnSyncedObjects(this DungeonFlow dungeonFlow)
        {
            List<SpawnSyncedObject> returnList = new List<SpawnSyncedObject>();

            foreach (Tile dungeonTile in dungeonFlow.GetTiles())
            {
                foreach (Doorway dungeonDoorway in dungeonTile.gameObject.GetComponentsInChildren<Doorway>())
                {
                    foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.ConnectorPrefabWeights)
                        foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                            if (!returnList.Contains(spawnSyncedObject))
                                returnList.Add(spawnSyncedObject);

                    foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.BlockerPrefabWeights)
                        foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                            if (!returnList.Contains(spawnSyncedObject))
                                returnList.Add(spawnSyncedObject);
                }

                foreach (SpawnSyncedObject spawnSyncedObject in dungeonTile.gameObject.GetComponentsInChildren<SpawnSyncedObject>())
                    if (!returnList.Contains(spawnSyncedObject))
                        returnList.Add(spawnSyncedObject);
            }
            return (returnList);
        }

        public static void AddReferences(this CompatibleNoun compatibleNoun, TerminalKeyword firstNoun, TerminalNode firstResult)
        {
            compatibleNoun.noun = firstNoun;
            compatibleNoun.result = firstResult;
        }

        public static void AddCompatibleNoun(this TerminalKeyword terminalKeyword, TerminalKeyword newNoun,  TerminalNode newResult)
        {
            if (terminalKeyword.compatibleNouns == null)
                terminalKeyword.compatibleNouns = new CompatibleNoun[0];
            CompatibleNoun newCompataibleNoun = new CompatibleNoun();
            newCompataibleNoun.noun = newNoun;
            newCompataibleNoun.result = newResult;
            terminalKeyword.compatibleNouns = terminalKeyword.compatibleNouns.AddItem(newCompataibleNoun).ToArray();
        }

        public static void AddCompatibleNoun(this TerminalNode terminalNode, TerminalKeyword newNoun, TerminalNode newResult)
        {
            if (terminalNode.terminalOptions == null)
                terminalNode.terminalOptions = new CompatibleNoun[0];
            CompatibleNoun newCompataibleNoun = new CompatibleNoun();
            newCompataibleNoun.noun = newNoun;
            newCompataibleNoun.result = newResult;
            terminalNode.terminalOptions = terminalNode.terminalOptions.AddItem(newCompataibleNoun).ToArray();
        }

        public static void TryAdd(this TerminalNode self, TerminalKeyword noun, TerminalNode node)
        {
            if (!self.terminalOptions.Contains(noun, node))
                self.AddCompatibleNoun(noun, node);
        }

        public static void TryAdd(this TerminalKeyword self, TerminalKeyword noun, TerminalNode node)
        {
            if (!self.compatibleNouns.Contains(noun, node))
                self.AddCompatibleNoun(noun, node);
        }


        public static bool Contains(this TerminalNode self, TerminalKeyword keyword, TerminalNode node)
        {
            return (self.terminalOptions.Contains(keyword, node));
        }

        public static bool Contains(this TerminalKeyword self, TerminalKeyword keyword, TerminalNode node)
        {
            return (self.compatibleNouns.Contains(keyword,node));
        }

        public static bool Contains(this CompatibleNoun[] self, TerminalKeyword keyword, TerminalNode node)
        {
            for (int i = 0; i < self.Length; i++)
                if (self[i].noun == keyword && self[i].result == node)
                    return (true);
            return (false);
        }

        public static bool Contains(this CompatibleNoun[] self, TerminalKeyword keyword)
        {
            for (int i = 0; i < self.Length; i++)
                if (self[i].noun == keyword)
                    return (true);
            return (false);
        }

        public static bool Contains(this CompatibleNoun[] self, TerminalNode node)
        {
            for (int i = 0; i < self.Length; i++)
                if (self[i].result == node)
                    return (true);
            return (false);
        }

        public static bool TryGet(this CompatibleNoun[] self, TerminalKeyword noun, out CompatibleNoun value)
        {
            value = null;
            for (int i = 0; i < self.Length; i++)
                if (self[i].noun == noun)
                {
                    value = self[i];
                    break;
                }
            return (value != null);
        }

        public static bool TryGet(this CompatibleNoun[] self, TerminalNode result, out CompatibleNoun value)
        {
            value = null;
            for (int i = 0; i < self.Length; i++)
                if (self[i].result == result)
                {
                    value = self[i];
                    break;
                }
            return (value != null);
        }

        public static bool TryGet(this CompatibleNoun[] self, TerminalNode result, out TerminalKeyword noun)
        {
            noun = null;
            if (self.TryGet(result, out CompatibleNoun pair))
                noun = pair.noun;
            return (noun != null);
        }

        public static bool TryGet(this CompatibleNoun[] self, TerminalKeyword noun, out TerminalNode result)
        {
            result = null;
            if (self.TryGet(noun, out CompatibleNoun pair))
                result = pair.result;
            return (result != null);
        }

        public static void Add(this IntWithRarity intWithRarity, int id, int rarity)
        {
            intWithRarity.id = id;
            intWithRarity.rarity = rarity;
        }

        public static IntWithRarity Create(this IntWithRarity intWithRarity, int id, int rarity)
        {
            IntWithRarity returnR = new IntWithRarity();
            returnR.id = id;
            returnR.rarity = rarity;
            return (returnR);
        }

        public static void AddOrAddAdd<K,V>(this Dictionary<K,List<V>> dict, K key, V value)
        {
            if (key == null || value == null) return;
            if (dict.TryGetValue(key, out List<V> list) == false)
                dict.Add(key, new List<V> { value });
            else if (!list.Contains(value))
                list.Add(value);
        }

        public static string Sanitized(this string currentString) => new string(currentString.SkipToLetters().RemoveWhitespace().ToLowerInvariant());
        public static string RemoveWhitespace(this string input) => new string(input.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
        public static string SkipToLetters(this string input) => new string(input.SkipWhile(c => !char.IsLetter(c)).ToArray());
        public static string StripSpecialCharacters(this string input)
        {
            string returnString = string.Empty;

            foreach (char charmander in input)
                if ((!ConfigHelper.illegalCharacters.ToCharArray().Contains(charmander) && char.IsLetterOrDigit(charmander)) || charmander.ToString() == " ")
                    returnString += charmander;

            return returnString;
        }

        public static List<DungeonFlow> GetDungeonFlows(this RoundManager roundManager)
        {

            return roundManager.dungeonFlowTypes.Select(i => i.dungeonFlow).ToList();

            
        }

        public static T[] Add<T>(this T[] array, T newValue)
        {
            return (array.AddItem(newValue)).ToArray();
        }
    }
}