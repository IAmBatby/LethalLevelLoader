using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader;

[CreateAssetMenu(menuName = "LethalLevelLoader/ExtendedFootstepSurface")]
public class ExtendedFootstepSurface : ScriptableObject
{
    public FootstepSurface footstepSurface;
    public List<Material> associatedMaterials;
    public List<GameObject> associatedGameObjects;
    internal int arrayIndex;
}
