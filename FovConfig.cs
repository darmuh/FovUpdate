using BepInEx.Configuration;
using UnityEngine;

namespace FovUpdate
{
    public static class FovConfig
    {
        public enum AdjustUI
        {
            EnableAllFixes,
            MinimalFixes,
            NoFixes
        }

        internal static ConfigEntry<bool> DeveloperLogging = null!;
        internal static ConfigEntry<float> UserDefinedFov = null!;
        internal static ConfigEntry<float> UserCrouchFov = null!;
        internal static ConfigEntry<float> UserSprintFov = null!;
        internal static ConfigEntry<float> MapFov = null!;
        internal static ConfigEntry<float> MaximumPossibleFov = null!;
        public static ConfigEntry<bool> EffectsFix = null!;
        public static ConfigEntry<bool> OnScreenFix = null!;
        public static ConfigEntry<bool> AspectRatioFix = null!;
        public static ConfigEntry<AdjustUI> UI_Adjustments = null!;

        internal static bool ChangingSprintFov = false;

        internal static float UpdateSprintConfigItem()
        {
            if (UserSprintFov.Value + UserDefinedFov.Value > MaximumPossibleFov.Value)
            {
                ChangingSprintFov = true;
                Plugin.Log.LogMessage($"Sprint FOV + desired FOV exceeds maximum of {MaximumPossibleFov.Value}. Lowering Sprint FOV to maximum acceptable value");
                UserSprintFov.Value = Mathf.Clamp(MaximumPossibleFov.Value - UserDefinedFov.Value, 0, 100);
            }

            ChangingSprintFov = false;
            return UserSprintFov.Value;
        }

        internal static void Init()
        {
            DeveloperLogging = Plugin.instance.Config.Bind("Debug", "Developer Logging", false, new ConfigDescription("Enable this to see developer logging output"));
            UserDefinedFov = Plugin.instance.Config.Bind("Settings", "Fov", 70f, new ConfigDescription("Set this to desired Fov value (standard gameplay)", new AcceptableValueRange<float>(45f, 140f)));
            UserCrouchFov = Plugin.instance.Config.Bind("Settings", "CrouchFov", 55f, new ConfigDescription("Set this to desired Fov value for when the player is crouched (tumble mode)", new AcceptableValueRange<float>(45f, 140f)));
            UserSprintFov = Plugin.instance.Config.Bind("Settings", "SprintFov", 20f, new ConfigDescription("""
                Set this to the desired base modifier for your fov when sprinting.
                This number will be added on to your regular fov when you start sprinting.
                Default in vanilla is 20.
                NOTE: If Maximum FOV fix is enabled, this value will be capped based on your base Fov setting to ensure the fov can never exceed the number set via Maximum Possible Fov
                """, new AcceptableValueRange<float>(0f, 100f)));
            MapFov = Plugin.instance.Config.Bind("Settings", "MapFov", 50f, new ConfigDescription("Set this to desired Fov value for when pulling out your map.\nLeave at default value of 50 to let the Effects fix automatically adjust this value based on your base fov", new AcceptableValueRange<float>(35f, 140f)));
            AspectRatioFix = Plugin.instance.Config.Bind("Settings", "Aspect-ratio fix on/off", false, "Set this to true to enable compatibility with your monitor's resolution (Ultrawide/Ultralong)");
            UI_Adjustments = Plugin.instance.Config.Bind("Settings", "UI Adjustments", AdjustUI.EnableAllFixes, """
                Allows you to determine how much this mod will touch the UI.
                This setting requires a game restart to be used effectively.
                VALUES:
                EnableAllFixes - Allow mod full control to adjust UI.
                MinimalFixes - This only enables minimal fixes to the UI so that it fills the screen, as done in previous versions of this mod.
                NoFixes - No UI fixes will be enabled and the UI will be left untouched completely.
                """);
            OnScreenFix = Plugin.instance.Config.Bind("Settings", "OnScreen fix on/off", true, """
                This setting updates the OnScreen method calculation to account for your prefered FOV when enabled
                Should fix some of the more exagerated examples of being able to stare at enemies like the "Shadow Child" in the corner of your screen without triggering the looked at effect by using a high fov
                """);
            EffectsFix = Plugin.instance.Config.Bind("Settings", "Effects fix on/off", true, """
                This setting adjusts the strength of fov effects based on your base fov.
                So if you have a higher fov than standard, the amount you zoom in from enemies or items will be lessened to match a similar zoom distance as with the original default fov
                """);
            MaximumPossibleFov = Plugin.instance.Config.Bind("Settings", "Maximum Possible Fov", 170f, new ConfigDescription("Requires Maximum FOV fix to be enabled\nClamps the fov to never be higher than this number", new AcceptableValueRange<float>(150f, 200f)));
        }

        internal static bool FullUIFixesEnabled()
        {
            return UI_Adjustments.Value == AdjustUI.EnableAllFixes;
        }

        internal static bool MinimalUIFixesEnabled()
        {
            return UI_Adjustments.Value != AdjustUI.NoFixes;
        }
    }
}
