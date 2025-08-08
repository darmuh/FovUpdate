using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static FovUpdate.Plugin;

namespace FovUpdate
{
    [HarmonyPatch(typeof(CameraZoom), nameof(CameraZoom.Awake))]
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
                if (FovConfig.AspectRatioFix.Value)
                    UltraWideSupport.StretchFix(x);

            });

            __instance.zoomPrev = FovConfig.UserDefinedFov.Value;
            __instance.zoomNew = FovConfig.UserDefinedFov.Value;
            __instance.zoomCurrent = FovConfig.UserDefinedFov.Value;
            __instance.playerZoomDefault = FovConfig.UserDefinedFov.Value;
            __instance.SprintZoom = FovConfig.UpdateSprintConfigItem();
        }

        internal static bool AreWeInGame()
        {
            if (RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu)
                return false;

            if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu)
                return false;

            return true;
        }
    }

    //ResolutionOverride
    [HarmonyPatch(typeof(GraphicsManager), nameof(GraphicsManager.UpdateRenderSize))]
    public class ResolutionOverride
    {
        internal static float lastMultiplier = 1f;
        internal static bool EarlyReturn = false;
        public static void SetResolutionFix()
        {
            //do not override resoltuion
            if (FovConfig.ResMultiplier.Value == 1f && lastMultiplier == FovConfig.ResMultiplier.Value)
            {
                Spam("Pixelation setting remaining unchanged");
                return;
            }

            //Use game's pixelation setting
            if (FovConfig.ResMultiplier.Value == 1f)
            {
                Spam("Updating to base-game's pixelation setting");
                lastMultiplier = FovConfig.ResMultiplier.Value;
                EarlyReturn = true;
                GraphicsManager.instance.UpdateRenderSize();
                return;
            }

            float desiredWidth = Screen.width * FovConfig.ResMultiplier.Value;
            float desiredHeight = Screen.height * FovConfig.ResMultiplier.Value;
            float maxTextureSize = SystemInfo.maxTextureSize;

            //github issue #5, Res Multiplier error
            if (desiredWidth > maxTextureSize || desiredHeight > maxTextureSize)
            {
                WARNING($"Unable to apply Resolution Multiplier of {FovConfig.ResMultiplier.Value}!\nExpected Height ({desiredHeight}) or Width ({desiredWidth}) is larger than maxium Unity supported value of (GPU limit) {maxTextureSize}");

                FovConfig.ResMultiplier.Value = Mathf.Min(maxTextureSize / (float)Screen.width, maxTextureSize / (float)Screen.height);

                desiredWidth = (float)Screen.width * FovConfig.ResMultiplier.Value;
                desiredHeight = (float)Screen.height * FovConfig.ResMultiplier.Value;

                Log.LogMessage($"Resolution multiplier clamped to {FovConfig.ResMultiplier.Value} (GPU limit: {maxTextureSize}) Expected Width: {desiredWidth} Expected Heigh: {desiredHeight}");
            }

            // Apply the resolution (ensure integer values)
            RenderTextureMain.instance.textureWidthOriginal = Mathf.FloorToInt(desiredWidth);
            RenderTextureMain.instance.textureHeightOriginal = Mathf.FloorToInt(desiredHeight);
            lastMultiplier = FovConfig.ResMultiplier.Value;
            Spam($"RenderTexture resolution set to: {desiredWidth}x{desiredHeight} via multiplier {FovConfig.ResMultiplier.Value}");
        }

        public static void Postfix()
        {
            if (EarlyReturn)
            {
                EarlyReturn = false;
                return;
            }

            SetResolutionFix();
        }
    }

    //UpdateWindowMode
    //original source - https://github.com/Oksamies/UltrawideOrLongFix/blob/main/Plugin.cs#L67-L113 
    [HarmonyPatch(typeof(GraphicsManager), nameof(GraphicsManager.Update))]
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
            if (Rects.Count == 0)
            {
                if(!FovConfig.StretchUI())
                    Rects = [RenderTextureMain.instance.gameObject.GetComponent<RectTransform>()];
                else
                    Rects = [.. RenderTextureMain.instance.gameObject.GetComponentsInChildren<RectTransform>()];
                newRect = true;
            }

            currentAspectRatio = (float)Screen.width / (float)Screen.height;

            //Check if aspect ratio has changed or if our reference rects were updated
            if (previousAspectRatio == currentAspectRatio && !newRect)
                return;

            //Update all rects with the appropriate sizeDelta
            if (currentAspectRatio > defaultAspectRatio)
            {
                Rects.Do(r => r.sizeDelta = new Vector2(428 * currentAspectRatio, 428));
                ScreenIs = ScreenStatus.Wide;
                Spam("Updating Aspect Ratio for ultrawide support!");
            }
            else if (currentAspectRatio == defaultAspectRatio)
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
            //Calls method to update camera aspect ratios to avoid stretching
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



    [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.SpawnRPC))]
    public class SpawnPlayerStuff
    {
        public static void Postfix(PlayerAvatar __instance)
        {
            if (!CameraPatchThings.AreWeInGame() || !__instance.photonView.IsMine)
                return;

            if (Camera.main.fieldOfView == FovConfig.UserDefinedFov.Value)
            {
                Log.LogMessage("@SpawnPatch: Fov already set to correct value");
                return;
            }

            PlayerAvatar.instance.StartCoroutine(ChatCommandHandler.ForceFovZoomCurve(FovConfig.UserDefinedFov.Value, PlayerAvatar.instance.gameObject));

            Log.LogMessage($"@SpawnPatch: Fov set to number [ {FovConfig.UserDefinedFov.Value} ]");

            if (CameraZoom.Instance == null)
                return;

            CameraZoom.Instance.SprintZoom = FovConfig.UpdateSprintConfigItem();
            Log.LogMessage($"@SpawnPatch: SprintFov set to number [ {CameraZoom.Instance.SprintZoom} ]");
        }
    }

    [HarmonyPatch(typeof(SemiFunc), nameof(SemiFunc.OnSceneSwitch))]
    public class LeaveToMainReset
    {
        public static void Postfix()
        {
            //fix stretched aspect ratio
            if (UltraWideSupport.ScreenIs != UltraWideSupport.ScreenStatus.Default)
            {
                UpdateCams();
                playerCams.Do(x => UltraWideSupport.StretchFix(x));
            }
            return;
        }
    }

    [HarmonyPatch(typeof(CameraZoom), nameof(CameraZoom.OverrideZoomSet))]
    public class EffectsFix
    {
        public static void Prefix(ref float zoom)
        {
            bool adjust = true;
            //early return for:
            //patch disabled via config
            //ForceFovZoomCurve coroutine is running (changingFov bool)
            //TumbleCheck returns true (player is crouching to crouchfov value)
            if (ChatCommandHandler.changingFov || MapAndConfigCheck(ref zoom, ref adjust) || TumbleCheck())
                return;

            //we get our fov scale by dividing the vanilla fov by our set value
            //Then we adjust the zoom value of this method to be divided by our fov scale
            //adjust boolean is modified by map check to skip this if needed
            if (adjust)
            {
                float fovScale = 70f / FovConfig.UserDefinedFov.Value;
                zoom /= fovScale;
            }
            
            //if the max fov fix is enabled, clamp it
            if(FovConfig.ClampFix.Value)
                Mathf.Clamp(zoom, 1f, FovConfig.MaximumPossibleFov.Value);
            
            Spam($"zoom set to {zoom}\n Adjusted: {adjust}");
        }

        private static bool MapAndConfigCheck(ref float value, ref bool cont)
        {
            if(Map.Instance != null)
            {
                if (FovConfig.MapFov.Value != 50f && Map.Instance.Active)
                {
                    Spam("Adjusting mapfov!");
                    value = FovConfig.MapFov.Value;
                    cont = false;
                    return false;
                }
                    
            }

            return !FovConfig.EffectsFix.Value;
        }

        private static bool TumbleCheck()
        {
            if (PlayerAvatar.instance == null || PlayerController.instance == null)
                return false;

            if (PlayerAvatar.instance.tumble == null)
                return false;

            //inputdisabletimer because enemies like upscream force you into a crouch and affect your zoom differently than the crouch fov
            return PlayerAvatar.instance.tumble.isTumbling && PlayerController.instance.InputDisableTimer <= 0f;
        }
    }

    [HarmonyPatch(typeof(SemiFunc), nameof(SemiFunc.OnScreen))]
    public class AdjustOnScreenBool
    {
        public static void Prefix(ref float paddWidth, ref float paddHeight)
        {
            //early return if we are not in game or the fix is disabled
            if (!CameraPatchThings.AreWeInGame() || !FovConfig.OnScreenFix.Value)
                return;

            //get fov scale by dividing our defined fov by the vanilla 70
            //this is different than our other fovscale calculations because the padding needs to be smaller for higher fovs
            float fovScale = FovConfig.UserDefinedFov.Value / 70f;

            paddHeight /= fovScale;
            paddWidth /= fovScale;
        }
    }

    [HarmonyPatch(typeof(CameraZoom), nameof(CameraZoom.Update))]
    [HarmonyPriority(Priority.Last)]
    public class CameraZoomUpdateClamp
    {
        static int replacements = 0;

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CameraZoom_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogMessage("CameraZoomUpdateClamp Transpiler Initialized");
            replacements = 0;
            return Transpilers.Manipulator(instructions, instruction =>
            instruction.Calls(AccessTools.Method(("UnityEngine.Camera:set_fieldOfView"))), NewInstruction);
        }

        internal static void NewInstruction(CodeInstruction instruction)
        {
            //our replacement will run the delegate below
            CodeInstruction replacement = Transpilers.EmitDelegate<Action<Camera, float>>((cam, fov) =>
            {
                //check if we are using the clamp fix or not
                if (FovConfig.ClampFix.Value)
                {
                    cam.fieldOfView = Mathf.Clamp(fov, 1f, FovConfig.MaximumPossibleFov.Value);
                }
                else
                    cam.fieldOfView = fov;
                
            });
            instruction.opcode = replacement.opcode;
            instruction.operand = replacement.operand;
            replacements++;
            Log.LogMessage($"CameraZoom_Transpiler patched Update to clamp the final fov value!\n[ {replacements} ] lines changed");
        }
    }

    [HarmonyPatch(typeof(PlayerTumble), nameof(PlayerTumble.Update))]
    [HarmonyPriority(Priority.Last)]
    public class TumbleAdjustment
    {
        static int replacements = 0;
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TumbleAdjustment_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogMessage("TumbleAdjustment Transpiler Initialized");
            replacements = 0;
            //ldc.i4.s
            //only looks for our specific float value and replaces it
            return Transpilers.Manipulator(instructions, x => ChangeThisFloat(x, 55f), NewInstruction);
        }

        internal static void NewInstruction(CodeInstruction instruction)
        {
            CodeInstruction replacement = Transpilers.EmitDelegate(OverrideZoomSpecial);
            instruction.opcode = replacement.opcode;
            instruction.operand = replacement.operand;
            replacements++;
            Log.LogMessage($"TumbleAdjustment patched crouchfov config!\n[ {replacements} ] lines changed");
        }

        internal static bool ChangeThisFloat(CodeInstruction instruction, float expectedValue)
        {
            if (instruction.opcode != OpCodes.Ldc_R4)
                return false;

            if (!float.TryParse(instruction.operand.ToString(), out float value))
                return false;

            if (value != expectedValue)
                return false;

            return true;
        }

        internal static float OverrideZoomSpecial()
        {
            return FovConfig.UserCrouchFov.Value;
        }
    }

    //ChatManager MessageSend
    [HarmonyPatch(typeof(ChatManager), nameof(ChatManager.MessageSend))]
    public class ChatCommandHandler
    {
        private static string lastMsg = ":o";
        internal static bool changingFov = false; //now internal for use by other patches
        public static bool Prefix(ChatManager __instance)
        {
            if (__instance.chatMessage == lastMsg)
                return true;

            Spam($"Reading chat message - {__instance.chatMessage}");
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
                Log.LogMessage($"Invalid sprint fov message format given - [ {message} ]");
                return $"No number specified for /sfov!";
            }
            else
            {
                string fovNumString = args[1];
                if (float.TryParse(fovNumString, out float fov))
                {
                    if (fov < 0f || fov > 100f)
                        return $"Unable to set sprint fov to {fov} (out of range)";

                    FovConfig.UserSprintFov.Value = fov;
                    CameraZoom.Instance.SprintZoom = FovConfig.UpdateSprintConfigItem();

                    Log.LogMessage($"SprintFov set to number [ {CameraZoom.Instance.SprintZoom} ]");
                    return $"Sprint fov updated";
                }
                else
                {
                    
                    Log.LogMessage($"Unable to parse value from {fovNumString}!");
                    return $"Unable to parse value from {fovNumString}!";
                }
            }
        }

        internal static string HandleCrouchFovChange(string message)
        {
            if (CameraZoom.Instance.OverrideActive && !PlayerAvatar.instance.tumble.isTumbling)
            {
                Log.LogMessage("Unable to update crouch fov, camera OverrideActive is true! (not tumbling)");
                lastMsg = "";
                return $"Unable to change crouch fov, overriden by outside entity!";
            }

            string[] args = message.Split(' ');

            if (args.Length == 1)
            {
                Log.LogMessage($"Invalid crouch fov message format given - [ {message} ]");
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

                    Log.LogMessage($"CrouchFov set to number [ {fov} ]");
                    return $"crouch fov set to {fov}";
                }
                else
                {
                    Log.LogMessage($"Unable to parse value from {fovNumString}!");
                    return $"Unable to parse value from {fovNumString}!";
                }
            }
        }

        internal static string HandleFovChange(string message)
        {
            if (CameraZoom.Instance.OverrideActive && !PlayerAvatar.instance.isTumbling)
            {
                Log.LogMessage("Unable to update fov, camera OverrideActive is true!");
                lastMsg = "";
                return $"Unable to change fov, camera is overriden!";
            }

            string[] args = message.Split(' ');

            if (args.Length == 1)
            {
                Log.LogMessage($"Invalid fov message format given - [ {message} ]");
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
                        CameraZoom.Instance.SprintZoom = FovConfig.UpdateSprintConfigItem();
                        return $"fov will be {fov} when you get up";
                    }

                    ChatManager.instance.StartCoroutine(ForceFovZoomCurve(fov, PlayerAvatar.instance.gameObject));
                    FovConfig.UserDefinedFov.Value = fov;
                    Log.LogMessage($"Fov set to number [ {fov} ]");
                    CameraZoom.Instance.SprintZoom = FovConfig.UpdateSprintConfigItem();
                    return $"fov set to {fov}";
                }
                else
                {
                    Log.LogMessage($"Unable to parse value from {fovNumString}!");
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

            changingFov = true; //moved to before overridezoomset function for other patches
            CameraZoom.Instance.zoomPrev = CameraZoom.Instance.zoomCurrent;
            CameraZoom.Instance.OverrideZoomSet(newFov, 2f, 1f, 1f, obj, 150);

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
