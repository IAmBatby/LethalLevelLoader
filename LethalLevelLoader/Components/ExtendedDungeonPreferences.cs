using System;
using System.Collections.Generic;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLib/DungeonPreferences")]
    public class ExtendedDungeonPreferences : ScriptableObject
    {
        [Header("Dynamic Level List Settings")]
        public bool enableInjectionViaLevelCost;
        public int levelCostMin;
        public int levelCostMax;

        [Space(20)]

        public bool enableInjectionViaLevelDungeonMultiplierSetting;
        public int sizeMultiplierMin = 1;
        public int sizeMultiplierMax = 1;
        [Range(0, 1)]
        public float sizeMultiplierClampPercentage = 0;

        public List<Vector2WithRarity> dynamicRoutePricesList = new List<Vector2WithRarity>();

        [Space(20)]

        public bool enableInjectionViaLevelTags;
        public List<StringWithRarity> levelTagsList = new List<StringWithRarity>();

        [Space(15)]
        [Header("Manual Level List Settings")]
        public List<StringWithRarity> manualLevelSourceReferenceList = new List<StringWithRarity>();
        [Space(5)]
        public List<StringWithRarity> manualLevelNameReferenceList = new List<StringWithRarity>();

    }

    [System.Serializable]
    public class StringWithRarity
    {
        [Range(0, 1)] public string name;
        [HideInInspector] public int rarity;

        public StringWithRarity(string newName, float newSpawnChance)
        {
            name = newName;
            rarity = (int)newSpawnChance * 300;
        }


        public StringWithRarity(string newName, int newRarity)
        {
            name = newName;
            rarity = newRarity;
        }
    }

    public class Vector2WithRarity
    {
        public int min;
        public int max;
        public int rarity;

        public Vector2WithRarity(Vector2 vector2, int newRarity)
        {
            min = (int)vector2.x;
            max = (int)vector2.y;
            rarity = newRarity;
        }

        public Vector2WithRarity(int newMin, int newMax, int newRarity)
        {
            min = newMin;
            max = newMax;
            rarity = newRarity;
        }
    }
}
