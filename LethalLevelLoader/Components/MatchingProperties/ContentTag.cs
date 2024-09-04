using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ContentTag", menuName = "Lethal Level Loader/Utility/ContentTag", order = 11)]
    public class ContentTag : ScriptableObject
    {
        public string contentTagName;
        public Color contentTagColor;


        public static ContentTag Create(string tag, Color color)
        {
            ContentTag contentTag = ScriptableObject.CreateInstance<ContentTag>();
            contentTag.contentTagName = tag;
            contentTag.contentTagColor = color;
            contentTag.name = tag + "ContentTag";

            return (contentTag);
        }

        public static ContentTag Create(string tag)
        {
            return (ContentTag.Create(tag, Color.white));
        }
    }
}
