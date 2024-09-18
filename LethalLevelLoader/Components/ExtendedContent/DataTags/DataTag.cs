using System;
using System.Collections.Generic;
using System.Text;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Serialization;

namespace LethalLevelLoader
{
    public class DataTag : ScriptableObject
    {
        [field: FormerlySerializedAs("contentTagName")]
        [field: SerializeField] public string TagName { get; protected set; }
    }

    public class DataTag<T> : DataTag
    {
        [field: FormerlySerializedAs("contentTagColor")]
        [field: SerializeField] public T TagValue { get; protected set; }

        public static C Create<C>(string newTagName, T newTagValue) where C : DataTag<T>
        {
            C dataTag = CreateInstance<C>();
            dataTag.TagName = newTagName;
            dataTag.TagValue = newTagValue;
            dataTag.name = newTagName + " (Data Tag: " + typeof(T).Name + ")";

            return (dataTag);

        }

        public bool TryGetValue(string tagName, out T value)
        {
            if (tagName == TagName && TagValue != null)
            {
                value = TagValue;
                return (true);
            }
            value = default;
            return (false);
        }

        internal void SetValues(string newTagName, T newValue)
        {
            TagName = newTagName;
            TagValue = newValue;
        }
    }

    [CreateAssetMenu(fileName = "Color Tag", menuName = "Lethal Level Loader/Utility/Data Tags/Color Tag", order = 11)]
    public class ColorTag : DataTag<Color> { }

    [CreateAssetMenu(fileName = "Float Tag", menuName = "Lethal Level Loader/Utility/Data Tags/Float Tag", order = 11)]
    public class FloatTag : DataTag<float> { }

    [CreateAssetMenu(fileName = "Vector2 Tag", menuName = "Lethal Level Loader/Utility/Data Tags/Vector2 Tag", order = 11)]
    public class Vector2Tag : DataTag<float> { }

    [CreateAssetMenu(fileName = "Vector3 Tag", menuName = "Lethal Level Loader/Utility/Data Tags/Vector3 Tag", order = 11)]
    public class Vector3Tag : DataTag<float> { }

    [CreateAssetMenu(fileName = "Positions Tag", menuName = "Lethal Level Loader/Utility/Data Tags/Positions Tag", order = 11)]
    public class Vector3CollectionTag : DataTag<List<Vector3>> { }

    [CreateAssetMenu(fileName = "String Tag", menuName = "Lethal Level Loader/Utility/Data Tags/String Tag", order = 11)]
    public class StringTag : DataTag<string> { }

    [CreateAssetMenu(fileName = "Bool Tag", menuName = "Lethal Level Loader/Utility/Data Tags/Bool Tag", order = 11)]
    public class BoolTag : DataTag<bool> { }
}