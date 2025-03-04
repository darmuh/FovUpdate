using BepInEx.Configuration;

namespace FovUpdate
{
    internal static class FovConfig
    {
        internal static ConfigEntry<bool> DeveloperLogging = null!;
        internal static ConfigEntry<float> UserDefinedFov = null!;


        internal static void Init()
        {
            DeveloperLogging = Plugin.instance.Config.Bind<bool>("Debug", "Developer Logging", false, new ConfigDescription("Enable this to see developer logging output"));
            UserDefinedFov = Plugin.instance.Config.Bind<float>("Settings", "Fov", 70f, new ConfigDescription("Set this to desired Fov value", new AcceptableValueRange<float>(45f, 140f)));
        }
    }
}
