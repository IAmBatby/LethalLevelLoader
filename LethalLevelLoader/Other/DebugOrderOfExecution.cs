using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    class DebugOrderOfExecution
    {
        //Start Of Round

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        [HarmonyPrefix]
        public static void StartOfRound_Awake(StartOfRound __instance)
        {
            DebugHelper.Log("OrderOfExecution: StartOfRound Awake");
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnEnable))]
        [HarmonyPrefix]
        public static void StartOfRound_OnEnable(StartOfRound __instance)
        {
            DebugHelper.Log("OrderOfExecution: StartOfRound OnEnable");
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
        [HarmonyPrefix]
        public static void StartOfRound_Start(StartOfRound __instance)
        {
            DebugHelper.Log("OrderOfExecution: StartOfRound Start");
        }

        //Round Manager

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Awake))]
        [HarmonyPrefix]
        public static void RoundManager_Awake(RoundManager __instance)
        {
            DebugHelper.Log("OrderOfExecution: RoundManager Awake");
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Start))]
        [HarmonyPrefix]
        public static void RoundManager_Start(RoundManager __instance)
        {
            DebugHelper.Log("OrderOfExecution: RoundManager Start");
        }

        //Time Of Day

        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.Awake))]
        [HarmonyPrefix]
        public static void TimeOfDay_Awake(TimeOfDay __instance)
        {
            DebugHelper.Log("OrderOfExecution: TimeOfDay Awake");
        }

        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.Start))]
        [HarmonyPrefix]
        public static void TimeOfDay_Start(TimeOfDay __instance)
        {
            DebugHelper.Log("OrderOfExecution: TimeOfDay Start");
        }

        //Terminal

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
        [HarmonyPrefix]
        public static void Terminal_Awake(Terminal __instance)
        {
            DebugHelper.Log("OrderOfExecution: Terminal Awake");
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.OnEnable))]
        [HarmonyPrefix]
        public static void Terminal_OnEnable(Terminal __instance)
        {
            DebugHelper.Log("OrderOfExecution: Terminal OnEnable");
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Start))]
        [HarmonyPrefix]
        public static void StartOfRound_Start(Terminal __instance)
        {
            DebugHelper.Log("OrderOfExecution: Terminal Start");
        }
    }
}
