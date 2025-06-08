# FovUpdate (And More)

## 0.3.1 has been compiled for v0.1.2.42_beta 
*If you are not playing the beta version of the game, you may need to downgrade to previous versions.*  

### This is a simple mod that allows you to update the game's FOV to your desired setting. 

### Also provides support for upscaling/downscaling the game as well as support for UltraWide or UltraLong screens (if enabled)

**This is a client sided mod and only the person who is adjusting their fov will need it.**  

- For basic instructions on how to use this mod, please follow the wiki article [here](https://thunderstore.io/c/repo/p/darmuh/FovUpdate/wiki/3169-how-do-i-change-the-fov/)
- Set both your base-game field of view and your "crouched" / "tumble" field of view via the config.
- Set your map item field of view in the config as well.
- While in-game, you can also use the following chat commands:
	- ``\fov <number>`` input any number after ``\fov`` to immediately update your fov (and the config item)
	- ``\cfov <number>`` input any number after ``\cfov`` to immediately update your crouch fov (and the config item)
	- ``\sfov <number>`` input any number after ``\sfov`` to immediately update your sprinting fov modifier (and the config item)
	- FYI, everyone in the lobby **will** hear your chat commands
- Tested briefly in multiplayer, please report any issues to the github for this mod.
- ``AspectRatioFix`` config item for those with Ultrawide or Ultralong monitors.
	- This fix is originally from [Oksamies' UltrawideOrLongFix](https://thunderstore.io/c/repo/p/Oksamies/UltrawideOrLongFix/) and has been slightly modified.
	- This setting will ensure your camera updates to the correct aspect ratio (unstretched horizontally)
	- Don't want your UI stretched? Try the ``Dont Stretch UI`` config item out!
- Config change event support has been added for any mod that allows for config changes in-game (i.e. REPOConfig)
- Upscale or downscale your game with the ``Resolution Multiplier`` config item.
	- NOTE: This config item overrides the ``Pixelation`` graphics setting when set to a value other than 1.
	- NOTE2: Unity has a maximum texture size of 16384. If either aspect of your screen's resolution mutliplied by the  multiplier is larger than this value, the config item will be forced to an acceptable value.

### Fixes added in 0.3.0:  
- ``Maximum FOV fix on/off``: This setting prevents the fov from ever being higher than the number set in ``Maximum Possible Fov``  
	- Fixes the potential for unplayable fovs after numerous speed upgrade  
- ``OnScreen fix on/off``: This setting updates the OnScreen method calculation to account for your prefered FOV when enabled.  
	- Should fix some of the more exagerated examples of being able to stare at enemies like the "Shadow Child" in the corner of your screen without triggering the looked at effect by using a high fov  
- ``Effects fix on/off``: This setting adjusts the strength of fov effects based on your base fov.  
	- So if you have a higher fov than standard, the amount you zoom in from enemies or items will be lessened to match a similar zoom distance as with the original default fov.  


### Example Screenshots:
<details>
<summary>Icon Original Image</summary>

![Original Icon Image](https://github.com/darmuh/FovUpdate/blob/master/Screenshots/iconog.jpg?raw=true)

</details>

<details>
<summary>Example 1</summary>

![Example 1](https://github.com/darmuh/FovUpdate/blob/master/Screenshots/example1.jpg?raw=true)

</details>

<details>
<summary>Example 2</summary>

![Example 2](https://github.com/darmuh/FovUpdate/blob/master/Screenshots/example2.jpg?raw=true)

</details>

<details>
<summary>Example 3</summary>

![Example 3](https://github.com/darmuh/FovUpdate/blob/master/Screenshots/example3.jpg?raw=true)

</details>

<details>
<summary>Example 4</summary>

![Example 4](https://github.com/darmuh/FovUpdate/blob/master/Screenshots/example4.jpg?raw=true)

</details>

<details>
<summary>Example 5</summary>

![Example 5](https://github.com/darmuh/FovUpdate/blob/master/Screenshots/example5.jpg?raw=true)

</details>

<details>
<summary>Example 6</summary>

![Example 6](https://github.com/darmuh/FovUpdate/blob/master/Screenshots/example6.jpg?raw=true)

</details>

<details>
<summary>Example 7</summary>

![Example 7](https://github.com/darmuh/FovUpdate/blob/master/Screenshots/example7.jpg?raw=true)

</details>


### Last update to this page:
May 19th, 2025 (version 0.3.0)  
