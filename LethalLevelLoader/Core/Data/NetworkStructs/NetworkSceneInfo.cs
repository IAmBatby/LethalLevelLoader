using Unity.Netcode;
using UnityEngine.SceneManagement;
using static LethalLevelLoader.ExtendedNetworkManager;

namespace LethalLevelLoader
{
    public struct NetworkSceneInfo : INetworkSerializable
    {
        private uint m_levelSceneIndex;
        private NetworkString m_levelScenePath;


        public string LevelScenePath => m_levelScenePath.StringValue;
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
            m_levelScenePath = new NetworkString();
            m_levelScenePath.StringValue = levelScenePath;
            m_levelSceneIndex = (uint)levelSceneIndex;
        }


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_levelSceneIndex);
            serializer.SerializeNetworkSerializable(ref m_levelScenePath);
        }
    }
}
