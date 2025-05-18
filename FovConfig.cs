using BepInEx.Configuration;
using UnityEngine;

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

        internal static bool ChangingSprintFov = false;
        
        //caps fov at 180 while allowing fov to still raise with speed upgrades
        internal static float GetCappedSprintFov()
        {
            float upgradeMod = (float)StatsManager.instance.playerUpgradeSpeed[PlayerController.instance.playerAvatarScript.steamID] * 2f;
            float withUpgrades = UserSprintFov.Value + upgradeMod;
            float projectedFov = withUpgrades + UserDefinedFov.Value;
            if (projectedFov > 180f)
            {
                Plugin.Spam($"value = 180f - {projectedFov}, min = 0 - {upgradeMod}, max = {UserSprintFov.Value}");
                float CappedSprint = ClampNeg(UserSprintFov.Value + (180f - projectedFov), 0 - upgradeMod, UserSprintFov.Value);
                Plugin.Log.LogMessage($"Clamping sprint fov with speed upgrades to not push FOV beyond maximum of 180!\n\nReturning value: {CappedSprint}");
                return CappedSprint;
            }

            return UserSprintFov.Value;
        }

        private static float ClampNeg(float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max) 
                return max;

            return value;
        }

        internal static float UpdateSprintConfigItem()
        {
            ChangingSprintFov = true;
            if (UserSprintFov.Value + UserDefinedFov.Value > 180f)
            {
                Plugin.Log.LogMessage($"Sprint FOV + desired FOV exceeds maximum of 180. Lowering Sprint FOV to maximum acceptable value");
                UserSprintFov.Value = Mathf.Clamp(180f - UserDefinedFov.Value, 0, 100);
            }

            ChangingSprintFov = false;
            return GetCappedSprintFov();
        }

        internal static void Init()
        {
            DeveloperLogging = Plugin.instance.Config.Bind("Debug", "Developer Logging", false, new ConfigDescription("Enable this to see developer logging output"));
            UserDefinedFov = Plugin.instance.Config.Bind("Settings", "Fov", 70f, new ConfigDescription("Set this to desired Fov value (standard gameplay)", new AcceptableValueRange<float>(45f, 140f)));
            UserCrouchFov = Plugin.instance.Config.Bind("Settings", "CrouchFov", 55f, new ConfigDescription("Set this to desired Fov value for when the player is crouched (tumble mode)", new AcceptableValueRange<float>(45f, 140f)));
            UserSprintFov = Plugin.instance.Config.Bind("Settings", "SprintFov", 20f, new ConfigDescription("Set this to the desired base modifier for your fov when sprinting.\nThis number will be added on to your regular fov when you start sprinting.\nDefault in vanilla is 20.\nNOTE: This value will be capped based on your base Fov setting to ensure the fov can never exceed 180\nNOTE2: Speed upgrades also affect your fov! I have added a clamp so that even with speed upgrades your combined fov will never exceed 180, however, I do not stop the upgrades from increasing your fov if you set this to 0.", new AcceptableValueRange<float>(0f, 100f)));
            AspectRatioFix = Plugin.instance.Config.Bind("Settings", "Aspect-ratio fix on/off", false, "Set this to true to enable Oksamies' UltrawideOrLongFix for widescreen compatibility");
            ResMultiplier = Plugin.instance.Config.Bind("Settings", "Resolution Multiplier", 1f, new ConfigDescription("Use this to upscale or downscale your game!\nNOTE: This config item will override the \"Pixelation\" graphics setting except when the default is set. (1)", new AcceptableValueRange<float>(0.25f, 4.00f)));
        }
    }
}
