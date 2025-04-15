# Change Log

All notable changes to this project will be documented in this file.
 
The format is based on [Keep a Changelog](http://keepachangelog.com/).

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