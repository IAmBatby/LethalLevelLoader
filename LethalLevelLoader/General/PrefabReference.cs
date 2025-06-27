using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    public abstract class PrefabReference
    {
        internal virtual GameObject Prefab { get; private set; }
        internal void Restore(GameObject restoredReference) => Prefab = restoredReference;
    }

    public abstract class PrefabReference<T> : PrefabReference
    {
        internal T PrefabHolder { get; private set; }
        public PrefabReference(T newPrefabHolder) { PrefabHolder = newPrefabHolder; }
    }

    public class SpawnSyncedObjectReference : PrefabReference<SpawnSyncedObject>
    {
        public SpawnSyncedObjectReference(SpawnSyncedObject newPrefabHolder) : base(newPrefabHolder) { }
        internal override GameObject Prefab => PrefabHolder.spawnPrefab;
    }

    public class SpawnableMapObjectReference : PrefabReference<SpawnableMapObject>
    {
        public SpawnableMapObjectReference(SpawnableMapObject newPrefabHolder) : base(newPrefabHolder) { }
        internal override GameObject Prefab => PrefabHolder.prefabToSpawn;
    }

    public class SpawnableOutsideMapObjectReference : PrefabReference<SpawnableOutsideMapObject>
    {
        public SpawnableOutsideMapObjectReference(SpawnableOutsideMapObject newPrefabHolder) : base(newPrefabHolder) { }
        internal override GameObject Prefab => PrefabHolder.prefabToSpawn;
    }

    public class SpawnableOutsideObjectReference : PrefabReference<SpawnableOutsideObject>
    {
        public SpawnableOutsideObjectReference(SpawnableOutsideObject newPrefabHolder) : base(newPrefabHolder) { }
        internal override GameObject Prefab => PrefabHolder.prefabToSpawn;
    }
}
