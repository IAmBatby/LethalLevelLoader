using Unity.Netcode;
using UnityEngine.SceneManagement;
using static LethalLevelLoader.LethalLevelLoaderNetworkManager;

namespace LethalLevelLoader
{
    public struct NetworkSceneInfo : INetworkSerializable
    {
        private uint m_levelSceneIndex;
        private StringContainer m_levelScenePath;


        public string LevelScenePath => m_levelScenePath.SomeText;
        public int LevelSceneIndex => (int)m_levelSceneIndex;
        public int SceneIndex
        {
            get
            {
                NetworkScenePatcher.TryGetSceneIndex((int)m_levelSceneIndex, LevelScenePath, out int returnIndex);
                return (returnIndex);
            }
        }

        public bool IsLoaded
        {
            get
            {
                if (Origin == SceneOrigin.Build)
                    return (true);
                if (AssetBundleLoader.TryGetAssetBundleInfo(LevelScenePath, out AssetBundleInfo info))
                    return (info.IsLoaded);
                else
                    return (false);
            }
        }

        public enum SceneOrigin { Build, Bundle }
        public SceneOrigin Origin
        {
            get
            {
                if (SceneIndex >= SceneManager.sceneCountInBuildSettings)
                    return (SceneOrigin.Bundle);
                else
                    return (SceneOrigin.Build);
            }
        }

        public NetworkSceneInfo(int levelSceneIndex, string levelScenePath)
        {
            m_levelScenePath = new StringContainer();
            m_levelScenePath.SomeText = levelScenePath;
            m_levelSceneIndex = (uint)levelSceneIndex;
        }


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_levelSceneIndex);
            serializer.SerializeNetworkSerializable(ref m_levelScenePath);
        }
    }
}
