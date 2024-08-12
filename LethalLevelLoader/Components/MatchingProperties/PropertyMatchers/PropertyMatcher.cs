using System.Collections.Generic;
using System.Linq;

namespace LethalLevelLoader.Components.MatchingProperties.PropertyMatchers;

/// <summary>
/// A base class for implementing property matchers.<br/>
/// <br/>
/// The default property matcher used by LLL is <see cref="HighestRarityPropertyMatcher"/>,<br/>
/// with <see cref="MultiplierPropertyMatcher"/> as an alternative implementation.
/// </summary>
/// <remarks>
/// Custom PropertyMatchers aren't supported.
/// </remarks>
internal abstract class PropertyMatcher
{
    /// <summary>
    /// Processes updating rarity value using the property matcher's rules.
    /// </summary>
    /// <remarks>
    /// Called every time a <c>GetRaritiesViaMatching*</c> method of this property matcher yields a value.
    /// </remarks>
    /// <param name="currentValue">The value to be modified.</param>
    /// <param name="newValue">The value yielded by a <c>GetRaritiesViaMatching*</c> method.</param>
    /// <param name="debugActionObject"></param>
    /// <param name="debugActionReason"></param>
    /// <returns><see langword="true"/> if rarity was modified, <see langword="false"/> otherwise.</returns>
    public abstract bool UpdateRarity(ref int currentValue, int newValue, string debugActionObject = null, string debugActionReason = null);

    /// <summary>
    /// Returns rarities for every match deemed valid by the property matcher.
    /// </summary>
    /// <param name="comparingValue">The value we compare against with our matching vectors.</param>
    /// <param name="matchingVectors">Rarity vectors from an instance of <see cref="LethalLevelLoader.MatchingProperties"/> or a class that inherits it.</param>
    /// <returns>Every matched rarity, to be processed by this property matcher's implementation of <see cref="UpdateRarity"/>.</returns>
    public abstract IEnumerable<int> GetRaritiesViaMatchingWithinRanges(int comparingValue, List<Vector2WithRarity> matchingVectors);

    /// <param name="comparingStrings">The strings we compare against with our matching strings.</param>
    /// <param name="matchingStrings">Matching strings from an instance of <see cref="LethalLevelLoader.MatchingProperties"/> or a class that inherits it.</param>
    /// <inheritdoc cref="GetRaritiesViaMatchingWithinRanges"/>
    public abstract IEnumerable<int> GetRaritiesViaMatchingNormalizedStrings(List<string> comparingStrings, List<StringWithRarity> matchingStrings);

    /// <param name="comparingString">The string we compare against with our matching strings.</param>
    /// <inheritdoc cref="GetRaritiesViaMatchingNormalizedStrings"/>
    public IEnumerable<int> GetRaritiesViaMatchingNormalizedString(string comparingString, List<StringWithRarity> matchingStrings)
        => GetRaritiesViaMatchingNormalizedStrings([comparingString], matchingStrings);

    /// <param name="comparingTags">The tags we compare against with our matching strings.</param>
    /// <inheritdoc cref="GetRaritiesViaMatchingNormalizedStrings"/>
    public IEnumerable<int> GetRaritiesViaMatchingNormalizedTags(List<ContentTag> comparingTags, List<StringWithRarity> matchingStrings)
    {
        List<string> contentTagStrings = comparingTags.Select(t => t.contentTagName).ToList();
        return GetRaritiesViaMatchingNormalizedStrings(contentTagStrings, matchingStrings);
    }
}
