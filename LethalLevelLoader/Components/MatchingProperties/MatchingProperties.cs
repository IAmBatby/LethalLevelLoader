using System;
using System.Collections.Generic;
using LethalLevelLoader.Components.MatchingProperties.PropertyMatchers;
using UnityEngine;

namespace LethalLevelLoader
{
    public class MatchingProperties : ScriptableObject
    {
        [Space(5)] public int baseRarity = 0;
        [Space(5)] public PropertyMatcherType propertyMatcherType = PropertyMatcherType.HighestRarity;
        [Space(5)] public List<StringWithRarity> modNames = new List<StringWithRarity>();
        [Space(5)] public List<StringWithRarity> authorNames = new List<StringWithRarity>();

        private PropertyMatcher _propertyMatcher;

        public static MatchingProperties Create(ExtendedContent extendedContent)
        {
            MatchingProperties matchingProperties = ScriptableObject.CreateInstance<MatchingProperties>();
            matchingProperties.name = extendedContent.name + "MatchingProperties";
            return (matchingProperties);
        }

        internal PropertyMatcher GetPropertyMatcher()
        {
            if (_propertyMatcher is not null)
                return _propertyMatcher;

            switch (propertyMatcherType)
            {
                case PropertyMatcherType.HighestRarity:
                    _propertyMatcher = new HighestRarityPropertyMatcher();
                    break;

                case PropertyMatcherType.Multiplier:
                    _propertyMatcher = new MultiplierPropertyMatcher();
                    break;

                default:
                    DebugHelper.LogWarning(
                        $"Only PropertyMatchers of type {nameof(PropertyMatcherType.HighestRarity)} and {nameof(PropertyMatcherType.Multiplier)} are supported. " +
                        $"Continuing by using {nameof(HighestRarityPropertyMatcher)}.",
                        DebugType.User);
                    _propertyMatcher = new HighestRarityPropertyMatcher();
                    break;
            };

            return _propertyMatcher;
        }
    }
}
