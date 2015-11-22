## UPGRADE GUIDE ##

To upgrade to a new ProCamera2D version you should:
- Open a new empty scene
- Delete ProCamera2D folders (Assets/ProCamera2D and Assets/Gizmos/ProCamera2D)
- Import the new package




## QUICK START ##

Select your game camera (called Main Camera by default) and drag the ProCamera2D component file to the Main Camera GameObject.
You should now see the ProCamera2D component editor on the Inspector window when you select your Main Camera GameObject.




## USER GUIDE ##

For more information about all ProCamera2D features and how to use them please visit our User Guide at: 
http://www.procamera2d.com/user-guide




## SUPPORT ##

Do you have an issue you want to see resolved, a suggestion or you simply want to know if this is the right plugin for your game? Get in touch using any of the links below and we’ll do our best to get back to you ASAP.

Contact Form - http://www.procamera2d.com/support
Unity forums - http://goo.gl/n80dMb
Sub-reddit - https://www.reddit.com/r/procamera2d/
Twitter - http://www.twitter.com/lpfonseca




## CHANGELOG ##

1.6.2
- Fixed a bug that would show the incorrect triggers gizmos position if using a Circle
- Only show the Zoom trigger preview size when selected
- On the Zoom and Influence triggers, when not using the targets mid point, use the provided target to calculate the distance to the center percentage
- Zoom trigger "Reset Size On Exit" is now "Reset Size On Leave" so it resets progressively as you leave the trigger instead of only once you exit
- Fixed a bug where after deleting boundaries related extensions / triggers the camera could get stuck at (0,0)

1.6.1
- The camera is only parented if using the Shake extension
- Fixed an issue where deleting Cinematic camera targets could throw an (harmless) error to the console
- When using the Cinematics extension, if a target has a Hold Duration less than 0, the camera will follow it indefinitely
- Added a method (GoToNextTarget) to the Cinematics extension that allows you to skip the current target
- Prevent the Game Viewport size on the Pixel Perfect extension to go below (1, 1)
- Fixed a bug where the TriggerBoundaries would have a missing reference if instantiated manually

1.6.0
- Moved Camera Window, Numeric Boundaries and Geometry Boundaries from the core into their own extensions, leaving the core as light as possible
- Added a new powerful extension (Cinematics) that replaces the CinematicFocusTarget
- Added a new demo (TopDownShooter) that shows how to use multiple ProCamera2D features in one simple game
- Tweaked Zoom-To-Fit extension to support camera boundaries
- Tweaked Speed-Based-Zoom extension to support camera boundaries
- Forward focus auto-adjusts to the camera size when zooming
- Gizmos for triggers are now shown as circles instead of spheres
- Added the option for triggers to be triggered by a specific GameObject instead of always using the camera targets mid-point
- Renamed Plugins to Extensions and Helpers to Triggers

1.5.3
- Upgraded Shake extension - Presets support, rotation and overall tweaks
- Zooming with perspective cameras is now made by moving the camera instead of changing the field of view to avoid distortion
- Fix TriggerBoundaries left and right gizmos incorrect size on XZ and YZ axis
- Maintain pixel-perfect during shakes

1.5.2
- Fixed bug when applying influences on XZ and YZ axis

v1.5.1
- Added option to reset size when exiting a zoom trigger
- Added option to disable Zoom-To-Fit plugin when there's only one target
- Disable Parallax plugin toggle button when in perspective mode
- Fixed a bug when adding targets progressively
- Tweaked the CinematicFocusTrigger to take all influences in consideration when returning from the cinematic position
- Added an optional "duration" parameter to the RemoveCameraTarget method that allows to remove a camera target progressively by reducing it's influence over time
- Added an optional “duration” parameter to the UpdateScreenSize method that allows to manually update the screen size progressively

v1.5.0
- Added a new plugin, Speed Based Zoom that adjusts the zoom based on how fast the camera is moving.

v1.4.1
- Fixed a bug where if the camera is a prefab it would loose its targets on scene load, if the changes weren't saved

v1.4
- Pixel perfect support!! :)
- Fixed a few more bugs related to setting the Time.timeScale to 0 (Thanks to the users who reported this issue and helped solving it!)
- Added a user-guide link to the top of ProCamera2D editor for easier access to the documentation

v1.3.1
- Added a ParallaxObject MonoBehaviour that makes a GameObject position on the scene view to match the same relative position to the main parallax layer during runtime.
- Fixed slight camera movement on start when no targets are added
- Fixed a bug where if Time.timeScale was put to 0 the camera would stop following its targets afterwards
- Fixed a few Playmaker actions descriptions
- Fixed target vertical offset when on XZ and YZ axis
- Fixed PointerInfluence plugin on XZ and YZ axes

v1.3
- Full compatibility with 2DToolkit!
- Added namespace to UpdateType and MovementAxis enums to avoid conflicts with other packages
- Added the option to set the TriggerZoom helper size as a direct value instead of as a multiplier
- Added a new method to stop shaking and a flag to check if the camera is currently shaking
- Fixed bug with InfluenceTrigger not smoothing correctly the value on first entrance if ExclusiveInfluencePercentage is 1

v1.2
- Added support for perspective cameras
- Fixed bug with camera getting stuck when using Camera Window and Numeric Boundaries
- Fixed bug that made camera float away when using an offset on a specific axis and turned off following on that same axis

v1.1
- Custom PlayMaker actions with full API support
- Fix for AdjustCameraTargetInfluence method when starting at values different than zero

v1.0
- Public release