using BepInEx.Configuration;

namespace FovUpdate
{
    internal static class FovConfig
    {
        internal static ConfigEntry<bool> DeveloperLogging = null!;
        internal static ConfigEntry<float> UserDefinedFov = null!;
        internal static ConfigEntry<float> UserCrouchFov = null!;


        internal static void Init()
        {
            DeveloperLogging = Plugin.instance.Config.Bind<bool>("Debug", "Developer Logging", false, new ConfigDescription("Enable this to see developer logging output"));
            UserDefinedFov = Plugin.instance.Config.Bind<float>("Settings", "Fov", 70f, new ConfigDescription("Set this to desired Fov value (standard gameplay)", new AcceptableValueRange<float>(45f, 140f)));
            UserCrouchFov = Plugin.instance.Config.Bind<float>("Settings", "CrouchFov", 55f, new ConfigDescription("Set this to desired Fov value for when the player is crouched (tumble mode)", new AcceptableValueRange<float>(45f, 140f)));
        }
    }
}
