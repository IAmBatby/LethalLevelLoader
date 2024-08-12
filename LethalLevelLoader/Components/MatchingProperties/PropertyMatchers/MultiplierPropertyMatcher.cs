using System.Collections.Generic;

namespace LethalLevelLoader.Components.MatchingProperties.PropertyMatchers;

public class MultiplierPropertyMatcher : PropertyMatcher
{
    public override bool UpdateRarity(ref int currentValue, int newValue, string debugActionObject = null, string debugActionReason = null)
    {
        int oldValue = currentValue;
        currentValue *= newValue;

        if (!string.IsNullOrEmpty(debugActionReason))
        {
            if (!string.IsNullOrEmpty(debugActionObject))
                DebugHelper.Log($"Multiplied Rarity Of: {debugActionObject} From ({oldValue}) To ({currentValue}) Due To Matching {debugActionReason}", DebugType.Developer);
            else
                DebugHelper.Log($"Multiplied Rarity From ({oldValue}) To ({currentValue}) Due To Matching {debugActionReason}", DebugType.Developer);
        }

        return true;
    }

    public override IEnumerable<int> GetRaritiesViaMatchingWithinRanges(int comparingValue, List<Vector2WithRarity> matchingVectors)
    {
        foreach (Vector2WithRarity vectorWithRarity in matchingVectors)
        {
            if (vectorWithRarity.Min <= comparingValue && comparingValue <= vectorWithRarity.Max)
                yield return vectorWithRarity.Rarity;
        }

        yield break;
    }

    public override IEnumerable<int> GetRaritiesViaMatchingNormalizedStrings(List<string> comparingStrings, List<StringWithRarity> matchingStrings)
    {
        foreach (StringWithRarity stringWithRarity in matchingStrings)
        {
            foreach (string comparingString in new List<string>(comparingStrings))
            {
                if (stringWithRarity.Name.Sanitized().Contains(comparingString.Sanitized()) ||
                    comparingString.Sanitized().Contains(stringWithRarity.Name.Sanitized()))
                {
                    yield return stringWithRarity.Rarity;
                }
            }
        }

        yield break;
    }
}
