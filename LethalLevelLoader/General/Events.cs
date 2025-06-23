using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalLevelLoader
{
    public enum GameStates { Uninitialized, Startup, PreMainMenu, MainMenu, Lobby, Moon }
    public static class Events
    {
        public static GameStates CurrentState { get; private set; } = GameStates.Uninitialized;
        public static GameStates FurthestState { get; private set; } = GameStates.Uninitialized;

        public static ExtendedEvent<GameStates> OnCurrentStateChanged { get; private set; } = new ExtendedEvent<GameStates>();
        public static ExtendedEvent<GameStates> OnFurthestStateChanged { get; private set; } = new ExtendedEvent<GameStates>();

        internal static void ChangeGameState(GameStates newState)
        {
            if (newState == CurrentState) return;

            CurrentState = newState;
            if ((int)CurrentState > (int)FurthestState)
            {
                FurthestState = newState;
                OnFurthestStateChanged.Invoke(newState);
            }
            OnCurrentStateChanged.Invoke(newState);
        }

        internal static void TryUpdateGameState(string newSceneName)
        {
            GameStates newState = newSceneName switch
            {
                "InitSceneLaunchOptions" => GameStates.PreMainMenu,
                "MainMenu" => GameStates.MainMenu,
                "SampleSceneRelay" => GameStates.Lobby,
                _ when PatchedContent.AllLevelSceneNames.Contains(newSceneName) => GameStates.Moon,
                _ => GameStates.Uninitialized
            };
            if (newState != GameStates.Uninitialized)
                ChangeGameState(newState);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        internal static void Test()
        {
            DebugHelper.Log("InitAttributeWork!", DebugType.User);
        }
    }
}
