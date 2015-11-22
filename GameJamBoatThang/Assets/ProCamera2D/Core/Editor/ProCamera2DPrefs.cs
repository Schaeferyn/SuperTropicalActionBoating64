using UnityEditor;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public static class ProCamera2DPrefs
    {	
    	static bool _prefsLoaded;

        static Color[] _procamera2DGizmosColors;
        readonly static string[] _procamera2DGizmosKeys = new string[]
        {
        	PrefsData.NumericBoundariesColorKey, 
        	PrefsData.TargetsMidPointColorKey,
        	PrefsData.InfluencesColorKey,
        	PrefsData.ShakeInfluenceColorKey,
        	PrefsData.OverallOffsetColorKey,
        	PrefsData.CamDistanceColorKey,
        	PrefsData.CamTargetPositionColorKey,
        	PrefsData.CamTargetPositionSmoothedColorKey,
        	PrefsData.CurrentCameraPositionColorKey,
        	PrefsData.CameraWindowColorKey,

        	PrefsData.ForwardFocusColorKey,

			PrefsData.ZoomToFitColorKey,

            PrefsData.BoundariesTriggerColorKey,

            PrefsData.InfluenceTriggerColorKey,

            PrefsData.ZoomTriggerColorKey,

            PrefsData.TriggerShapeColorKey,
        };
        readonly static Color[] _procamera2DGizmosValues = new Color[]
        {
        	PrefsData.NumericBoundariesColorValue, 
        	PrefsData.TargetsMidPointColorValue,
        	PrefsData.InfluencesColorValue,
        	PrefsData.ShakeInfluenceColorValue,
        	PrefsData.OverallOffsetColorValue,
        	PrefsData.CamDistanceColorValue,
        	PrefsData.CamTargetPositionColorValue,
        	PrefsData.CamTargetPositionSmoothedColorValue,
        	PrefsData.CurrentCameraPositionColorValue,
        	PrefsData.CameraWindowColorValue,

        	PrefsData.ForwardFocusColorValue,

			PrefsData.ZoomToFitColorValue,        	

            PrefsData.BoundariesTriggerColorValue,

            PrefsData.InfluenceTriggerColorValue,

            PrefsData.ZoomTriggerColorValue,

            PrefsData.TriggerShapeColorValue,
        };

        static Vector2 _scrollPos;
	
        [PreferenceItem("ProCamera2D")]
        static void PreferencesGUI()
        {
            // Load the preferences
            if (!_prefsLoaded)
            {
                _procamera2DGizmosColors = new Color[_procamera2DGizmosKeys.Length];
                for (int i = 0; i < _procamera2DGizmosColors.Length; i++) 
                {
                	_procamera2DGizmosColors[i] = EditorPrefsX.GetColor(_procamera2DGizmosKeys[i], _procamera2DGizmosValues[i]);
                }

                _prefsLoaded = true;
            }
		
            // Preferences GUI
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("ProCamera2D", EditorStyles.boldLabel);
            for (int i = 0; i < _procamera2DGizmosColors.Length; i++) 
            {
            	_procamera2DGizmosColors[i] = EditorGUILayout.ColorField(_procamera2DGizmosKeys[i], _procamera2DGizmosColors[i]);

            	if(i == 9)
            	{
            		EditorGUILayout.Space();
            		GUILayout.Label("Plugins", EditorStyles.boldLabel);
            	}

                if(i == 11)
                {
                    EditorGUILayout.Space();
                    GUILayout.Label("Helpers", EditorStyles.boldLabel);
                }
            }

            // Reset defaults
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Use defaults", GUILayout.Width(120)))
            {
                for (int i = 0; i < _procamera2DGizmosColors.Length; i++) 
	            {
	            	EditorPrefsX.SetColor(_procamera2DGizmosKeys[i], _procamera2DGizmosValues[i]);
	            }
            }

            EditorGUILayout.EndScrollView();
		
            // Save the preferences
            if (GUI.changed)
            {
                for (int i = 0; i < _procamera2DGizmosColors.Length; i++) 
		        {
		        	EditorPrefsX.SetColor(_procamera2DGizmosKeys[i], _procamera2DGizmosColors[i]);
		        }
            }
        }
    }
}