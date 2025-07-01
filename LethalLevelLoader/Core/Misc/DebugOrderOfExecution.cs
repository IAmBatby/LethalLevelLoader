﻿using HarmonyLib;
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
            DebugHelper.Log("OrderOfExecution: StartOfRound Awake", DebugType.Developer);
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnEnable))]
        [HarmonyPrefix]
        public static void StartOfRound_OnEnable(StartOfRound __instance)
        {
            DebugHelper.Log("OrderOfExecution: StartOfRound OnEnable", DebugType.Developer);
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
        [HarmonyPrefix]
        public static void StartOfRound_Start(StartOfRound __instance)
        {
            DebugHelper.Log("OrderOfExecution: StartOfRound Start", DebugType.Developer);
        }

        //Round Manager

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Awake))]
        [HarmonyPrefix]
        public static void RoundManager_Awake(RoundManager __instance)
        {
            DebugHelper.Log("OrderOfExecution: RoundManager Awake", DebugType.Developer);
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Start))]
        [HarmonyPrefix]
        public static void RoundManager_Start(RoundManager __instance)
        {
            DebugHelper.Log("OrderOfExecution: RoundManager Start", DebugType.Developer);
        }

        //Time Of Day

        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.Awake))]
        [HarmonyPrefix]
        public static void TimeOfDay_Awake(TimeOfDay __instance)
        {
            DebugHelper.Log("OrderOfExecution: TimeOfDay Awake", DebugType.Developer);
        }

        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.Start))]
        [HarmonyPrefix]
        public static void TimeOfDay_Start(TimeOfDay __instance)
        {
            DebugHelper.Log("OrderOfExecution: TimeOfDay Start", DebugType.Developer);
        }

        //Terminal

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
        [HarmonyPrefix]
        public static void Terminal_Awake(Terminal __instance)
        {
            DebugHelper.Log("OrderOfExecution: Terminal Awake", DebugType.Developer);
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.OnEnable))]
        [HarmonyPrefix]
        public static void Terminal_OnEnable(Terminal __instance)
        {
            DebugHelper.Log("OrderOfExecution: Terminal OnEnable", DebugType.Developer);
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Start))]
        [HarmonyPrefix]
        public static void StartOfRound_Start(Terminal __instance)
        {
            DebugHelper.Log("OrderOfExecution: Terminal Start", DebugType.Developer);
        }
    }
}