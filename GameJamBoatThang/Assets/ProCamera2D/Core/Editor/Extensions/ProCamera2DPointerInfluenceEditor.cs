using UnityEditor;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [CustomEditor(typeof(ProCamera2DPointerInfluence))]
    public class ProCamera2DPointerInfluenceEditor : Editor
    {
        void OnEnable()
        {
            var proCamera2DPointerInfluence = (ProCamera2DPointerInfluence)target;

            if(proCamera2DPointerInfluence.ProCamera2D == null && Camera.main != null)
                proCamera2DPointerInfluence.ProCamera2D = Camera.main.GetComponent<ProCamera2D>();
        }

        public override void OnInspectorGUI()
        {
            var proCamera2DPointerInfluence = (ProCamera2DPointerInfluence)target;
            
            if(proCamera2DPointerInfluence.ProCamera2D == null)
                EditorGUILayout.HelpBox("ProCamera2D is not set.", MessageType.Error, true);
                
            DrawDefaultInspector();
        }
    }
}