using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace FovUpdate
{
    [HarmonyPatch(typeof(CameraZoom), "Awake")]
    public class CameraPatchThings
    {
        public static List<Camera> playerCams = [];
        public static void Prefix(CameraZoom __instance)
        {
            if (!ShouldChangeFov())
                return;

            playerCams.RemoveAll(c => c == null);
            __instance.cams.Do(x =>
            {
                playerCams.Add(x);
                Plugin.Spam($"Original Fov of {x.name} is {x.fieldOfView}");
                x.fieldOfView = FovConfig.UserDefinedFov.Value;
                Plugin.Spam($"Field of view for {x.name} set to {FovConfig.UserDefinedFov.Value}");
            });
            __instance.zoomPrev = FovConfig.UserDefinedFov.Value;
            __instance.zoomNew = FovConfig.UserDefinedFov.Value;
            __instance.zoomCurrent = FovConfig.UserDefinedFov.Value;
            __instance.playerZoomDefault = FovConfig.UserDefinedFov.Value;
            __instance.SprintZoom = FovConfig.UserSprintFov.Value;
        }

        internal static bool ShouldChangeFov()
        {
            if (SemiFunc.IsMainMenu())
                return false;

            if (GameDirector.instance.PlayerList.Count <= 0)
                return false;

            if (CameraNoPlayerTarget.instance == null)
                return true;

            if (CameraNoPlayerTarget.instance.enabled)
                return false;

            return true;
        }
    }

    //Spawn //PlayerAvatar
    [HarmonyPatch(typeof(PlayerAvatar), "Spawn")]
    public class SpawnPlayerFov
    {
        public static void Postfix(PlayerAvatar __instance)
        {
            if (!CameraPatchThings.ShouldChangeFov())
                return;

            if(__instance.localCamera.fieldOfView == FovConfig.UserDefinedFov.Value)
            {
                Plugin.Spam("Fov already set to correct value");
                return;
            }

            __instance.StartCoroutine(ChatCommandHandler.ForceFovZoomCurve(FovConfig.UserDefinedFov.Value, FovConfig.UserDefinedFov, __instance.gameObject));

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
            //Plugin.Spam($"Tumble Fov set to {FovConfig.UserCrouchFov.Value}");
            return FovConfig.UserCrouchFov.Value;
        }
    }

    //ChatManager MessageSend
    [HarmonyPatch(typeof(ChatManager), "MessageSend")]
    public class ChatCommandHandler
    {
        private static string lastMsg = "";
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
                    FovConfig.UserCrouchFov.Value = fov;

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
                        ChatManager.instance.StartCoroutine(ForceFovZoomCurve(fov, FovConfig.UserCrouchFov, PlayerAvatar.instance.tumble.gameObject, false));
                    else
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
                        return $"fov will be {fov} when you get up";
                    }

                    ChatManager.instance.StartCoroutine(ForceFovZoomCurve(fov, FovConfig.UserDefinedFov, PlayerAvatar.instance.gameObject));
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

        internal static IEnumerator ForceFovZoomCurve(float newFov, ConfigEntry<float> configEntry, GameObject obj = null!, bool updateDef = true)
        {
            if(changingFov)
                yield break;

            if(obj == null)
                obj = PlayerController.instance.playerAvatar.gameObject;

            CameraZoom.Instance.OverrideZoomSet(newFov, 2f, 1f, 1f, obj, 150);
            changingFov = true;

            while (Mathf.Abs(CameraZoom.Instance.zoomCurrent - newFov) > 0.25 && CameraZoom.Instance.OverrideZoomObject == obj)
            {
                //Plugin.Spam($"Fov is {CameraZoom.Instance.zoomCurrent}");
                yield return new WaitForEndOfFrame();
            }

            if(updateDef)
                CameraZoom.Instance.playerZoomDefault = newFov;

            configEntry.Value = newFov;
            Plugin.instance.Config.Save();
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
