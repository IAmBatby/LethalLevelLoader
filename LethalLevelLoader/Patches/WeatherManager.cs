using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace LethalLevelLoader
{
    internal class WeatherManager
    {
        public static ExtendedWeatherEffect CurrentExtendedWeatherEffect;

        public static Dictionary<LevelWeatherType, ExtendedWeatherEffect> vanillaExtendedWeatherEffectsDictionary = new Dictionary<LevelWeatherType, ExtendedWeatherEffect>();

        public static void PopulateVanillaExtendedWeatherEffectsDictionary()
        {
            foreach (ExtendedWeatherEffect extendedWeatherEffect in PatchedContent.VanillaExtendedWeatherEffects.OrderBy(w => (int)w.baseWeatherType))
                    vanillaExtendedWeatherEffectsDictionary.Add(extendedWeatherEffect.baseWeatherType, extendedWeatherEffect);
        }

        public static void PopulateExtendedLevelEnabledExtendedWeatherEffects()
        {
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                if (extendedLevel.selectableLevel.randomWeathers != null)
                    foreach (RandomWeatherWithVariables randomWeatherWithVariables in extendedLevel.selectableLevel.randomWeathers)
                        if (vanillaExtendedWeatherEffectsDictionary.TryGetValue(randomWeatherWithVariables.weatherType, out ExtendedWeatherEffect extendedWeatherEffect))
                            extendedLevel.enabledExtendedWeatherEffects.Add(extendedWeatherEffect);

                foreach (ExtendedWeatherEffect customExtendedWeatherEffect in PatchedContent.CustomExtendedWeatherEffects)
                    extendedLevel.enabledExtendedWeatherEffects.Add(customExtendedWeatherEffect);
            }
        }

        public static void SetExtendedLevelsWeather(int connectedPlayersOnServer)
        {
            StartOfRound startOfRound = Patches.StartOfRound;
            List<ExtendedLevel> extendedLevels = new List<ExtendedLevel>(PatchedContent.ExtendedLevels);

            foreach (ExtendedLevel extendedLevel in extendedLevels)
            {
                if (extendedLevel.selectableLevel.overrideWeather == false)
                {
                    extendedLevel.currentExtendedWeatherEffect = null;
                    extendedLevel.selectableLevel.currentWeather = LevelWeatherType.None;
                }
                else
                {
                    extendedLevel.selectableLevel.currentWeather = extendedLevel.selectableLevel.overrideWeatherType;
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
                if (extendedLevel.selectableLevel.randomWeathers != null && extendedLevel.selectableLevel.randomWeathers.Length != 0)
                    extendedLevel.selectableLevel.currentWeather = extendedLevel.selectableLevel.randomWeathers[random.Next(0, extendedLevel.selectableLevel.randomWeathers.Length)].weatherType;
                extendedLevels.Remove(extendedLevel);

            }
        }

        public static void SetExtendedLevelsExtendedWeatherEffect(int connectedPlayersOnServer)
        {
            StartOfRound startOfRound = Patches.StartOfRound;
            List<ExtendedLevel> extendedLevels = new List<ExtendedLevel>(PatchedContent.ExtendedLevels);

            foreach (ExtendedLevel extendedLevel in extendedLevels)
            {
                extendedLevel.currentExtendedWeatherEffect = null;
                if (extendedLevel.selectableLevel.overrideWeather != false)
                    if (vanillaExtendedWeatherEffectsDictionary.TryGetValue(extendedLevel.selectableLevel.overrideWeatherType, out ExtendedWeatherEffect extendedWeatherEffect))
                        extendedLevel.currentExtendedWeatherEffect = extendedWeatherEffect;
            }

            Random random = new Random(startOfRound.randomMapSeed + 31);
            float daySurvivalStreakMultiplier = 1f;
            if (connectedPlayersOnServer + 1 > 1 && startOfRound.daysPlayersSurvivedInARow > 2 && startOfRound.daysPlayersSurvivedInARow % 3 == 0)
                daySurvivalStreakMultiplier = (float)random.Next(15, 25) / 10f;

            int randomWeatherEffectToggleAttempts = Mathf.Clamp((int)(Mathf.Clamp(startOfRound.planetsWeatherRandomCurve.Evaluate((float)random.NextDouble()) * daySurvivalStreakMultiplier, 0f, 1f) * (float)PatchedContent.ExtendedLevels.Count), 0, PatchedContent.ExtendedLevels.Count);

            for (int j = 0; j < randomWeatherEffectToggleAttempts; j++)
            {
                ExtendedLevel extendedLevel = extendedLevels[random.Next(0, extendedLevels.Count)];
                extendedLevel.currentExtendedWeatherEffect = extendedLevel.enabledExtendedWeatherEffects[random.Next(0, extendedLevel.enabledExtendedWeatherEffects.Count)];
                extendedLevels.Remove(extendedLevel);
            }

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                if (extendedLevel.currentExtendedWeatherEffect == null)
                    extendedLevel.selectableLevel.currentWeather = LevelWeatherType.None;
                else if (extendedLevel.currentExtendedWeatherEffect.contentType == ContentType.Vanilla)
                    extendedLevel.selectableLevel.currentWeather = extendedLevel.currentExtendedWeatherEffect.baseWeatherType;
            }
        }

        public static ExtendedWeatherEffect GetVanillaExtendedWeatherEffect(LevelWeatherType levelWeatherType)
        {
            foreach (ExtendedWeatherEffect extendedWeatherEffect in PatchedContent.ExtendedWeatherEffects)
                if (extendedWeatherEffect.contentType == ContentType.Vanilla)
                    if (extendedWeatherEffect.baseWeatherType == levelWeatherType)
                        return (extendedWeatherEffect);

            return (null);
        }
    }
}
