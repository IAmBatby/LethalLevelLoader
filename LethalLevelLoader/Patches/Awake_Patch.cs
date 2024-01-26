using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [HarmonyPatch(typeof(MonoBehaviour))]
    internal class Awake_Patch
    {

        static void Prefix(object[] __args, MethodBase __originalMethod)
        {
            DebugHelper.Log("Ye");

            var parameters = __originalMethod.GetParameters();
            Debug.Log($"Method {__originalMethod.FullDescription()}:");
            for (var i = 0; i < __args.Length; i++)
                Debug.Log($"{parameters[i].Name} of type {parameters[i].ParameterType} is {__args[i]}");
        }

        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(RoundManager));
            Debug.Log("Assembly Name Is: " + assembly.FullName);

            IEnumerable<MethodBase> returnMethodBase = new List<MethodBase>();

            foreach (Type type in assembly.GetTypes())
                foreach (MethodInfo method in type.GetMethods())
                    if (method != null && method.Name == "SendMessage")
                        returnMethodBase = returnMethodBase.AddItem(method);

            foreach (MethodBase method in returnMethodBase)
                Debug.Log(method.GetType() + " : " + method.Name);

            return returnMethodBase;
        }
    }
}
