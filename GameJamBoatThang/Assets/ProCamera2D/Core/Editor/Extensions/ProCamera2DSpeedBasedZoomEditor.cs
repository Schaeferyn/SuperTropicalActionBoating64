using UnityEditor;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [CustomEditor(typeof(ProCamera2DSpeedBasedZoom))]
    public class ProCamera2DSpeedBasedZoomEditor : Editor
    {
        void OnEnable()
        {
            var proCamera2DSpeedBasedZoom = (ProCamera2DSpeedBasedZoom)target;

            if(proCamera2DSpeedBasedZoom.ProCamera2D == null && Camera.main != null)
                proCamera2DSpeedBasedZoom.ProCamera2D = Camera.main.GetComponent<ProCamera2D>();
        }

        public override void OnInspectorGUI()
        {
            var proCamera2DSpeedBasedZoom = (ProCamera2DSpeedBasedZoom)target;

            if(proCamera2DSpeedBasedZoom.ProCamera2D == null)
                EditorGUILayout.HelpBox("ProCamera2D is not set.", MessageType.Error, true);

            DrawDefaultInspector();

            if (proCamera2DSpeedBasedZoom.ZoomInSmoothness < .5f)
                proCamera2DSpeedBasedZoom.ZoomInSmoothness = .5f;

            if (proCamera2DSpeedBasedZoom.ZoomOutSmoothness < .5f)
                proCamera2DSpeedBasedZoom.ZoomOutSmoothness = .5f;

            if (proCamera2DSpeedBasedZoom.MaxZoomInAmount < 1f)
                proCamera2DSpeedBasedZoom.MaxZoomInAmount = 1f;

            if (proCamera2DSpeedBasedZoom.MaxZoomOutAmount < 1f)
                proCamera2DSpeedBasedZoom.MaxZoomOutAmount = 1f;

            if (proCamera2DSpeedBasedZoom.SpeedForZoomOut <= proCamera2DSpeedBasedZoom.SpeedForZoomIn)
                proCamera2DSpeedBasedZoom.SpeedForZoomOut = proCamera2DSpeedBasedZoom.SpeedForZoomIn + .1f;

            if (proCamera2DSpeedBasedZoom.SpeedForZoomIn >= proCamera2DSpeedBasedZoom.SpeedForZoomOut)
                proCamera2DSpeedBasedZoom.SpeedForZoomIn = proCamera2DSpeedBasedZoom.SpeedForZoomOut - .1f;

            if (proCamera2DSpeedBasedZoom.SpeedForZoomIn < .5f)
                proCamera2DSpeedBasedZoom.SpeedForZoomIn = .5f;
        }
    }
}