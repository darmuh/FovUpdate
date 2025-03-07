# Change Log

All notable changes to this project will be documented in this file.
 
The format is based on [Keep a Changelog](http://keepachangelog.com/).

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