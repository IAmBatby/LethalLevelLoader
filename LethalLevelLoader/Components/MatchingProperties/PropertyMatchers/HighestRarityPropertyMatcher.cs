using System.Collections.Generic;

namespace LethalLevelLoader.Components.MatchingProperties.PropertyMatchers;

internal class HighestRarityPropertyMatcher : PropertyMatcher
{
    public override bool UpdateRarity(ref int currentValue, int newValue, string debugActionObject = null, string debugActionReason = null)
    {
        if (newValue > currentValue)
        {
            if (!string.IsNullOrEmpty(debugActionReason))
            {
                if (!string.IsNullOrEmpty(debugActionObject))
                    DebugHelper.Log($"Raised Rarity Of: {debugActionObject} From ({currentValue}) To ({newValue}) Due To Matching {debugActionReason}", DebugType.Developer);
                else
                    DebugHelper.Log($"Raised Rarity From ({currentValue}) To ({newValue}) Due To Matching {debugActionReason}", DebugType.Developer);
            }
            currentValue = newValue;
            return true;
        }
        return false;
    }

    public override IEnumerable<int> GetRaritiesViaMatchingWithinRanges(int comparingValue, List<Vector2WithRarity> matchingVectors)
    {
        int returnInt = 0;
        foreach (Vector2WithRarity vectorWithRarity in matchingVectors)
            if (vectorWithRarity.Rarity >= returnInt)
                if (comparingValue >= vectorWithRarity.Min && comparingValue <= vectorWithRarity.Max)
                    returnInt = vectorWithRarity.Rarity;
        yield return returnInt;
    }

    public override IEnumerable<int> GetRaritiesViaMatchingNormalizedStrings(List<string> comparingStrings, List<StringWithRarity> matchingStrings)
    {
        int returnInt = 0;
        foreach (StringWithRarity stringWithRarity in matchingStrings)
            foreach (string comparingString in new List<string>(comparingStrings))
                if (stringWithRarity.Rarity >= returnInt)
                    if (stringWithRarity.Name.Sanitized().Contains(comparingString.Sanitized()) || comparingString.Sanitized().Contains(stringWithRarity.Name.Sanitized()))
                        returnInt = stringWithRarity.Rarity;
        yield return returnInt;
    }
}
