using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader;

[CreateAssetMenu(fileName = "ExtendedFootstepSurface", menuName = "Lethal Level Loader/Extended Content/ExtendedFootstepSurface", order = 27)]
public class ExtendedFootstepSurface : ExtendedContent<ExtendedFootstepSurface,FootstepSurface,FootstepManager>
{
    public override FootstepSurface Content => footstepSurface;
    public FootstepSurface footstepSurface;
    public List<Material> associatedMaterials;
}
