using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public class FootstepManager : ExtendedContentManager<ExtendedFootstepSurface, FootstepSurface, FootstepManager>
    {
        protected override (bool result, string log) ValidateExtendedContent(ExtendedFootstepSurface extendedFootstepSurface)
        {
            if (extendedFootstepSurface.footstepSurface == null)
                return (false, "FootstepSurface Was Null");
            if (extendedFootstepSurface.associatedMaterials == null)
                return (false, "Associated Materials List Was Null");
            if (extendedFootstepSurface.associatedMaterials.Count == 0)
                return (false, "Associated Materials List Was Empty");
            if (extendedFootstepSurface.footstepSurface.clips == null)
                return (false, "FootstepSurface Clips Array Was Null");
            if (extendedFootstepSurface.footstepSurface.clips.Length == 0)
                return (false, "FootstepSurface Clips Array Was Empty");

            return (true, string.Empty);
        }
    }
}
