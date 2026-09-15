using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static FovUpdate.Plugin;

namespace FovUpdate
{
    [HarmonyPatch(typeof(CameraZoom), nameof(CameraZoom.Awake))]
    public class CameraPatchThings
    {
        public static void Prefix(CameraZoom __instance)
        {
            // issue #10: if no player controller then this camera is probably a menu item (ie. customization menu)
            if (!AreWeInGame() || __instance.PlayerController == null)
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

            __instance.MaximumZoom = FovConfig.MaximumPossibleFov.Value;
            __instance.zoomPrev = FovConfig.UserDefinedFov.Value;
            __instance.zoomNew = FovConfig.UserDefinedFov.Value;
            __instance.zoomCurrent = FovConfig.UserDefinedFov.Value;
            __instance.playerZoomDefault = FovConfig.UserDefinedFov.Value;
            __instance.SprintZoom = FovConfig.UpdateSprintConfigItem();
        }

        
    }

    [HarmonyPatch(typeof(StatsUI), nameof(StatsUI.Update))]
    public class HideUpgradeTextFix
    {
        public static void Postfix(StatsUI __instance)
        {
            // while this fix could be applied even for standard aspect ratios, it's not necessary
            // Also allowing to opt out of this one with minimal fixes
            if (!FovConfig.AspectRatioFix.Value || !FovConfig.FullUIFixesEnabled())
                return;

            // hide upgrade text when its supposed to be hidden
            if (__instance.isHidden && __instance.Text.enabled)
                __instance.Text.enabled = false;
            else if(!__instance.isHidden && !__instance.Text.enabled)
                __instance.Text.enabled = true;

        }
    }

    [HarmonyPatch(typeof(HealthUI), nameof(HealthUI.Start))]
    public class GameHUDFix
    {
        public static void Prefix(HealthUI __instance)
        {
            // Allowed to opt out of this fix with minimal fixes
            if (!FovConfig.AspectRatioFix.Value || !FovConfig.FullUIFixesEnabled() || !AreWeInGame())
                return;

            float ratio = GetRatioModifier();

            // native fixes not needed
            if (ratio == 1f)
                return;

            // grabs all of in-game hud elements together cleanly
            Transform hud = __instance.transform.parent;
            if (hud.TryGetComponent<RectTransform>(out RectTransform rectTransform))
                AdjustToDifference(rectTransform, ratio < 1f); //only reposition for ultrawide res, not ultratall              
        }
    }

    [HarmonyPatch(typeof(SemiUI), nameof(SemiUI.Start))]
    public class SemiUIPositionFixes
    {
        private static TaskCompletionSource<float> ItemHintYTask = new();
        public static void Postfix(SemiUI __instance)
        {
            // Allowed to opt out of this fix with minimal fixes
            if (!FovConfig.AspectRatioFix.Value || !FovConfig.FullUIFixesEnabled() || !AreWeInGame())
                return;

            float ratio = GetRatioModifier();

            // not needed for native
            if (ratio == 1f)
                return;

            // ultrawide (maybe not needed)
            if (ratio < 1f)
            {
                __instance.hidePosition = new(__instance.hidePosition.x * ratio, __instance.hidePosition.y);
                __instance.showPosition = new(__instance.showPosition.x * ratio, __instance.showPosition.y);
            }

            // ultratall
            if (ratio > 1f)
            {
                bool shopNumbers = __instance.transform.parent.name == "Shop";
                bool itemInfo = __instance.GetComponent<ItemInfoUI>() != null;
                bool itemInfoExtra = __instance.GetComponent<ItemInfoExtraUI>() != null;

                if (itemInfoExtra)
                {
                    _ = GetItemHintY(__instance, ratio);
                    return;
                }
                   
                if (shopNumbers)
                {
                    __instance.hidePosition = new(__instance.hidePosition.x, __instance.hidePosition.y * ratio);
                    __instance.showPosition = new(__instance.showPosition.x, __instance.showPosition.y / ratio);

                    // fix scan lines showing up in regular gameplay for ultratall
                    if (!SemiFunc.IsLevelShop(RunManager.instance.levelCurrent))
                        __instance.AllChildrenSetActive(false);
                }
                else
                {
                    __instance.hidePosition = new(__instance.hidePosition.x, __instance.hidePosition.y * ratio);
                    __instance.showPosition = new(__instance.showPosition.x, __instance.showPosition.y * ratio);
                }

                if (itemInfo)
                {
                    Spam($"Setting result for ItemHintYTask to {__instance.showPosition.y}");
                    ItemHintYTask.TrySetResult(__instance.showPosition.y);
                }
                    
            }
        }

        private static async Task GetItemHintY(SemiUI __instance, float ratio)
        {
            if(!ItemHintYTask.Task.IsCompleted)
                await ItemHintYTask.Task;
            
            Spam($"Setting {__instance.name} new positions!");
            __instance.hidePosition = new(__instance.hidePosition.x, __instance.hidePosition.y * ratio);
            __instance.showPosition = new(__instance.showPosition.x, ItemHintYTask.Task.Result + 36f);

            Spam($"Show {__instance.showPosition}, Hide {__instance.hidePosition}");
            ItemHintYTask = new(); // reset task for next time patch runs
        }
    }

    [HarmonyPatch(typeof(VideoOverlay), nameof(VideoOverlay.Awake))]
    public class VideoFix
    {
        public static void Prefix(VideoOverlay __instance)
        {
            // Allowed to opt out with minimal UI fixes
            if (!FovConfig.AspectRatioFix.Value || !FovConfig.FullUIFixesEnabled())
                return;

            float ratio = GetRatioModifier();

            // no fixes needed for native
            if (ratio == 1f)
                return;

            // adjusts the video overlay transitions, there's a few of them I haven't tested but should be fine
            if (__instance.TryGetComponent<RectTransform>(out RectTransform rectTransform))
                AdjustToDifference(rectTransform, ratio < 1f); // only reposition ultrawide, not ultratall
        }
    }

    [HarmonyPatch(typeof(PlayerAvatarMenuHover), nameof(PlayerAvatarMenuHover.Awake))]
    public class UnStretchMenuAvatar
    {
        public static void Prefix(PlayerAvatarMenuHover __instance)
        {
            // this UI fix cannot be opted out of, it fixes issue #10 on github
            if (!FovConfig.AspectRatioFix.Value)
                return;

            // check if parent menu is MenuPageEsc
            bool adjustPos = __instance.transform.parent?.parent?.GetComponent<MenuPageEsc>() == null;

            // adjusts the avatar preview and will only update the position if the menu is NOT the escape menu
            if (__instance.TryGetComponent<RectTransform>(out RectTransform rectTransform))
                AdjustToDifference(rectTransform, adjustPos);
        }
    }

    // resize menu cursor to match other UI elements
    [HarmonyPatch(typeof(MenuCursor), nameof(MenuCursor.Start))]
    public class MenuCursorSizeFix
    {
        public static void Prefix(MenuCursor __instance)
        {
            if (!FovConfig.AspectRatioFix.Value || !FovConfig.FullUIFixesEnabled())
                return;

            // fix not needed for native
            if (GetRatioModifier() == 1f) 
                return;

            Transform? cursor = __instance.transform.GetChild(0);

            if (cursor != null)
            {
                if (cursor.TryGetComponent<RectTransform>(out RectTransform rectTransform))
                    AdjustToDifference(rectTransform, false); // no need to reposition the cursor
            }
        }
    }

    // resize selection target scale based on resolution
    [HarmonyPatch(typeof(MenuSelectionBox), nameof(MenuSelectionBox.MenuSelectionBoxSetTarget))]
    public class MenuSelectionFix
    {
        public static void Prefix(ref Vector3 scale, ref Vector2 customScale)
        {
            if (!FovConfig.AspectRatioFix.Value || !FovConfig.FullUIFixesEnabled())
                return;

            // calculate diference from standard resolution
            float ratio = GetRatioModifier();

            // ultrawide
            if (ratio < 1f)
            {
                // remove horizontal stretching from UI elements using ratio
                scale = new Vector3(scale.x * ratio, scale.y, scale.z);

                // remove horizontal stretching for slider based menu items
                customScale = new Vector2(customScale.x * ratio, customScale.y);
            }

            // ultratall
            if (ratio > 1f)
            {
                // remove vertical stretching from UI elements using ratio
                scale = new Vector3(scale.x, scale.y / ratio, scale.z);

                // remove vertical stretching for slider based menu items
                customScale = new Vector2(customScale.x, customScale.y / ratio);
            }
            
        }
    }

    // fix misaligned menu buttons in cosmetics page
    [HarmonyPatch(typeof(MenuPageCosmetics), nameof(MenuPageCosmetics.Start))]
    public class CosmeticsPageFixes
    {
        public static void Postfix(MenuPageCosmetics __instance)
        {
            if (!FovConfig.AspectRatioFix.Value || !FovConfig.FullUIFixesEnabled())
                return;

            // this one is a bit of a messy fix because even when I don't touch the UI these elements are borked outside native res
            // The below is not perfect and results may vary by each resolution

            float ratioMod = GetRatioModifier();

            // native res does not need any changes
            if (ratioMod == 1)
                return;

            AdjustCosmeticCategoryButtons(__instance.categoriesTransform.parent, __instance.categoriesHolder, ratioMod);
            AdjustCosmeticCategoryButtons(__instance.subCategoriesTransform.parent, __instance.subCategoriesHolder, ratioMod);

        }

        // breakout method for re-use between categories/subcategories buttons
        private static void AdjustCosmeticCategoryButtons(Transform maskTransform, MenuElementCosmeticCategoryScroll holder, float ratioMod)
        {
            if (!maskTransform.TryGetComponent<RectMask2D>(out RectMask2D subCatMask2D))
                return;

            // this may be selfish of me, but it looks better with some negative padding so I've changed it here
            subCatMask2D.padding = new(-10f, subCatMask2D.padding.y, subCatMask2D.padding.z, subCatMask2D.padding.w);

            Spam("Attempting to fix customize menu category arrow buttons");
            Spam("Get World Corners:");
            Vector3[] corners = new Vector3[4];
            subCatMask2D.rectTransform.GetWorldCorners(corners);
            for (int i = 0; i < corners.Length; i++)
                Spam($"{corners[i]}");

            if (holder.buttonLeft.TryGetComponent<RectTransform>(out RectTransform leftBut))
            { 
                float xPos = corners[0].x;
                float yPos = leftBut.position.y;
                if (ratioMod < 1)
                    xPos -= ((leftBut.sizeDelta.x * ratioMod) + 5f); // ultrawide res position
                else
                {
                    xPos -= (leftBut.sizeDelta.x - 20f); // ultratall res position
                    yPos = corners[0].y;
                }

                Spam($"Left Corner Position of mask with resolution offset for button - {xPos}, {yPos}");

                // Set left button position to left of scroller mask using above calculation and a universal padding value of 5
                leftBut.position = new Vector3(xPos , yPos, leftBut.position.z);            
            }

            if (holder.buttonRight.TryGetComponent<RectTransform>(out RectTransform rightBut))
            {
                float xPos = corners[3].x;
                float yPos = rightBut.position.y;

                if (ratioMod > 1)
                    yPos = corners[3].y;
                
                Spam($"Right button placed at right corner of mask {xPos}, {yPos}");

                // set right button position directly at right of scroller mask (plus 5 for padding), no additional calculation needed
                rightBut.position = new Vector3(xPos, yPos, rightBut.position.z);
            }

            // fix ultratall divider position initial value
            if (ratioMod > 1f)
            {
                holder.menuPageCosmetics.topDividerRestingPositionStart = new(holder.menuPageCosmetics.topDividerRestingPositionStart.x, holder.menuPageCosmetics.topDividerRestingPositionStart.y / ratioMod);
                holder.menuPageCosmetics.topDividerRestingPositionCurrent = new(holder.menuPageCosmetics.topDividerRestingPositionCurrent.x, holder.menuPageCosmetics.topDividerRestingPositionCurrent.y / ratioMod);
                holder.menuPageCosmetics.topDividerRestingPositionEnd = new(holder.menuPageCosmetics.topDividerRestingPositionEnd.x, holder.menuPageCosmetics.topDividerRestingPositionEnd.y / ratioMod);
                holder.menuPageCosmetics.topDividerRestingPositionEndNew = new(holder.menuPageCosmetics.topDividerRestingPositionEndNew.x, holder.menuPageCosmetics.topDividerRestingPositionEndNew.y / ratioMod);
            }    
        }
    }

    [HarmonyPatch(typeof(MenuPageCosmetics), nameof(MenuPageCosmetics.RefreshScrollContent))]
    public class CustomizePageTallFix
    {
        public static void Postfix(MenuPageCosmetics __instance)
        {
            if (!FovConfig.AspectRatioFix.Value || !FovConfig.FullUIFixesEnabled())
                return;

            float ratioMod = GetRatioModifier();

            // native res and ultrawide does not need any changes
            if (ratioMod <= 1)
                return;

            // fix refresh height for ultratall
            __instance.topDividerRestingPositionEndNew = new (__instance.topDividerRestingPositionEnd.x, __instance.topDividerRestingPositionEndNew.y / ratioMod);
        }
    }

    [HarmonyPatch(typeof(MenuPage), nameof(MenuPage.Start))]
    public class MenuPagesFix
    {
        // ignore scaling on these objects in all menus
        private static readonly List<string> DontTouch = ["Background", "Panel Back", "Panel", "Bool Setting - "];

        public static void Prefix(MenuPage __instance)
        {
            if (!FovConfig.AspectRatioFix.Value || !FovConfig.FullUIFixesEnabled())
                return;

            float ratioMod = GetRatioModifier();

            // not needed at native res
            if (ratioMod == 1f)
                return;

            // password page gets kinda messed up if it's not excluded
            // to include this particular page in the future would prob require patching MenuPagePassword.Update (ie confirmButtonAnimationTransform)
            if (__instance.gameObject.GetComponent<MenuPagePassword>() != null)
                return;

            bool escMenu = __instance.gameObject.GetComponent<MenuPageEsc>() != null;
            bool lobbyMenu = __instance.gameObject.GetComponent<MenuPageLobby>() != null;

            Spam($"escMenu - {escMenu}, lobbyMenu - {lobbyMenu}");

            for (int i = 0; i < __instance.transform.childCount; i++)
            {
                // should adjust position bool, generally will always be true for ultrawide
                bool adjustPos = ratioMod != 1f;
                Transform child = __instance.transform.GetChild(i);

                if (!child.TryGetComponent<RectTransform>(out RectTransform rectTransform))
                    continue;

                // leave backgrounds, panels, etc. as-is (no resizing or repositioning)
                if (DontTouch.Contains(child.name))
                    continue;

                // dont reposition anything in esc menu
                if (escMenu)
                    adjustPos = false; //child.gameObject.name != "Header";

                // lobby menu should only move the gradient
                if (lobbyMenu)
                    adjustPos = child.name == "Gradient";

                // adjust scale and position
                AdjustToDifference(rectTransform, adjustPos);
            }
        }
    }

    [HarmonyPatch(typeof(RenderTextureMain), nameof(RenderTextureMain.Start))]
    public class RenderTextureMainOverrides
    {
        public static void Postfix(RenderTextureMain __instance)
        {
            // this patch is specifically for ultrawide support
            if (!FovConfig.AspectRatioFix.Value)
                return;

            // replacing the below to scale based on the monitor's actual resolution rather than 1080p
            __instance.textureWidthLarge = (float)Screen.width * 0.78f; // originally 1500 which is 78% of 1920
            __instance.textureWidthMedium = (float)Screen.width * 0.58f; // originally 1125 which is 58% of 1920 (20% decrease)
            __instance.textureWidthSmall = (float)Screen.width * 0.39f; // originally 750 which is 39% of 1920 (19% decrease)

            __instance.textureHeightLarge = (float)Screen.height * 0.77f; // originally 836 which is 77.4% of 1080
            __instance.textureHeightMedium = (float)Screen.height * 0.58f; // originally 627 which is 58% of 1080
            __instance.textureHeightSmall = (float)Screen.height * 0.38f; // originally 418 which is 38% of 1080

            // update to our values
            GraphicsManager.instance.UpdateRenderSize();
        }
    }

    [HarmonyPatch(typeof(GraphicsManager), nameof(GraphicsManager.UpdateRenderSize))]
    public class CatchNoPixelationSetting
    {
        // This will skip the vanilla method if our custom pixelation setting "None" is chosen
        public static bool Prefix()
        {
            int setting = DataDirector.instance.SettingValueFetch(DataDirector.Setting.RenderSize);
            if (setting != 99)
            {
                Spam($"Non-controlled pixelation setting set! ({setting})");
                return true;
            }

            Spam($"Controlled pixelation setting being set! ({setting})");
            UseNonePixelation();
            return false;
        }

        internal static void UseNonePixelation()
        {
            Spam("Setting pixelation to None!");
            RenderTextureMain.instance.textureWidthOriginal = (float)Screen.width; // set to monitor resolution
            RenderTextureMain.instance.textureHeightOriginal = (float)Screen.height; // set to monitor resolution
            RenderTextureMain.instance.ResetResolution();
        }
    }

    [HarmonyPatch(typeof(MenuSlider), nameof(MenuSlider.Awake))]
    public class InsertPixelationSetting
    {
        // Selection Event
        private static readonly UnityEvent Nopixel = new();
        // Pixelation Menu Slider (refreshes every time the menu page is opened)
        private static MenuSlider? PixelSlider { get; set; } = null!;
        // The option this mod adds to the slider, will be created only if it's null
        private static MenuSlider.CustomOption? NoneOption { get; set; } = null!;
        public static void Prefix(MenuSlider __instance)
        {
            if (__instance.elementName != "Pixelation")
                return;

            PixelSlider = __instance;
            // enable wrap around for the setting because it's nice
            PixelSlider.wrapAround = true;
            // create the option, localization is optional so we set it null
            NoneOption ??= new MenuSlider.CustomOption
                {
                    customOptionText = "None",
                    localizedCustomOptionText = null,
                    onOption = Nopixel
                };
            // set our unique value for this setting in case others insert another option in the future
            NoneOption.customValueInt = 99;

            // stop here, slider already has our option
            if (PixelSlider.customOptions.Contains(NoneOption))
                return;

            // remove listeners because we're going to readd our listener here
            Nopixel.RemoveAllListeners();

            // this basically emulates the vanilla method Menuslider.CustomOptionAdd
            // not using vanilla method because we need to keep track of the option and it returns void
            PixelSlider.customOptions.Add(NoneOption);
            PixelSlider.settingSegments = Mathf.Max(PixelSlider.customOptions.Count - 1, 1);
            PixelSlider.startValue = 0;
            PixelSlider.endValue = PixelSlider.customOptions.Count - 1;

            // add our action for the setting
            Nopixel.AddListener(SetNone);

            // set bar percentage (float) based on option number using the saved setting value
            PixelSlider.SetBar(PixelSlider.GetIndexFromCustomValue(DataDirector.instance.SettingValueFetch(DataDirector.Setting.RenderSize) / PixelSlider.endValue));
            Spam("Inserted 'none' Pixelation setting!");
        }

        private static void SetNone()
        {
            Log.LogMessage("Pixelation set to [NONE] by FovUpdate");
            GraphicsManager.instance.UpdateRenderSize();
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

        internal static ScreenStatus ScreenIs = ScreenStatus.Default;

        internal static float previousAspectRatio;
        internal static float currentAspectRatio;
        public static readonly float defaultAspectRatio = 1.7777778f;
        internal static List<RectTransform> Rects = [];
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
                if(!FovConfig.MinimalUIFixesEnabled())
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
            if (!AreWeInGame() || !__instance.photonView.IsMine)
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
            if (!AreWeInGame() || !FovConfig.OnScreenFix.Value)
                return;

            //get fov scale by dividing our defined fov by the vanilla 70
            //this is different than our other fovscale calculations because the padding needs to be smaller for higher fovs
            float fovScale = FovConfig.UserDefinedFov.Value / 70f;

            paddHeight /= fovScale;
            paddWidth /= fovScale;
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

            GameObject overrideObj;

            if (obj == null)
                overrideObj = PlayerController.instance.playerAvatar.gameObject;
            else
                overrideObj = obj;

            changingFov = true; //moved to before overridezoomset function for other patches
            CameraZoom.Instance.zoomPrev = CameraZoom.Instance.zoomCurrent;
            CameraZoom.Instance.OverrideZoomSet(newFov, 2f, 1f, 1f, obj, 150);

            while (Mathf.Abs(CameraZoom.Instance.zoomCurrent - newFov) > 0.25 && CameraZoom.Instance.OverrideZoomObject == overrideObj)
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
