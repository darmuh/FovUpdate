using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace FovUpdate
{
    [BepInPlugin("com.github.darmuh.FovUpdate", "FovUpdate", (PluginInfo.PLUGIN_VERSION))]

    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance = null!;
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "com.github.darmuh.FovUpdate";
            public const string PLUGIN_NAME = "FovUpdate";
            public const string PLUGIN_VERSION = "0.2.1";
        }

        internal static ManualLogSource Log = null!;

        private void Awake()
        {
            instance = this;
            Log = base.Logger;
            Log.LogInfo($"{PluginInfo.PLUGIN_NAME} is loading with version {PluginInfo.PLUGIN_VERSION}!");

            //Config.ConfigReloaded += OnConfigReloaded;
            FovConfig.Init();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo($"{PluginInfo.PLUGIN_NAME} load complete!");
        }

        internal static void Spam(string message)
        {
            if (FovConfig.DeveloperLogging.Value)
                Log.LogDebug(message);
            else
                return;
        }

        internal static void ERROR(string message)
        {
            Log.LogError(message);
        }

        internal static void WARNING(string message)
        {
            Log.LogWarning(message);
        }
    }
}
