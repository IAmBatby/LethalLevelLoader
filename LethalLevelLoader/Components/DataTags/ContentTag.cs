
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "Content Tag", menuName = "Lethal Level Loader/Utility/Data Tags/Content Tag", order = 11)]
    public class ContentTag : ColorTag
    {
        [HideInInspector] [Obsolete] public string contentTagName;
        [HideInInspector][Obsolete] public Color contentTagColor;

        private void Awake()
        {
            if (!string.IsNullOrEmpty(contentTagName))
                SetValues(contentTagName, contentTagColor);
        }
    }
}
