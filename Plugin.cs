using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace FovUpdate
{
    [BepInAutoPlugin]
    public partial class Plugin : BaseUnityPlugin
    {
        public static Plugin instance = null!;

        internal static ManualLogSource Log = null!;
        public static List<Camera> playerCams = [];

        private void Awake()
        {
            instance = this;
            Log = base.Logger;
            Log.LogInfo($"{Name} is loading with version {Version}!");
            FovConfig.Init();
            instance.Config.SettingChanged += OnSettingChanged;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo($"{Name} load complete!");
            Log.LogInfo($"This version of the mod has been compiled for REPO version 0.2.1 :)");
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs settingChangedArg)
        {
            Spam("CONFIG SETTING CHANGE EVENT");
            if (settingChangedArg.ChangedSetting == null || CameraZoom.Instance == null)
                return;

            if (settingChangedArg.ChangedSetting == FovConfig.DeveloperLogging || settingChangedArg.ChangedSetting == FovConfig.AspectRatioFix)
                Log.LogDebug($"{settingChangedArg.ChangedSetting.Definition.Key} is enabled [ {(bool)settingChangedArg.ChangedSetting.BoxedValue} ]");

            if (settingChangedArg.ChangedSetting == FovConfig.UserSprintFov && !FovConfig.ChangingSprintFov)
            {
                CameraZoom.Instance.SprintZoom = FovConfig.UpdateSprintConfigItem();
                Spam($"SprintFov updated to {CameraZoom.Instance.SprintZoom}");
            }

            if (settingChangedArg.ChangedSetting == FovConfig.ResMultiplier)
                ResolutionOverride.SetResolutionFix();

            if (settingChangedArg.ChangedSetting == FovConfig.UserDefinedFov)
            {
                if(!CameraZoom.Instance.OverrideActive && CameraNoPlayerTarget.instance == null)
                    PlayerAvatar.instance.StartCoroutine(ChatCommandHandler.ForceFovZoomCurve((float)settingChangedArg.ChangedSetting.BoxedValue, PlayerAvatar.instance.gameObject));
                else
                {
                    CameraZoom.Instance.playerZoomDefault = FovConfig.UserDefinedFov.Value;
                }
                CameraZoom.Instance.SprintZoom = FovConfig.UpdateSprintConfigItem();
                Spam($"Fov updated to {(float)settingChangedArg.ChangedSetting.BoxedValue}");
            }

            if (settingChangedArg.ChangedSetting == FovConfig.UserCrouchFov)
            {
                if (PlayerAvatar.instance == null)
                    return;

                if (PlayerAvatar.instance.tumble == null)
                    return;

                if(CameraZoom.Instance.OverrideActive && PlayerAvatar.instance.tumble.isTumbling)
                    PlayerAvatar.instance.StartCoroutine(ChatCommandHandler.ForceFovZoomCurve((float)settingChangedArg.ChangedSetting.BoxedValue, PlayerAvatar.instance.tumble.gameObject, false));

                Spam($"CrouchFov updated to {(float)settingChangedArg.ChangedSetting.BoxedValue}");
            }

            //refresh rects to adjust to new ui change
            if (settingChangedArg.ChangedSetting == FovConfig.DontStretchUI)
                UltraWideSupport.Rects = [];
        }

        internal static void UpdateCams()
        {
            if (CameraZoom.Instance == null)
                return;

            playerCams.RemoveAll(c => c == null);
            CameraZoom.Instance.cams.DoIf(c => !playerCams.Contains(c), c => playerCams.Add(c));
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
