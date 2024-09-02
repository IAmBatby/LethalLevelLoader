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

        // List Of New Content, List Of Original Comparisons
        public virtual void TryRestoreContents(List<T> originalContents, ref List<T> newContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Count; i++)
                newContents[i] = TryRestoreContent(newContents[i], originalContents, debugAction, destroyOnRestore);
        }

        // 1 New Content : List Of Orignal Comparisons
        public virtual T TryRestoreContent(T newContent, List<T> originalContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            if (newContent == null) return newContent;

            for (int i = 0; i < originalContents.Count; i++)
                if (originalContents[i] != null && CompareContent(originalContents[i], newContent))
                    return (RestoreContent(originalContents[i], newContent, debugAction, destroyOnRestore));

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

    public class UnityContentRestore<T> : ContentRestore<T> where T : Object
    {
        public override bool CompareContent(T originalContent, T newContent)
        {
            if (originalContent.name != null && newContent.name != null)
                return (originalContent.name == newContent.name);
            else
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
        public void TryRestoreContents(List<SpawnableItemWithRarity> newContents, List<Item> originalContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Count; i++)
                newContents[i].spawnableItem = TryRestoreContent(newContents[i].spawnableItem, originalContents, debugAction, destroyOnRestore);
        }
    }

    public class EnemyRestore : UnityContentRestore<EnemyType>
    {
        public void TryRestoreContents(List<SpawnableEnemyWithRarity> newContents, List<EnemyType> originalContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Count; i++)
                newContents[i].enemyType = TryRestoreContent(newContents[i].enemyType, originalContents, debugAction, destroyOnRestore);
        }
    }

    public class SpawnableMapObjectRestore : UnityContentRestore<GameObject>
    {
        public void TryRestoreContents(SpawnableMapObject[] newContents, List<GameObject> originalContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Length; i++)
                newContents[i].prefabToSpawn = TryRestoreContent(newContents[i].prefabToSpawn, originalContents, debugAction, destroyOnRestore);
        }

        public void TryRestoreContents(List<RandomMapObject> newContents, List<GameObject> originalContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Count; i++)
                for (int j = 0; j < newContents[i].spawnablePrefabs.Count; j++)
                    newContents[i].spawnablePrefabs[j] = TryRestoreContent(newContents[i].spawnablePrefabs[j], originalContents, debugAction, destroyOnRestore);
        }

        public void TryRestoreContents(RandomMapObject newContent, List<GameObject> originalContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int j = 0; j < newContent.spawnablePrefabs.Count; j++)
                newContent.spawnablePrefabs[j] = TryRestoreContent(newContent.spawnablePrefabs[j], originalContents, debugAction, destroyOnRestore);
        }
    }

    public class SpawnableOutsideObjectRestore : UnityContentRestore<GameObject>
    {
        public void TryRestoreContents(SpawnableOutsideObjectWithRarity[] newContents, List<SpawnableOutsideObject> originalContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            List<GameObject> prefabs = originalContents.Select(p => p.prefabToSpawn).ToList();
            for (int i = 0; i < newContents.Length; i++)
                newContents[i].spawnableObject.prefabToSpawn = TryRestoreContent(newContents[i].spawnableObject.prefabToSpawn, prefabs, debugAction, destroyOnRestore);
        }
    }

    public class ItemGroupRestore : UnityContentRestore<ItemGroup>
    {
        public void TryRestoreContents(List<RandomScrapSpawn> newContents, List<ItemGroup> originalContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Count; i++)
                newContents[i].spawnableItems = TryRestoreContent(newContents[i].spawnableItems, originalContents, debugAction, destroyOnRestore);
        }
    }

    public class ReverbPresetRestore : UnityContentRestore<ReverbPreset>
    {
        public void TryRestoreContents(AudioReverbTrigger[] newContents, List<ReverbPreset> originalContents, bool debugAction = false, bool destroyOnRestore = true )
        {
            for (int i = 0; i < newContents.Length; i++)
                newContents[i].reverbPreset = TryRestoreContent(newContents[i].reverbPreset, originalContents, debugAction, destroyOnRestore);
        }
    }

    public class AudioMixerGroupRestore : UnityContentRestore<AudioMixerGroup>
    {
        public void TryRestoreContents(AudioSource[] newContents, List<AudioMixerGroup> originalContents, bool debugAction = false, bool destroyOnRestore = true)
        {
            for (int i = 0; i < newContents.Length; i++)
                newContents[i].outputAudioMixerGroup = TryRestoreContent(newContents[i].outputAudioMixerGroup, originalContents, debugAction, destroyOnRestore);
        }
    }
}
