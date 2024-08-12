using System.Collections.Generic;
using System.Linq;

namespace LethalLevelLoader.Components.MatchingProperties.PropertyMatchers;

public abstract class PropertyMatcher
{
    public abstract bool UpdateRarity(ref int currentValue, int newValue, string debugActionObject = null, string debugActionReason = null);

    public abstract IEnumerable<int> GetRaritiesViaMatchingWithinRanges(int comparingValue, List<Vector2WithRarity> matchingVectors);

    public abstract IEnumerable<int> GetRaritiesViaMatchingNormalizedStrings(List<string> comparingStrings, List<StringWithRarity> matchingStrings);

    public IEnumerable<int> GetRaritiesViaMatchingNormalizedString(string comparingString, List<StringWithRarity> matchingStrings)
        => GetRaritiesViaMatchingNormalizedStrings([comparingString], matchingStrings);

    public IEnumerable<int> GetRaritiesViaMatchingNormalizedTags(List<ContentTag> comparingTags, List<StringWithRarity> matchingStrings)
    {
        List<string> contentTagStrings = comparingTags.Select(t => t.contentTagName).ToList();
        return GetRaritiesViaMatchingNormalizedStrings(contentTagStrings, matchingStrings);
    }
}
