using System.Collections.Generic;
using UnityEngine;

namespace LethalLevelLoader;

[CreateAssetMenu(fileName = "ExtendedFootstepSurface", menuName = "Lethal Level Loader/Extended Content/ExtendedFootstepSurface", order = 27)]
public class ExtendedFootstepSurface : ExtendedContent
{
    public FootstepSurface footstepSurface;
    public List<Material> associatedMaterials;

    internal override void Register(ExtendedMod extendedMod)
    {
        base.Register(extendedMod);

        extendedMod.ExtendedFootstepSurfaces.Add(this);
    }
}
