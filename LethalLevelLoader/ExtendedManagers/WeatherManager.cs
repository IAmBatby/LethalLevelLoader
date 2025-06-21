using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace LethalLevelLoader
{
    public class WeatherManager : ExtendedContentManager<ExtendedWeatherEffect, WeatherEffect, WeatherManager>
    {
        public static ExtendedWeatherEffect CurrentExtendedWeatherEffect;

        public static Dictionary<LevelWeatherType, ExtendedWeatherEffect> vanillaExtendedWeatherEffectsDictionary = new Dictionary<LevelWeatherType, ExtendedWeatherEffect>();

        public static void PopulateVanillaExtendedWeatherEffectsDictionary()
        {
            foreach (ExtendedWeatherEffect extendedWeatherEffect in PatchedContent.VanillaExtendedWeatherEffects.OrderBy(w => (int)w.BaseWeatherType))
                    vanillaExtendedWeatherEffectsDictionary.Add(extendedWeatherEffect.BaseWeatherType, extendedWeatherEffect);
        }

        public static void PopulateExtendedLevelEnabledExtendedWeatherEffects()
        {
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                if (extendedLevel.SelectableLevel.randomWeathers != null)
                    foreach (RandomWeatherWithVariables randomWeatherWithVariables in extendedLevel.SelectableLevel.randomWeathers)
                        if (vanillaExtendedWeatherEffectsDictionary.TryGetValue(randomWeatherWithVariables.weatherType, out ExtendedWeatherEffect extendedWeatherEffect))
                            extendedLevel.EnabledExtendedWeatherEffects.Add(extendedWeatherEffect);

                foreach (ExtendedWeatherEffect customExtendedWeatherEffect in PatchedContent.CustomExtendedWeatherEffects)
                    extendedLevel.EnabledExtendedWeatherEffects.Add(customExtendedWeatherEffect);
            }
        }

        public static void SetExtendedLevelsWeather(int connectedPlayersOnServer)
        {
            StartOfRound startOfRound = Patches.StartOfRound;
            List<ExtendedLevel> extendedLevels = new List<ExtendedLevel>(PatchedContent.ExtendedLevels);

            foreach (ExtendedLevel extendedLevel in extendedLevels)
            {
                if (extendedLevel.SelectableLevel.overrideWeather == false)
                {
                    extendedLevel.CurrentExtendedWeatherEffect = null;
                    extendedLevel.SelectableLevel.currentWeather = LevelWeatherType.None;
                }
                else
                {
                    extendedLevel.SelectableLevel.currentWeather = extendedLevel.SelectableLevel.overrideWeatherType;
                }
            }

            Random random = new Random(startOfRound.randomMapSeed + 31);
            float daySurvivalStreakMultiplier = 1f;
            if (connectedPlayersOnServer + 1 > 1 && startOfRound.daysPlayersSurvivedInARow > 2 && startOfRound.daysPlayersSurvivedInARow % 3 == 0)
                daySurvivalStreakMultiplier = (float)random.Next(15, 25) / 10f;

            int randomWeatherEffectToggleAttempts = Mathf.Clamp((int)(Mathf.Clamp(startOfRound.planetsWeatherRandomCurve.Evaluate((float)random.NextDouble()) * daySurvivalStreakMultiplier, 0f, 1f) * (float)PatchedContent.ExtendedLevels.Count), 0, PatchedContent.ExtendedLevels.Count);

            for (int j = 0; j < randomWeatherEffectToggleAttempts; j++)
            {
                ExtendedLevel extendedLevel = extendedLevels[random.Next(0, extendedLevels.Count)];
                if (extendedLevel.SelectableLevel.randomWeathers != null && extendedLevel.SelectableLevel.randomWeathers.Length != 0)
                    extendedLevel.SelectableLevel.currentWeather = extendedLevel.SelectableLevel.randomWeathers[random.Next(0, extendedLevel.SelectableLevel.randomWeathers.Length)].weatherType;
                extendedLevels.Remove(extendedLevel);

            }
        }

        public static void SetExtendedLevelsExtendedWeatherEffect(int connectedPlayersOnServer)
        {
            StartOfRound startOfRound = Patches.StartOfRound;
            List<ExtendedLevel> extendedLevels = new List<ExtendedLevel>(PatchedContent.ExtendedLevels);

            foreach (ExtendedLevel extendedLevel in extendedLevels)
            {
                extendedLevel.CurrentExtendedWeatherEffect = null;
                if (extendedLevel.SelectableLevel.overrideWeather != false)
                    if (vanillaExtendedWeatherEffectsDictionary.TryGetValue(extendedLevel.SelectableLevel.overrideWeatherType, out ExtendedWeatherEffect extendedWeatherEffect))
                        extendedLevel.CurrentExtendedWeatherEffect = extendedWeatherEffect;
            }

            Random random = new Random(startOfRound.randomMapSeed + 31);
            float daySurvivalStreakMultiplier = 1f;
            if (connectedPlayersOnServer + 1 > 1 && startOfRound.daysPlayersSurvivedInARow > 2 && startOfRound.daysPlayersSurvivedInARow % 3 == 0)
                daySurvivalStreakMultiplier = (float)random.Next(15, 25) / 10f;

            int randomWeatherEffectToggleAttempts = Mathf.Clamp((int)(Mathf.Clamp(startOfRound.planetsWeatherRandomCurve.Evaluate((float)random.NextDouble()) * daySurvivalStreakMultiplier, 0f, 1f) * (float)PatchedContent.ExtendedLevels.Count), 0, PatchedContent.ExtendedLevels.Count);

            for (int j = 0; j < randomWeatherEffectToggleAttempts; j++)
            {
                ExtendedLevel extendedLevel = extendedLevels[random.Next(0, extendedLevels.Count)];
                extendedLevel.CurrentExtendedWeatherEffect = extendedLevel.EnabledExtendedWeatherEffects[random.Next(0, extendedLevel.EnabledExtendedWeatherEffects.Count)];
                extendedLevels.Remove(extendedLevel);
            }

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                if (extendedLevel.CurrentExtendedWeatherEffect == null)
                    extendedLevel.SelectableLevel.currentWeather = LevelWeatherType.None;
                else if (extendedLevel.CurrentExtendedWeatherEffect.contentType == ContentType.Vanilla)
                    extendedLevel.SelectableLevel.currentWeather = extendedLevel.CurrentExtendedWeatherEffect.BaseWeatherType;
            }
        }

        public static ExtendedWeatherEffect GetVanillaExtendedWeatherEffect(LevelWeatherType levelWeatherType)
        {
            foreach (ExtendedWeatherEffect extendedWeatherEffect in PatchedContent.ExtendedWeatherEffects)
                if (extendedWeatherEffect.contentType == ContentType.Vanilla)
                    if (extendedWeatherEffect.BaseWeatherType == levelWeatherType)
                        return (extendedWeatherEffect);

            return (null);
        }
    }
}
