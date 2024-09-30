using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    public class SceneBundleManifest : ScriptableObject
    {
        [SerializeField] private List<string> sceneNames = new List<string>();
    }
}
