using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLevelLoader/ExtendedWeatherEffect")]
    public class ExtendedWeatherEffect : ScriptableObject
    {
        public LevelWeatherType baseWeatherType;

        public ContentType contentType;

        public string contentSourceName;

        public string weatherDisplayName;

        public GameObject worldObject;
        public GameObject globalObject;

        //public bool lerpPosition;
        //public bool sunAnimatorBool;
        //public bool transitioning;
        //public bool effectEnabled;

        internal static ExtendedWeatherEffect Create(LevelWeatherType levelWeatherType, WeatherEffect weatherEffect, string weatherDisplayName, string contentSourceName, ContentType newContentType)
        {
            return (ExtendedWeatherEffect.Create(levelWeatherType, weatherEffect.effectObject, weatherEffect.effectPermanentObject, weatherDisplayName, contentSourceName, newContentType));
        }

        internal static ExtendedWeatherEffect Create(LevelWeatherType levelWeatherType, GameObject worldObject, GameObject globalObject, string newWeatherDisplayName, string newContentSourceName, ContentType newContentType)
        {
            ExtendedWeatherEffect newExtendedWeatherEffect = ScriptableObject.CreateInstance<ExtendedWeatherEffect>();

            newExtendedWeatherEffect.weatherDisplayName = newWeatherDisplayName;
            newExtendedWeatherEffect.contentSourceName = newContentSourceName;

            newExtendedWeatherEffect.name = newExtendedWeatherEffect.weatherDisplayName + "ExtendedWeatherEffect";

            newExtendedWeatherEffect.baseWeatherType = levelWeatherType;
            newExtendedWeatherEffect.contentType = newContentType;
            newExtendedWeatherEffect.worldObject = worldObject;
            newExtendedWeatherEffect.globalObject = globalObject;

            return (newExtendedWeatherEffect);
        }

        internal void Initialize()
        {
            PatchedContent.ExtendedWeatherEffects.Add(this);

            DebugHelper.Log("Initializing ExtendedWeatherEffect: " + weatherDisplayName + "(" + contentType.ToString() + ")");
        }

    }
}
