using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using LethalLevelLoader.Tools;
using UnityEngine.Rendering.HighDefinition;

namespace LethalLevelLoader
{
    public static class LevelLoader
    {
        internal static List<MeshCollider> customLevelMeshCollidersList = new List<MeshCollider>();

        internal static AnimatorOverrideController shipAnimatorOverrideController;
        internal static AnimationClip defaultShipFlyToMoonClip;
        internal static AnimationClip defaultShipFlyFromMoonClip;

        internal static Vector3 defaultDustCloudFogVolumeSize;
        internal static Vector3 defaultFoggyFogVolumeSize;

        internal static LocalVolumetricFog dustCloudFog;
        internal static LocalVolumetricFog foggyFog;


        internal static GameObject defaultQuicksandPrefab;

        internal static FootstepSurface[] defaultFootstepSurfaces;

        internal static Dictionary<Collider, List<Material>> cachedLevelColliderMaterialDictionary = new Dictionary<Collider, List<Material>>();
        internal static Dictionary<string, List<Collider>> cachedLevelMaterialColliderDictionary = new Dictionary<string, List<Collider>>();
        internal static Dictionary<string, FootstepSurface> activeExtendedFootstepSurfaceDictionary = new Dictionary<string, FootstepSurface>();
        internal static LayerMask triggerMask;

        internal static Shader vanillaWaterShader;

        internal static async void EnableMeshColliders()
        {
            List<MeshCollider> instansiatedCustomLevelMeshColliders = new List<MeshCollider>();

            int counter = 0;
            foreach (MeshCollider meshCollider in UnityEngine.Object.FindObjectsOfType<MeshCollider>())
                if (meshCollider.gameObject.name.Contains(" (LLL Tracked)"))
                    instansiatedCustomLevelMeshColliders.Add(meshCollider);

            Task[] meshColliderEnableTasks = new Task[instansiatedCustomLevelMeshColliders.Count];

            foreach (MeshCollider meshCollider in instansiatedCustomLevelMeshColliders)
            {
                meshColliderEnableTasks[counter] = EnableMeshCollider(meshCollider);
                counter++;
            }

            await Task.WhenAll(meshColliderEnableTasks);

            //customLevelMeshCollidersList.Clear();
        }

        internal static async Task EnableMeshCollider(MeshCollider meshCollider)
        {
            meshCollider.enabled = true;
            meshCollider.gameObject.name.Replace(" (LLL Tracked)", "");
            await Task.Yield();
        }

        internal static void RefreshShipAnimatorClips(ExtendedLevel extendedLevel)
        {
            DebugHelper.Log("Refreshing Ship Animator Clips!", DebugType.Developer);
            shipAnimatorOverrideController["HangarShipLandB"] = extendedLevel.ShipFlyToMoonClip;
            shipAnimatorOverrideController["ShipLeave"] = extendedLevel.ShipFlyFromMoonClip;
        }

        internal static void RefreshFogSize(ExtendedLevel extendedLevel)
        {
            if (dustCloudFog != null)
                dustCloudFog.parameters.size = extendedLevel.OverrideDustStormVolumeSize;
            if (foggyFog != null)
                foggyFog.parameters.size = extendedLevel.OverrideFoggyVolumeSize;
        }

        internal static void RefreshFootstepSurfaces()
        {
            List<FootstepSurface> activeFootstepSurfaces = new List<FootstepSurface>(defaultFootstepSurfaces);
            foreach (ExtendedFootstepSurface extendedSurface in LevelManager.CurrentExtendedLevel.ExtendedMod.ExtendedFootstepSurfaces)
            {
                extendedSurface.footstepSurface.surfaceTag = "Untagged";
                activeFootstepSurfaces.Add(extendedSurface.footstepSurface);
            }

            Patches.StartOfRound.footstepSurfaces = activeFootstepSurfaces.ToArray();
        }

        internal static void TryRestoreWaterShaders(Scene scene)
        {
            List<Material> uniqueMaterials = new List<Material>();
            foreach (MeshRenderer meshRenderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                if (meshRenderer.gameObject.scene == scene)
                    foreach (Material sharedMaterial in meshRenderer.sharedMaterials)
                    {
                        if (sharedMaterial != null && !string.IsNullOrEmpty(sharedMaterial.name))
                            if (!uniqueMaterials.Contains(sharedMaterial))
                                uniqueMaterials.Add(sharedMaterial);
                    }

            foreach (Material sharedMaterial in uniqueMaterials)
                ContentRestorer.TryRestoreWaterShader(sharedMaterial);
        }

        internal static void BakeSceneColliderMaterialData(Scene scene)
        {
            cachedLevelColliderMaterialDictionary.Clear();
            cachedLevelMaterialColliderDictionary.Clear();
            activeExtendedFootstepSurfaceDictionary = GetActiveExtendedFoostepSurfaceDictionary();

            triggerMask = LayerMask.NameToLayer("Triggers");

            List<Collider> allValidSceneColliders = new List<Collider>();

            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                foreach (Collider collider in rootObject.GetComponents<Collider>())
                {
                    if (ValidateCollider(collider) && !allValidSceneColliders.Contains(collider))
                        allValidSceneColliders.Add(collider);
                }
                foreach (Collider collider in rootObject.GetComponentsInChildren<Collider>())
                {
                    if (ValidateCollider(collider) && !allValidSceneColliders.Contains(collider))
                        allValidSceneColliders.Add(collider);
                }
            }
            
            foreach (Collider sceneCollider in allValidSceneColliders)
            {
                if (sceneCollider.TryGetComponent(out MeshRenderer meshRenderer))
                {
                    List<Material> validMaterials = new List<Material>();
                    foreach (Material material in meshRenderer.sharedMaterials)
                        if (material != null && !string.IsNullOrEmpty(material.name))
                            validMaterials.Add(material);

                    if (!cachedLevelColliderMaterialDictionary.ContainsKey(sceneCollider))
                        cachedLevelColliderMaterialDictionary.Add(sceneCollider, new List<Material>(validMaterials));

                    foreach (Material material in validMaterials)
                    {
                        if (!cachedLevelMaterialColliderDictionary.ContainsKey(material.name))
                            cachedLevelMaterialColliderDictionary.Add(material.name, new List<Collider> { sceneCollider });
                        else if (!cachedLevelMaterialColliderDictionary[material.name].Contains(sceneCollider))
                            cachedLevelMaterialColliderDictionary[material.name].Add(sceneCollider);
                    }
                }
            }

            //DebugHelper.DebugCachedLevelColliderData();
        }

        internal static bool ValidateCollider(Collider collider)
        {
            if (collider == null) return (false);
            if (collider.gameObject.activeSelf == false) return (false);
            if (collider.isTrigger == true) return (false);
            if (collider.gameObject.layer == triggerMask) return (false);
            if (collider.gameObject.CompareTag("Untagged") == false) return (false);

            return (true);
        }

        internal static Dictionary<string, FootstepSurface> GetActiveExtendedFoostepSurfaceDictionary()
        {
            Dictionary<string, FootstepSurface> returnDict = new Dictionary<string, FootstepSurface>();

            foreach (ExtendedFootstepSurface extendedFootstepSurface in LevelManager.CurrentExtendedLevel.ExtendedMod.ExtendedFootstepSurfaces)
                foreach (Material material in extendedFootstepSurface.associatedMaterials)
                    if (material != null && !string.IsNullOrEmpty(material.name))
                        if (!returnDict.ContainsKey(material.name))
                            returnDict.Add(material.name, extendedFootstepSurface.footstepSurface);


            return (returnDict);
        }

        public static bool TryGetFootstepSurface(Collider collider, out FootstepSurface footstepSurface)
        {
            footstepSurface = null;

            if (collider == null)
                return (false);

            if (cachedLevelColliderMaterialDictionary.TryGetValue(collider, out List<Material> materials))
                if (materials != null)
                    foreach (Material material in materials)
                        if (material != null && !string.IsNullOrEmpty(material.name))
                            activeExtendedFootstepSurfaceDictionary.TryGetValue(material.name, out footstepSurface);

            return (footstepSurface != null);
        }
    }
}