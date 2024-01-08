using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LethalLevelLoader
{
    class ConfigHelper
    {
        //Turns a single string into a list of StringWithRarity's, For easy config setup
        //Example: string configString = "FirstPlanetName (Rarity: Int), SecondPlanetName (Rarity: Int)"

        public static List<StringWithRarity> ConvertToStringWithRarityList(string inputString)
        {
            List<StringWithRarity> returnList = new List<StringWithRarity>();

            var tupleList = inputString.Split(',')
                .Select(s =>
                {
                    s = s.Trim();
                    int startIndex = s.IndexOf('(') + 1;
                    int endIndex = s.IndexOf(')');
                    string word = s.Substring(0, startIndex - 2);
                    int number = int.Parse(s.Substring(startIndex, endIndex - startIndex));
                    return (word, number);
                }).ToList();

            foreach (var tuple in tupleList)
                returnList.Add(new StringWithRarity(tuple.word, tuple.number));

            return (returnList);
        }
    }
}
