using BepInEx.Configuration;

namespace FovUpdate
{
    public static class FovConfig
    {
        internal static ConfigEntry<bool> DeveloperLogging = null!;
        internal static ConfigEntry<float> UserDefinedFov = null!;
        internal static ConfigEntry<float> UserCrouchFov = null!;
        internal static ConfigEntry<float> UserSprintFov = null!;
        public static ConfigEntry<bool> AspectRatioFix = null!;
        public static ConfigEntry<float> ResMultiplier = null!;


        internal static void Init()
        {
            DeveloperLogging = Plugin.instance.Config.Bind("Debug", "Developer Logging", false, new ConfigDescription("Enable this to see developer logging output"));
            UserDefinedFov = Plugin.instance.Config.Bind("Settings", "Fov", 70f, new ConfigDescription("Set this to desired Fov value (standard gameplay)", new AcceptableValueRange<float>(45f, 140f)));
            UserCrouchFov = Plugin.instance.Config.Bind("Settings", "CrouchFov", 55f, new ConfigDescription("Set this to desired Fov value for when the player is crouched (tumble mode)", new AcceptableValueRange<float>(45f, 140f)));
            UserSprintFov = Plugin.instance.Config.Bind("Settings", "SprintFov", 20f, new ConfigDescription("Set this to desired modifier for your fov when sprinting.\nThis number will be added on to your regular fov when you start sprinting.\nDefault is vanilla (20)", new AcceptableValueRange<float>(0f, 100f)));
            AspectRatioFix = Plugin.instance.Config.Bind("Settings", "Aspect-ratio fix on/off", false, "Set this to true to enable Oksamies' UltrawideOrLongFix for widescreen compatibility");
            ResMultiplier = Plugin.instance.Config.Bind("Settings", "Resolution Multiplier", 1f, new ConfigDescription("Use this to upscale or downscale your game!\nNOTE: This config item will override the \"Pixelation\" graphics setting except when the default is set. (1)", new AcceptableValueRange<float>(0.25f, 4.00f)));
        }
    }
}
