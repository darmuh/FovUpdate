# Change Log

All notable changes to this project will be documented in this file.
 
The format is based on [Keep a Changelog](http://keepachangelog.com/).

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