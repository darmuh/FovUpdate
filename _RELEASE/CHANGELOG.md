# Change Log

All notable changes to this project will be documented in this file.
 
The format is based on [Keep a Changelog](http://keepachangelog.com/).

## [0.3.2]
- Small code cleanup in patching.
- Switched from CameraAim to player spawn patch to update camera fov
- Switched to OnSceneSwitch instead of CameraAim spawn for stretch fix
- Compiled for latest public version of the game

## [0.3.1]
- Added ability to set specific field of view setting for the map.
	- Set via the MapFov config item, leave as default to allow for effectsfix to adjust this item's field of view automatically.
- Added ability to not stretch the UI when using the ultrawide fix.
	- This is not the best looking and not recommended by default
- Compiled for latest beta version, v0.1.2.42_beta 

## [0.3.0]
- Removed fix added in 0.2.11 for [issue #6](https://github.com/darmuh/FovUpdate/issues/6) in favor of new transpiler patch.  
- Added transpiler for CameraZoom Update to clamp the resulting fov and hopefully resolve [issue #6](https://github.com/darmuh/FovUpdate/issues/6) for good.  
- Added prefix patch for OverrideZoomSet to adjust the zoom factor to scale with your new fov.  
	- If you have a larger fov than 70, the amount you zoom will be lessened to be more in-line with the vanilla behavior.  
	- And if you have a fov smaller than 70 set, the amount you zoom will be bigger.  
- Added Prefix patch for SemiFunc.OnScreen to account for your fov difference.  
	- This method is used by base-game for basically anything that happens when it appears on your screen.  
	- The most notable example is the Shadow Child enemy's interactions. They should now react more often to you having them in your peripheral even with a higher fov.  
- Added new config items associated to the new game patches so you can disable them if you don't want their fixes.

## [0.2.11]
 - Found and added fix for issue reported in [issue #6](https://github.com/darmuh/FovUpdate/issues/6) where numerous speed upgrades would result in an unplayable Field of View while sprinting.
	- This is a base-game issue that is more apparent when playing with a larger fov
	- The current fix is that I recalculate your sprint fov modifier to account for the number of upgrades you have if the expected peak FOV will be larger than 180
	- I have [reported this issue as a bug to the actual devs of the game](https://discord.com/channels/1330873443515760640/1373418458552860753) so that they can add a fix on their end some day. It would be a lot simpler/easier to fix on their end.
	- Thanks ud119 on github for helping me identify the root cause of this issue
 - Compiled for v0.1.2.31_beta version of the game

## [0.2.10]
 - Compiled for beta
 - Added GetCappedSprintFov method to cap sprintfov values. 
	- This method will update your sprint fov if it causes your fov to exceed 180 when sprinting.
	- Reminder that sprint fov is an added on value on top of your regular fov. So having a regular fov of 100 and a sprint fov of 90 would have raised your fov to 190 once you hit the peak of your sprint. This cap will now automatically lower your sprint fov so that your peak fov will not be larger than 180.
	- If anyone would like the hard cap raised from 180 please feel free to reach out via a github issue or the modding discord.
 - Changed spawn patch to CameraAim Spawn

## [0.2.9]
 - Adjusted github issue #5 fix to hopefully account for some minor math errors.
	- Logs will now show what your resolution AND modifier have been forced to when exceeding the limit.
	- I also switched from hardcoding 16834 to using SystemInfo.maxTextureSize

## [0.2.8]
 - Added fix for github issue #5, where the resolution multiplier would result in a resolution that is larger than unity's maximum supported texture size.
	- The fix will now force you to a lower, valid resolution multiplier.
	- This fix is untested since I don't have a monitor with a large enough resolution to experience the issue.

## [0.2.7]
 - Hopefully fixed ``Resolution Multiplier`` patch not applying on reload.
	- Also added a portion of the patch to use the game's pixelation setting when returning to default value

## [0.2.6]
 - Slight adjustment to ``Resolution Multiplier`` patch to allow for setting back to default value of 1 when launching with a different value
 - Latest version of REPOConfig also handles the values much better now. Allowing for precise resolution multiplier selections! (Thanks Nick)

## [0.2.5]
 - Added ``Resolution Multiplier`` config item to allow for upscaling/downscaling the game.
	- Max downscale is a 0.25 multiplier whereas max upscale is a 4 times multiplier.
	- This setting overrides the ``Pixelation`` graphics setting in base game when set to a value other than 1.
	- NOTE: REPOConfig does not read these values all that well. You will want to edit this config item out of game if you want to use a value other than 1,2,3, or 4.

## [0.2.4]
 - Added aspect ratio fix when ``AspectRatioFix`` is enabled and the resolution is not standard.
	- This will update the aspect ratio of the camera to your current resolution.
 - Minor updates to camera list handling
	- moved list to plugin class and added UpdateCams method

## [0.2.3]
 - Added config change event support now that REPOConfig is in a good spot and offers config changing in-game.
 - Added comments to ultrawide support patch to explain logic of code

## [0.2.2]
 - Fixed ``/sfov`` command updating crouchfov config item instead of sprintfov config item
 - Added ``AspectRatioFix`` config item to support ultra-wide and other non-conventional monitors.
	- Yoinked a patch from [Oksamies' UltrawideOrLongFix](https://thunderstore.io/c/repo/p/Oksamies/UltrawideOrLongFix/) (with their permission ofc)
	- This config item is disabled by default, you will need to enable it once the config item is generated (after game launch)
	- I tried to update the patch to cache things where I could to prevent some performance loss due to continuous running code

## [0.2.1]
 - Adjusted some patching around. Now fov change should most often be done via the spawn patch.
	- I'm still unable to replicate the issue I was seeing in multiplayer where my fov would reset, but i'm hoping this shuffling of logic might have fixed the rare bug.
 - Added SprintFov setting and related chat command ``/sfov``
	- This will allow you to modify the fov modifier that sprint adds.
	- Vanilla value is 20, however you can add more to exaggerate the effect or set it to 0 to remove it.

## [0.2.0]
 - Added chat commands ``/fov`` and ``/cfov`` to change fov/crouch fov in-game
 - Added crouch fov config item ``CrouchFov`` for when the player is crouched (tumble mode)
 - Hopefully fixed issue of fov not changing on respawn by adding a spawn patch

## [0.1.0]
 - Initial Release.