using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace LethalLevelLoader.Tools
{
    public abstract class ContentRestore
    {
        public abstract void Flush();
    }
    public abstract class ContentRestore<T> : ContentRestore
    {
        protected List<T> restoredContentDestroyList = new List<T>();

        protected abstract List<T> GetComparisonValues();

        // List Of New Content, List Of Original Comparisons
        public virtual void TryRestoreContents(ref List<T> newContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Count; i++)
                newContents[i] = TryRestoreContent(newContents[i], debugAction, destroyOnRestore);
        }

        // 1 New Content : List Of Orignal Comparisons
        public virtual T TryRestoreContent(T newContent, bool debugAction = false, bool destroyOnRestore = true)
        {
            if (newContent == null) return newContent;

            List<T> originalContents = GetComparisonValues();

            for (int i = 0; i < originalContents.Count; i++)
                return (TryRestoreContent(originalContents[i], newContent, debugAction, destroyOnRestore));

            return (newContent);
        }

        // 1 New Content : 1 Original Content Comparison
        public virtual T TryRestoreContent(T originalContent, T newContent, bool debugAction = false, bool destroyOnReplace = true)
        {
            if (originalContent == null || newContent == null) return originalContent;

            if (CompareContent(originalContent, newContent))
                return (RestoreContent(originalContent, newContent, debugAction, destroyOnReplace));
            else
                return (originalContent);
        }

        public abstract bool CompareContent(T originalContent, T newContent);

        protected virtual T RestoreContent(T originalContent, T newContent, bool debugAction = false, bool destroyOnReplace = true)
        {
            if (originalContent != null && newContent != null)
            {
                if (debugAction == true && originalContent.ToString() != null)
                    DebugHelper.Log("Restoring " + originalContent.GetType().ToString() + ": Old Asset Name: " + originalContent + " , New Asset Name: ", DebugType.Developer);

                if (destroyOnReplace == true)
                    if (!restoredContentDestroyList.Contains(originalContent))
                        restoredContentDestroyList.Add(originalContent);
            }
            else
                DebugHelper.LogWarning("Asset Restoration Failed, Null Reference Found!", DebugType.Developer);
            return (newContent);
        }
    }

    public abstract class UnityContentRestore<T> : ContentRestore<T> where T : Object
    {
        public override bool CompareContent(T originalContent, T newContent)
        {
            if (originalContent.name != null && newContent.name != null)
                return (originalContent.name == newContent.name);
            return (false);
        }

        public override void Flush()
        {
            for (int i = 0; i < restoredContentDestroyList.Count; i++)
                Object.Destroy(restoredContentDestroyList[i]);
            restoredContentDestroyList.Clear();
        }
    }

    public class ItemRestore : UnityContentRestore<Item>
    {
        protected override List<Item> GetComparisonValues() => OriginalContent.Items;

        public void TryRestoreContents(List<SpawnableItemWithRarity> newContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Count; i++)
                newContents[i].spawnableItem = TryRestoreContent(newContents[i].spawnableItem, debugAction, destroyOnRestore);
        }
    }

    public class EnemyRestore : UnityContentRestore<EnemyType>
    {
        protected override List<EnemyType> GetComparisonValues() => OriginalContent.Enemies;

        public void TryRestoreContents(List<SpawnableEnemyWithRarity> newContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Count; i++)
                newContents[i].enemyType = TryRestoreContent(newContents[i].enemyType, debugAction, destroyOnRestore);
        }
    }

    public class SpawnableMapObjectRestore : UnityContentRestore<GameObject>
    {
        protected override List<GameObject> GetComparisonValues() => OriginalContent.SpawnableMapObjects;

        public void TryRestoreContents(SpawnableMapObject[] newContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Length; i++)
                newContents[i].prefabToSpawn = TryRestoreContent(newContents[i].prefabToSpawn, debugAction, destroyOnRestore);
        }

        public void TryRestoreContents(List<RandomMapObject> newContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Count; i++)
                for (int j = 0; j < newContents[i].spawnablePrefabs.Count; j++)
                    newContents[i].spawnablePrefabs[j] = TryRestoreContent(newContents[i].spawnablePrefabs[j], debugAction, destroyOnRestore);
        }

        public void TryRestoreContents(RandomMapObject newContent, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int j = 0; j < newContent.spawnablePrefabs.Count; j++)
                newContent.spawnablePrefabs[j] = TryRestoreContent(newContent.spawnablePrefabs[j], debugAction, destroyOnRestore);
        }
    }

    public class SpawnableOutsideObjectRestore : UnityContentRestore<GameObject>
    {
        protected override List<GameObject> GetComparisonValues() => OriginalContent.SpawnableOutsideObjects;

        public void TryRestoreContents(SpawnableOutsideObjectWithRarity[] newContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Length; i++)
                newContents[i].spawnableObject.prefabToSpawn = TryRestoreContent(newContents[i].spawnableObject.prefabToSpawn, debugAction, destroyOnRestore);
        }
    }

    public class LevelAmbienceLibraryRestore : UnityContentRestore<LevelAmbienceLibrary>
    {
        protected override List<LevelAmbienceLibrary> GetComparisonValues() => OriginalContent.LevelAmbienceLibraries;
    }

    public class ItemGroupRestore : UnityContentRestore<ItemGroup>
    {
        protected override List<ItemGroup> GetComparisonValues() => OriginalContent.ItemGroups;

        public void TryRestoreContents(RandomScrapSpawn[] newContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Length; i++)
                newContents[i].spawnableItems = TryRestoreContent(newContents[i].spawnableItems, debugAction, destroyOnRestore);
        }
    }

    public class ReverbPresetRestore : UnityContentRestore<ReverbPreset>
    {
        protected override List<ReverbPreset> GetComparisonValues() => OriginalContent.ReverbPresets;

        public void TryRestoreContents(AudioReverbTrigger[] newContents, bool debugAction = false, bool destroyOnRestore = true )
        {
            for (int i = 0; i < newContents.Length; i++)
                newContents[i].reverbPreset = TryRestoreContent(newContents[i].reverbPreset, debugAction, destroyOnRestore);
        }
    }

    public class AudioMixerGroupRestore : UnityContentRestore<AudioMixerGroup>
    {
        protected override List<AudioMixerGroup> GetComparisonValues() => OriginalContent.AudioMixerGroups;

        public void TryRestoreContents(AudioSource[] newContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Length; i++)
                newContents[i].outputAudioMixerGroup = TryRestoreContent(newContents[i].outputAudioMixerGroup, debugAction, destroyOnRestore);
        }
    }
}
