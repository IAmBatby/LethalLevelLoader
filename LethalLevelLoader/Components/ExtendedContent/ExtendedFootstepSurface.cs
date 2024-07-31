using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader;

[CreateAssetMenu(fileName = "ExtendedFootstepSurface", menuName = "Lethal Level Loader/Extended Content/ExtendedFootstepSurface", order = 27)]
public class ExtendedFootstepSurface : ExtendedContent
{
    public FootstepSurface footstepSurface = null!;
    public List<Material> associatedMaterials = null!;
    internal int arrayIndex;
}
