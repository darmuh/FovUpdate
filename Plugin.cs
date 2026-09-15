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
        internal static Plugin instance = null!;

        internal static ManualLogSource Log = null!;
        internal static List<Camera> playerCams = [];

        private void Awake()
        {
            instance = this;
            Log = base.Logger;
            Log.LogInfo($"{Name} is loading with version {Version}!");
            FovConfig.Init();
            instance.Config.SettingChanged += OnSettingChanged;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo($"{Name} load complete!");
            Log.LogInfo($"This version of the mod has been compiled for REPO version 0.4.4.3 (Cosmetic Update & More)\nEnjoy! :3");
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
            if (settingChangedArg.ChangedSetting == FovConfig.UI_Adjustments)
                UltraWideSupport.Rects = [];
        }

        internal static void UpdateCams()
        {
            if (CameraZoom.Instance == null)
                return;

            playerCams.RemoveAll(c => c == null);
            CameraZoom.Instance.cams.DoIf(c => !playerCams.Contains(c), c => playerCams.Add(c));
        }

        // ---- Testing Methods, accessible via UnityExplorer ---- //
        private static void TestUltraWide() => SetWindowRes(1720, 720);

        private static void Test1610() => SetWindowRes(1280, 800);

        private static void TestMobileRes() => SetWindowRes(720, 1280);

        private static void ReloadScene() => RunManager.instance.RestartScene();

        internal static void SetWindowRes(int width, int height)
        {
            UnityEngine.Screen.SetResolution(width, height, false);
        }
        // ---- Testing Methods, accessible via UnityExplorer ---- //

        // get difference from the norm of 1.7777778f
        internal static float GetRatioModifier()
        {
            //Spam($"GetRatioModifier - {UltraWideSupport.defaultAspectRatio} / {UltraWideSupport.currentAspectRatio}");
            return UltraWideSupport.defaultAspectRatio / UltraWideSupport.currentAspectRatio;
        }

        internal static void AdjustToDifference(RectTransform transform, bool updatePos = true)
        {
            Spam($"AdjustToDifference {transform.name}, updatePos = {updatePos}");
            float xScale = transform.localScale.x;
            float yScale = transform.localScale.y;
            float zScale = transform.localScale.z;
            float ratio = GetRatioModifier();

            // ultrawide
            if (ratio < 1f)
            {
                if (xScale <= 0f)
                    return;

                // remove horizontal stretching from UI elements using ratio
                transform.localScale = new Vector3(xScale * ratio, yScale, zScale);
                // adjust anchor based on calculated diff
                if (updatePos)
                    transform.anchoredPosition = new Vector2(transform.anchoredPosition.x * ratio, transform.anchoredPosition.y);
            }

            // taller than 16:9 resolutions (like 16:10)
            if (ratio > 1f)
            {
                if (yScale <= 0f)
                    return;

                // remove vertical stretching from UI elements using ratio
                // dividing rather than multiplying for vertical scaling
                transform.localScale = new Vector3(xScale, yScale / ratio, zScale);
                // adjust anchor based on calculated diff
                if (updatePos)
                {
                    transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, transform.anchoredPosition.y / ratio);
                } 
                Spam($"Position: {transform.anchoredPosition}");
            }
            
        }
        internal static bool AreWeInGame()
        {
            return !SemiFunc.MenuLevel();
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
