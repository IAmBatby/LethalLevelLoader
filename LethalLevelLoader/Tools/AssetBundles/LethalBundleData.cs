using System.Collections.Generic;

namespace LethalLevelLoader
{
    public enum BundleType { Default, Scene, SceneData }
    [System.Serializable]
    public class LethalBundle
    {
        public BundleInfo MainBundle { get; private set; }
        public BundleInfo SceneDataBundle { get; private set; }
        public BundleInfo SceneBundle { get; private set; }

        private List<ExtendedMod> extendedMods = new List<ExtendedMod>();

        public LethalBundle(params BundleInfo[] infos)
        {
            foreach (BundleInfo info in infos)
                AddBundleInfo(info);
        }

        internal void AddBundleInfo(BundleInfo info)
        {
            if (info.BundleType == BundleType.Default)
                MainBundle = info;
            else if (info.BundleType == BundleType.Scene)
                SceneBundle = info;
            else
                SceneDataBundle = info;
        }
    }

    [System.Serializable]
    public struct BundleInfo
    {
        public string Path { get; private set; }
        public string FileName { get; private set; }
        public BundleType BundleType { get; private set; }

        public BundleInfo(string newFilePath, string newBundleName, BundleType newBundleType)
        {
            Path = newFilePath;
            FileName = newBundleName;
            BundleType = newBundleType;
        }
    }
}
