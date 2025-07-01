using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader;

[CreateAssetMenu(fileName = "ExtendedFootstepSurface", menuName = "Lethal Level Loader/Extended Content/ExtendedFootstepSurface", order = 27)]
public class ExtendedFootstepSurface : ExtendedContent<ExtendedFootstepSurface,FootstepSurface,FootstepManager>
{
    public override FootstepSurface Content { get => footstepSurface; protected set => footstepSurface = value; }
    public FootstepSurface footstepSurface;
    public List<Material> associatedMaterials;

    internal override List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration() => NoPrefabReferences;
    internal override List<GameObject> GetNetworkPrefabsForRegistration()
    {
        return (new List<GameObject>());
    }
}
