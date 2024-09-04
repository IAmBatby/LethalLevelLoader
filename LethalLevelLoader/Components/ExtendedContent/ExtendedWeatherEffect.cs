using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedWeatherEffect", menuName = "Lethal Level Loader/Extended Content/ExtendedWeatherEffect", order = 25)]
    public class ExtendedWeatherEffect : ExtendedContent
    {
        [field: Header("General Settings")]

        [field: SerializeField] public LevelWeatherType BaseWeatherType { get; set; } = LevelWeatherType.None;
        [field: SerializeField] public string WeatherDisplayName { get; set; } = string.Empty;

        [field: SerializeField] public GameObject WorldObject { get; set; }
        [field: SerializeField] public GameObject GlobalObject { get; set; }

        public ContentType contentType;

        //public bool lerpPosition;
        //public bool sunAnimatorBool;
        //public bool transitioning;
        //public bool effectEnabled;

        internal static ExtendedWeatherEffect Create(LevelWeatherType levelWeatherType, WeatherEffect weatherEffect, string weatherDisplayName)
        {
            return (ExtendedWeatherEffect.Create(levelWeatherType, weatherEffect.effectObject, weatherEffect.effectPermanentObject, weatherDisplayName));
        }

        internal static ExtendedWeatherEffect Create(LevelWeatherType levelWeatherType, GameObject worldObject, GameObject globalObject, string newWeatherDisplayName)
        {
            ExtendedWeatherEffect newExtendedWeatherEffect = ScriptableObject.CreateInstance<ExtendedWeatherEffect>();

            newExtendedWeatherEffect.WeatherDisplayName = newWeatherDisplayName;

            newExtendedWeatherEffect.name = newExtendedWeatherEffect.WeatherDisplayName + "ExtendedWeatherEffect";

            newExtendedWeatherEffect.BaseWeatherType = levelWeatherType;
            newExtendedWeatherEffect.WorldObject = worldObject;
            newExtendedWeatherEffect.GlobalObject = globalObject;

            return (newExtendedWeatherEffect);
        }

        internal override (bool result, string log) Validate()
        {
            return (true, string.Empty);
        }
    }
}
