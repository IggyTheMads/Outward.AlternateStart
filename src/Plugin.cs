using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SideLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AlternateStart
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SL.GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "com.iggy.altstart";
        const string NAME = "Alternate Start";
        const string VERSION = "1.0";

        public static Plugin Instance;

        public const string SL_PACK_NAME = "iggythemad AlternateStart";
        public const string QUEST_EVENT_FAMILY_NAME = "A_Iggy_AlternateStart";

        internal void Awake()
        {
            Instance = this;

            Logger.LogMessage($"{NAME} Awake()");

            ScenarioManager.Init();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        internal void OnGUI()
        {
            ScenarioManager.OnGUI();
        }

        internal static void Log(object log) => Instance.Logger.LogMessage(log?.ToString() ?? string.Empty);
        internal static void LogWarning(object log) => Instance.Logger.LogWarning(log?.ToString() ?? string.Empty);
        internal static void LogError(object log) => Instance.Logger.LogError(log?.ToString() ?? string.Empty);
    }
}
