# FovUpdate (And More)

### Simple client-sided mod that allows you to update the game's FOV to your desired setting. 

### Also provides UltraWide or UltraLong screens support (if enabled)

*Please report any issues found to the linked github distro for this mod.*  

- For basic instructions on how to use this mod, please follow the wiki article [here](https://thunderstore.io/c/repo/p/darmuh/FovUpdate/wiki/3169-how-do-i-change-the-fov/)
- Set both your base-game field of view and your "crouched" / "tumble" field of view via the config.
- Set your map item field of view in the config as well.
- While in-game, you can also use the following chat commands:
	- ``\fov <number>`` input any number after ``\fov`` to immediately update your fov (and the config item)
	- ``\cfov <number>`` input any number after ``\cfov`` to immediately update your crouch fov (and the config item)
	- ``\sfov <number>`` input any number after ``\sfov`` to immediately update your sprinting fov modifier (and the config item)
	- FYI, everyone in the lobby **will** hear your chat commands
- ``AspectRatioFix`` config item for those with Ultrawide or Ultralong monitors.
	- This fix is originally from [Oksamies' UltrawideOrLongFix](https://thunderstore.io/c/repo/p/Oksamies/UltrawideOrLongFix/) and has been slightly modified.
	- This setting will ensure your camera updates to the correct aspect ratio (unstretched horizontally)
	- See "UI Adjustments Configurable Setting" section below for more information on related ``UI Adjustments`` config item.
- Config change event support has been added for any mod that allows for config changes in-game (i.e. REPOConfig)
- Remove pixelation completely with the added ``None`` pixelation setting. (Settings > Graphics > Pixelation)

### FOV-Related Configurable Fixes:  
- ``Maximum FOV fix on/off``: This setting prevents the fov from ever being higher than the number set in ``Maximum Possible Fov``  
	- Fixes the potential for unplayable fovs after numerous speed upgrade  
- ``OnScreen fix on/off``: This setting updates the OnScreen method calculation to account for your prefered FOV when enabled.  
	- Should fix some of the more exagerated examples of being able to stare at enemies like the "Shadow Child" in the corner of your screen without triggering the looked at effect by using a high fov  
- ``Effects fix on/off``: This setting adjusts the strength of fov effects based on your base fov.  
	- So if you have a higher fov than standard, the amount you zoom in from enemies or items will be lessened to match a similar zoom distance as with the original default fov.  

### UI Adjustments Configurable Setting:
- This setting relates to UI specific fixes for Ultrawide resolutions. 
- It does nothing if ``AspectRatioFix`` is disabled, or if you are playing at a standard 16:9 resolution.
- ``EnableAllFixes``: This setting value will enable all of the best effort fixes this mod has to de-stretch and reposition UI elements.
- ``MinimalFixes``: This setting value will enable only the bare minimum of UI fixes to make the game playable on non-standard resolutions.
- ``NoFixes``: This setting value will ensure this mod does not touch anything in the UI.

### Example Screenshots:
<details>
<summary>FOV Screenshots</summary>

| | | | |
|:---:|:---:|:---:|:---:|
| ![Original Icon Image](https://github.com/darmuh/FovUpdate/blob/master/FOV%20Screenshots/iconog.jpg?raw=true) | ![Example 1](https://github.com/darmuh/FovUpdate/blob/master/FOV%20Screenshots/example1.jpg?raw=true) | ![Example 2](https://github.com/darmuh/FovUpdate/blob/master/FOV%20Screenshots/example2.jpg?raw=true) | ![Example 3](https://github.com/darmuh/FovUpdate/blob/master/FOV%20Screenshots/example3.jpg?raw=true) |
| ![Example 4](https://github.com/darmuh/FovUpdate/blob/master/FOV%20Screenshots/example4.jpg?raw=true) | ![Example 5](https://github.com/darmuh/FovUpdate/blob/master/FOV%20Screenshots/example5.jpg?raw=true) | ![Example 6](https://github.com/darmuh/FovUpdate/blob/master/FOV%20Screenshots/example6.jpg?raw=true) | ![Example 7](https://github.com/darmuh/FovUpdate/blob/master/FOV%20Screenshots/example7.jpg?raw=true) |

</details>

<details>
<summary>Ultrawide Comparison Screenshots</summary>

### Main Menu

| EnableAllFixes |  MinimalFixes | NoFixes |
|:---:|:---:|:---:|
| ![main_full](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/main_full.jpg?raw=true)  |  ![main_min](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/main_min.jpg?raw=true) | ![main_none](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/main_none.jpg?raw=true) |

### Lobby Menu

| EnableAllFixes |  MinimalFixes | NoFixes |
|:---:|:---:|:---:|
| ![lobby_full](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/lobby_full.jpg?raw=true)  |  ![lobby_min](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/lobby_min.jpg?raw=true) | ![lobby_none](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/lobby_none.jpg?raw=true) |

### In-Game UI

| EnableAllFixes |  MinimalFixes | NoFixes |
|:---:|:---:|:---:|
| ![item_full](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/item_full.jpg?raw=true)  |  ![item_min](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/item_min.jpg?raw=true) | ![item_none](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/item_none.jpg?raw=true) |

### In-Game Escape Menu

| EnableAllFixes |  MinimalFixes | NoFixes |
|:---:|:---:|:---:|
| ![esc_full](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/esc_full.jpg?raw=true)  |  ![esc_min](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/esc_min.jpg?raw=true) | ![esc_none](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/esc_none.jpg?raw=true) |

### Customization Menu

| EnableAllFixes |  MinimalFixes | NoFixes |
|:---:|:---:|:---:|
| ![custom_full](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/custom_full.jpg?raw=true)  |  ![custom_min](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/custom_min.jpg?raw=true) | ![custom_none](https://github.com/darmuh/FovUpdate/blob/master/Ultrawide%20Screenshots/custom_none.jpg?raw=true) |

</details>


### Last update to this page:
August 17th, 2026 (version 0.4.0)  
