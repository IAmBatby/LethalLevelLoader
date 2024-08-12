using System;
using System.Collections.Generic;
using LethalLevelLoader.Components.MatchingProperties.PropertyMatchers;
using UnityEngine;

namespace LethalLevelLoader
{
    public class MatchingProperties : ScriptableObject
    {
        [Space(5)] public int baseRarity = 0;
        [Space(5)] public List<StringWithRarity> modNames = new List<StringWithRarity>();
        [Space(5)] public List<StringWithRarity> authorNames = new List<StringWithRarity>();

        [NonSerialized] public PropertyMatcher activePropertyMatcher;

        public static MatchingProperties Create(ExtendedContent extendedContent)
        {
            MatchingProperties matchingProperties = ScriptableObject.CreateInstance<MatchingProperties>();
            matchingProperties.name = extendedContent.name + "MatchingProperties";
            return (matchingProperties);
        }
    }
}
