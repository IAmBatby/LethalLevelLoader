using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader;

[CreateAssetMenu(fileName = "ExtendedFootstepSurface", menuName = "Lethal Level Loader/Extended Content/ExtendedFootstepSurface", order = 27)]
public class ExtendedFootstepSurface : ExtendedContent
{
    public FootstepSurface footstepSurface;
    public List<Material> associatedMaterials;
    public List<GameObject> associatedGameObjects;
    internal int arrayIndex;
}
