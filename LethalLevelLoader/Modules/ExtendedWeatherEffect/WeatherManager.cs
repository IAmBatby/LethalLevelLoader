using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace LethalLevelLoader
{
    public class WeatherManager : ExtendedContentManager<ExtendedWeatherEffect, WeatherEffect>
    {
        public static ExtendedWeatherEffect CurrentExtendedWeatherEffect;

        public static Dictionary<LevelWeatherType, ExtendedWeatherEffect> vanillaExtendedWeatherEffectsDictionary = new Dictionary<LevelWeatherType, ExtendedWeatherEffect>();

        protected override void PatchGame()
        {
            DebugHelper.Log(GetType().Name + " Patching Game!", DebugType.User);
        }

        protected override void UnpatchGame()
        {
            DebugHelper.Log(GetType().Name + " Unpatching Game!", DebugType.User);
        }

        protected override (bool result, string log) ValidateExtendedContent(ExtendedWeatherEffect extendedWeatherEffect)
        {
            return (true, string.Empty);
        }

        protected override List<WeatherEffect> GetVanillaContent() => new List<WeatherEffect>();
        protected override ExtendedWeatherEffect ExtendVanillaContent(WeatherEffect content) => throw new NotImplementedException();

        protected override void PopulateContentTerminalData(ExtendedWeatherEffect content)
        {

        }
    }
}
