using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;

namespace LethalLevelLoader
{
    internal static class DomainExpansion
    {
        //internal static string sceneName = "Level4March";

        internal static void CleaveNextScene(string sceneName)
        {
            SceneManager.sceneLoaded += OnSceneCleaved;
            SceneManager.LoadSceneAsync(sceneName);
        }

        internal static void OnSceneCleaved(Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneManager.UnloadSceneAsync(scene);
            SceneManager.sceneLoaded -= OnSceneCleaved;
        }

        internal static void ToggleCleavePatch(bool value)
        {
            if (value == true)
                LethalLevelLoaderPlugin.Harmony.PatchAll(typeof(Awake_Patch));
            //else
                //LethalLevelLoaderPlugin.Harmony.UnpatchAll(typeof(Awake_Patch));
        }
    }
}
