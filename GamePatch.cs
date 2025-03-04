using System.Collections.Generic;
using HarmonyLib;

namespace FovUpdate
{
    [HarmonyPatch(typeof(CameraZoom), "Awake")]
    public class CameraPatchThings
    {
        //Below list not really needed until there's an in-game config editor
        //public static List<UnityEngine.Camera> playerCams = [];
        public static void Postfix(CameraZoom __instance)
        {
            if (!ShouldChangeFov())
                return;

            //playerCams.RemoveAll(c => c == null);
            __instance.cams.Do(x =>
            {
                //playerCams.Add(x);
                Plugin.Spam($"Original Fov of {x.name} is {x.fieldOfView}");
                x.fieldOfView = FovConfig.UserDefinedFov.Value;
                Plugin.Spam($"Field of view for {x.name} set to {FovConfig.UserDefinedFov.Value}");
            });
            __instance.zoomPrev = FovConfig.UserDefinedFov.Value;
            __instance.zoomNew = FovConfig.UserDefinedFov.Value;
            __instance.zoomCurrent = FovConfig.UserDefinedFov.Value;
            __instance.playerZoomDefault = FovConfig.UserDefinedFov.Value;
        }

        private static bool ShouldChangeFov()
        {
            if (SemiFunc.IsMainMenu())
                return false;

            if (SemiFunc.RunIsLobbyMenu())
                return false;

            if (SemiFunc.RunIsLobby())
                return false;

            return true;
        }
    }

 }
