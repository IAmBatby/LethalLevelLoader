using LethalLevelLoader;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static HookHelper;

public static class NetworkScenePatcher
{
    // start of script
    static List<string> scenePaths = new();
    
    static Dictionary<string, int> scenePathToBuildIndex = new();
    static Dictionary<int, string> buildIndexToScenePath = new();
    static Dictionary<uint, string> sceneHashToScenePath = new();

    public static void AddScenePath(string scenePath)
    {
        if (scenePaths.Contains(scenePath))
        {
            //Debug.LogError($"Can not add scene path {scenePath} to the network scene patcher! (already exists in scene paths list)");
            return;
        }
        scenePaths.Add(scenePath);
    }


    // where the patching starts >:3c
    static DisposableHookCollection hooks = new();
    
    internal static bool patched { get; private set; }
    internal static void Patch()
    {
        if (patched) return; patched = true;

        hooks.Hook<NetworkSceneManager>("GenerateScenesInBuild", GenerateScenesInBuild_Hook);
        hooks.Hook<NetworkSceneManager>("SceneNameFromHash", SceneNameFromHash_Hook);
        hooks.Hook<NetworkSceneManager>("ValidateSceneBeforeLoading", ValidateSceneBeforeLoading_Hook, new[] {typeof(int), typeof(string), typeof(LoadSceneMode)});

        hooks.ILHook<NetworkSceneManager>("SceneHashFromNameOrPath", ReplaceBuildIndexByScenePath);
        hooks.ILHook<NetworkSceneManager>("ValidateSceneEvent", ReplaceBuildIndexByScenePath);
        hooks.ILHook<NetworkSceneManager>("ScenePathFromHash", ReplaceScenePathByBuildIndex);
    }
    internal static void Unpatch()
    {
        if (!patched) return; patched = false;
        hooks.Clear();
    }

    static void ReplaceScenePathByBuildIndex(ILContext il)
    {
        ILCursor script = new(il);
        MethodInfo replacement = methodof(GetScenePathByBuildIndex);

        while (script.TryGotoNext(instr => instr.MatchCall(typeof(SceneUtility), "GetScenePathByBuildIndex")))
        {
            script.Remove();
            script.Emit(OpCodes.Call, replacement);
        }
    }
    static void ReplaceBuildIndexByScenePath(ILContext il)
    {
        ILCursor script = new(il);
        MethodInfo replacement = methodof(GetBuildIndexByScenePath);

        while (script.TryGotoNext(instr => instr.MatchCall(typeof(SceneUtility), "GetBuildIndexByScenePath")))
        {
            script.Remove();
            script.Emit(OpCodes.Call, replacement);
        }
    }

    static string GetScenePathByBuildIndex(int buildIndex)
    {
        if (buildIndexToScenePath.ContainsKey(buildIndex))
        {
            return buildIndexToScenePath[buildIndex];
        }
        return SceneUtility.GetScenePathByBuildIndex(buildIndex);
    }
    static int GetBuildIndexByScenePath(string scenePath)
    {
        int val = SceneUtility.GetBuildIndexByScenePath(scenePath);
        if (val == -1)
        {
            if (scenePathToBuildIndex.ContainsKey(scenePath)) val = scenePathToBuildIndex[scenePath];
        }
        return val;
    }
    static void GenerateScenesInBuild_Hook(Action<NetworkSceneManager> orig, NetworkSceneManager self)
    {
        scenePathToBuildIndex.Clear();
        buildIndexToScenePath.Clear();
        sceneHashToScenePath.Clear();

        orig(self);

        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < scenePaths.Count; i++)
        {
            int buildIndex = count + i;
            string scenePath = scenePaths[i];
            uint hash = scenePath.Hash32();

            self.HashToBuildIndex.Add(hash, buildIndex);
            self.BuildIndexToHash.Add(buildIndex, hash);

            scenePathToBuildIndex.Add(scenePath, buildIndex);
            buildIndexToScenePath.Add(buildIndex, scenePath);
            sceneHashToScenePath.Add(hash, scenePath);

            //DebugHelper.Log($"Added modded scene path: {scenePath}");
        }
    }
    static string SceneNameFromHash_Hook(Func<NetworkSceneManager, uint, string> orig, NetworkSceneManager self, uint sceneHash)
    {
        if (sceneHash == 0U) return "No Scene";
        if (sceneHashToScenePath.ContainsKey(sceneHash)) return sceneHashToScenePath[sceneHash];
        return orig(self, sceneHash);
    }

    static bool ValidateSceneBeforeLoading_Hook(Func<NetworkSceneManager, int, string, LoadSceneMode, bool> orig, NetworkSceneManager self, int sceneIndex, string sceneName, LoadSceneMode loadSceneMode)
    {
        bool valid = orig(self, sceneIndex, sceneName, loadSceneMode);
        //DebugHelper.LogWarning(valid ? $"Validation check success for scene: {sceneName}" : $"Bypassed validation check for scene {sceneName}");
        return true;
    }
}