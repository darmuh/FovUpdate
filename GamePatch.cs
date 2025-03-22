using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static FovUpdate.Plugin;

namespace FovUpdate
{
    [HarmonyPatch(typeof(CameraZoom), "Awake")]
    public class CameraPatchThings
    {

        public static void Prefix(CameraZoom __instance)
        {
            if (!AreWeInGame())
                return;

            playerCams.RemoveAll(c => c == null);
            __instance.cams.Do(x =>
            {
                playerCams.Add(x);
                Spam($"Original Fov of {x.name} is {x.fieldOfView}");
                x.fieldOfView = FovConfig.UserDefinedFov.Value;
                Spam($"Field of view for {x.name} set to {FovConfig.UserDefinedFov.Value}");
                if(FovConfig.AspectRatioFix.Value)
                    UltraWideSupport.StretchFix(x);
                
            });

            __instance.zoomPrev = FovConfig.UserDefinedFov.Value;
            __instance.zoomNew = FovConfig.UserDefinedFov.Value;
            __instance.zoomCurrent = FovConfig.UserDefinedFov.Value;
            __instance.playerZoomDefault = FovConfig.UserDefinedFov.Value;
            __instance.SprintZoom = FovConfig.UserSprintFov.Value;
        }

        internal static bool AreWeInGame()
        {
            if (SemiFunc.RunIsLobbyMenu())
                return false;

            if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu)
                return false;

            return true;
        }
    }

    //ResolutionOverride
    [HarmonyPatch(typeof(GraphicsManager), "UpdateRenderSize")]
    public class ResolutionOverride
    {
        internal static float lastScaleChange = 1f;
        public static void SetResolutionFix()
        {
            //return if value is the same or set to default
            if (FovConfig.ResMultiplier.Value == lastScaleChange)
                return;

            //resolution change, only if it does not match our cached value
            RenderTextureMain.instance.textureWidthOriginal = (float)Screen.width * (float)FovConfig.ResMultiplier.Value;
            RenderTextureMain.instance.textureHeightOriginal = (float)Screen.height * (float)FovConfig.ResMultiplier.Value;
            lastScaleChange = FovConfig.ResMultiplier.Value;
            Spam($"Resolution has been overriden with multiplier {FovConfig.ResMultiplier.Value}");
        }

        public static void Postfix()
        {
            Spam($"Overriding pixelation setting with resolution multiplier! {FovConfig.ResMultiplier.Value}");
            SetResolutionFix();
        }
    }

    //UpdateWindowMode
    //original source - https://github.com/Oksamies/UltrawideOrLongFix/blob/main/Plugin.cs#L67-L113 
    [HarmonyPatch(typeof(GraphicsManager), "Update")]
    public class UltraWideSupport
    {
        //Added for UltraWide Fov Fixes
        public enum ScreenStatus
        {
            Default,
            Wide,
            Tall
        }

        public static ScreenStatus ScreenIs = ScreenStatus.Default;
        
        public static float previousAspectRatio;
        public static float currentAspectRatio;
        public static readonly float defaultAspectRatio = 1.7777778f;
        public static List<RectTransform> Rects = [];
        public static void Postfix(GraphicsManager __instance)
        {
            //config item is disabled, skip patch
            if (!FovConfig.AspectRatioFix.Value)
                return;

            //no need to run code at this time
            if (__instance.fullscreenCheckTimer > 0f)
                return;

            //Remove null refs
            Rects.RemoveAll(r => r == null);

            //by default, we are not getting Rects every time this code is run
            bool newRect = false;

            //There are no cached RectTransforms at this time
            //Get the Rects from RenderTextureMain/RenderTextureOverlay(child)
            //set newRect to true, signifying we have gotten Rects this run
            if(Rects.Count == 0)
            {
                Rects = [.. RenderTextureMain.instance.gameObject.GetComponentsInChildren<RectTransform>()];
                newRect = true;
            }

            currentAspectRatio = (float)Screen.width / (float)Screen.height;

            //Check if aspect ratio has changed or if our reference rects were updated
            if (previousAspectRatio == currentAspectRatio && !newRect)
                return;

            //Update all rects with the appropriate sizeDelta
            //Also calculate fixed fov for playerCams
            if (currentAspectRatio > defaultAspectRatio)
            {
                Rects.Do(r => r.sizeDelta = new Vector2(428 * currentAspectRatio, 428));
                ScreenIs = ScreenStatus.Wide;
                Spam("Updating Aspect Ratio for ultrawide support!");
            }
            else if(currentAspectRatio == defaultAspectRatio)
            {
                Rects.Do(r => r.sizeDelta = new Vector2(750, 750 / currentAspectRatio));
                ScreenIs = ScreenStatus.Default;
                Spam("Updating Aspect Ratio to default!");
            }
            else
            {
                Rects.Do(r => r.sizeDelta = new Vector2(750, 750 / currentAspectRatio));
                ScreenIs = ScreenStatus.Tall;
                Spam("Updating Aspect Ratio for ultratall support!");
            }

            UpdateCams();
            //automated fov fix to make things feel less stretched
            playerCams.Do(c => StretchFix(c));

            //cache our aspectratio (for any changes)
            previousAspectRatio = currentAspectRatio;
        }

        public static void StretchFix(Camera cam)
        {
            if (!FovConfig.AspectRatioFix.Value || cam == null)
                return;

            currentAspectRatio = (float)Screen.width / (float)Screen.height;

            //Set aspect ratio of camera to avoid stretched cam
            cam.aspect = currentAspectRatio;
            Spam($"{cam.name} aspect ratio set to {cam.aspect}");
        }
    }

    [HarmonyPatch(typeof(PlayerAvatar), "Spawn")]
    public class SpawnPlayerFov
    {
        public static void Postfix(PlayerAvatar __instance)
        {
            if (!CameraPatchThings.AreWeInGame())
            {
                //fix fov in main menu
                if (UltraWideSupport.ScreenIs != UltraWideSupport.ScreenStatus.Default)
                {
                    UpdateCams();
                    playerCams.Do(x =>
                    {
                        UltraWideSupport.StretchFix(x);
                    });
                }
                return;
            }
                

            if (__instance.localCamera.fieldOfView == FovConfig.UserDefinedFov.Value)
            {
                Plugin.Spam("Fov already set to correct value");
                return;
            }

            __instance.StartCoroutine(ChatCommandHandler.ForceFovZoomCurve(FovConfig.UserDefinedFov.Value, __instance.gameObject));

            Plugin.Log.LogMessage($"@SpawnPatch: Fov set to number [ {FovConfig.UserDefinedFov.Value} ]");

            if (CameraZoom.Instance == null)
                return;

            CameraZoom.Instance.SprintZoom = FovConfig.UserSprintFov.Value;
            Plugin.Log.LogMessage($"@SpawnPatch: SprintFov set to number [ {FovConfig.UserSprintFov.Value} ]");
        }
    }

    [HarmonyPatch(typeof(PlayerTumble), "Update")]
    [HarmonyPriority(Priority.Last)]
    public class TumbleAdjustment
    {
        static int replacements = 0;
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TumbleAdjustment_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogMessage("TumbleAdjustment Transpiler Initialized");
            MethodInfo ChangeFov = AccessTools.Method("CameraZoom:OverrideZoomSet");
            replacements = 0;
            //ldc.i4.s
            instructions = Transpilers.Manipulator(instructions, x => ChangeThisFloat(x), NewInstruction);

            return instructions;
        }

        internal static void NewInstruction(CodeInstruction instruction)
        {
            CodeInstruction replacement = Transpilers.EmitDelegate(OverrideZoomSpecial);
            instruction.opcode = replacement.opcode;
            instruction.operand = replacement.operand;
            replacements++;
            Plugin.Log.LogMessage($"TumbleAdjustment patched crouchfov config!\n[ {replacements} ] lines changed");
        }

        internal static bool ChangeThisFloat(CodeInstruction instruction)
        {
            if (instruction.opcode != OpCodes.Ldc_R4)
                return false;

            if (!float.TryParse(instruction.operand.ToString(), out float value))
                return false;

            if (value != 55f)
                return false;

            return true;
        }

        internal static float OverrideZoomSpecial()
        {
            return FovConfig.UserCrouchFov.Value;
        }
    }

    //ChatManager MessageSend
    [HarmonyPatch(typeof(ChatManager), "MessageSend")]
    public class ChatCommandHandler
    {
        private static string lastMsg = ":o";
        private static bool changingFov = false;
        public static bool Prefix(ChatManager __instance)
        {
            if (__instance.chatMessage == lastMsg)
                return true;

            Plugin.Spam($"Reading chat message - {__instance.chatMessage}");
            lastMsg = __instance.chatMessage;

            if (__instance.chatMessage.StartsWith("/fov"))
                __instance.AddLetterToChat(" " + HandleFovChange(__instance.chatMessage));
            else if (__instance.chatMessage.StartsWith("/cfov"))
                __instance.AddLetterToChat(" " + HandleCrouchFovChange(__instance.chatMessage));
            else if (__instance.chatMessage.StartsWith("/sfov"))
                __instance.AddLetterToChat(" " + HandleSprintFovChange(__instance.chatMessage));

            return true;
        }

        internal static string HandleSprintFovChange(string message)
        {
            string[] args = message.Split(' ');

            if (args.Length == 1)
            {
                Plugin.Log.LogMessage($"Invalid sprint fov message format given - [ {message} ]");
                return $"No number specified for /sfov!";
            }
            else
            {
                string fovNumString = args[1];
                if (float.TryParse(fovNumString, out float fov))
                {
                    if (fov < 0f || fov > 100f)
                        return $"Unable to set sprint fov to {fov} (out of range)";

                    CameraZoom.Instance.SprintZoom = fov;
                    FovConfig.UserSprintFov.Value = fov;

                    Plugin.Log.LogMessage($"SprintFov set to number [ {fov} ]");
                    return $"Sprint fov set to {fov}";
                }
                else
                {
                    Plugin.Log.LogMessage($"Unable to parse value from {fovNumString}!");
                    return $"Unable to parse value from {fovNumString}!";
                }
            }
        }

        internal static string HandleCrouchFovChange(string message)
        {
            if (CameraZoom.Instance.OverrideActive && !PlayerAvatar.instance.tumble.isTumbling)
            {
                Plugin.Log.LogMessage("Unable to update crouch fov, camera OverrideActive is true! (not tumbling)");
                lastMsg = "";
                return $"Unable to change crouch fov, overriden by outside entity!";
            }

            string[] args = message.Split(' ');

            if (args.Length == 1)
            {
                Plugin.Log.LogMessage($"Invalid crouch fov message format given - [ {message} ]");
                return $"No number specified for /cfov!";
            }
            else
            {
                string fovNumString = args[1];
                if (float.TryParse(fovNumString, out float fov))
                {
                    if (fov < 45f || fov > 140f)
                        return $"Unable to set crouch fov to {fov} (out of range)";

                    if (PlayerAvatar.instance.isTumbling)
                        ChatManager.instance.StartCoroutine(ForceFovZoomCurve(fov, PlayerAvatar.instance.tumble.gameObject, false));

                    FovConfig.UserCrouchFov.Value = fov;

                    Plugin.Log.LogMessage($"CrouchFov set to number [ {fov} ]");
                    return $"crouch fov set to {fov}";
                }
                else
                {
                    Plugin.Log.LogMessage($"Unable to parse value from {fovNumString}!");
                    return $"Unable to parse value from {fovNumString}!";
                }
            }
        }

        internal static string HandleFovChange(string message)
        {
            if (CameraZoom.Instance.OverrideActive && !PlayerAvatar.instance.isTumbling)
            {
                Plugin.Log.LogMessage("Unable to update fov, camera OverrideActive is true!");
                lastMsg = "";
                return $"Unable to change fov, camera is overriden!";
            }

            string[] args = message.Split(' ');

            if (args.Length == 1)
            {
                Plugin.Log.LogMessage($"Invalid fov message format given - [ {message} ]");
                return $"No number specified for /fov!";
            }    
            else
            {
                string fovNumString = args[1];
                if (float.TryParse(fovNumString, out float fov))
                {
                    if (fov < 45f || fov > 140f)
                        return $"Unable to set fov to {fov} (out of range)";

                    if(PlayerAvatar.instance.isTumbling)
                    {
                        CameraZoom.Instance.playerZoomDefault = fov;
                        FovConfig.UserDefinedFov.Value = fov;
                        return $"fov will be {fov} when you get up";
                    }

                    ChatManager.instance.StartCoroutine(ForceFovZoomCurve(fov, PlayerAvatar.instance.gameObject));
                    FovConfig.UserDefinedFov.Value = fov;
                    Plugin.Log.LogMessage($"Fov set to number [ {fov} ]");
                    return $"fov set to {fov}";
                }
                else
                {
                    Plugin.Log.LogMessage($"Unable to parse value from {fovNumString}!");
                    return $"Unable to parse value from {fovNumString}!";
                }
            }
        }

        internal static IEnumerator ForceFovZoomCurve(float newFov, GameObject obj = null!, bool updateDef = true)
        {
            if (PlayerAvatar.instance == null)
                yield break;

            if(changingFov)
                yield break;

            if(obj == null)
                obj = PlayerController.instance.playerAvatar.gameObject;

            CameraZoom.Instance.zoomPrev = CameraZoom.Instance.zoomCurrent;
            CameraZoom.Instance.OverrideZoomSet(newFov, 2f, 1f, 1f, obj, 150);
            changingFov = true;

            while (Mathf.Abs(CameraZoom.Instance.zoomCurrent - newFov) > 0.25 && CameraZoom.Instance.OverrideZoomObject == obj)
            {
                //Plugin.Spam($"Fov is {CameraZoom.Instance.zoomCurrent}");
                yield return null!;
            }

            if(updateDef)
                CameraZoom.Instance.playerZoomDefault = newFov;

            changingFov = false;

            if (CameraZoom.Instance.OverrideZoomObject != obj)
                yield break;

            CameraZoom.Instance.OverrideActive = false;
            CameraZoom.Instance.OverrideZoomObject = null!;
            CameraZoom.Instance.OverrideZoomPriority = 999;
            CameraZoom.Instance.OverrideZoomSpeedIn = 1f;
            CameraZoom.Instance.OverrideZoomSpeedOut = 1f;
        }
    }
}
