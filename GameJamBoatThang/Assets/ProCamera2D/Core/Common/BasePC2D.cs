using System;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    abstract public class BasePC2D : MonoBehaviour
    {
        public ProCamera2D ProCamera2D;

        protected Func<Vector3, float> Vector3H;
        protected Func<Vector3, float> Vector3V;
        protected Func<Vector3, float> Vector3D;
        protected Func<float, float, Vector3> VectorHV;
        protected Func<float, float, float, Vector3> VectorHVD;

        protected Transform _transform;

        protected bool _gizmosDrawingFailed;

        protected virtual void Start()
        {
            _transform = transform;

            if (ProCamera2D == null && Camera.main != null)
                ProCamera2D = Camera.main.GetComponent<ProCamera2D>();
            else if(ProCamera2D == null)
                ProCamera2D = FindObjectOfType(typeof(ProCamera2D)) as ProCamera2D;
            
            if (ProCamera2D == null)
            {
                Debug.LogError(GetType().Name + ": ProCamera2D not set and not found on the MainCamera, or no camera with the MainCamera tag assigned.");
                return;
            }

            switch (ProCamera2D.Axis)
            {
                case MovementAxis.XY:
                    Vector3H = vector => vector.x;
                    Vector3V = vector => vector.y;
                    Vector3D = vector => vector.z;
                    VectorHV = (h, v) => new Vector3(h, v, 0);
                    VectorHVD = (h, v, d) => new Vector3(h, v, d);
                    break;
                case MovementAxis.XZ:
                    Vector3H = vector => vector.x;
                    Vector3V = vector => vector.z;
                    Vector3D = vector => vector.y;
                    VectorHV = (h, v) => new Vector3(h, 0, v);
                    VectorHVD = (h, v, d) => new Vector3(h, d, v);
                    break;
                case MovementAxis.YZ:
                    Vector3H = vector => vector.z;
                    Vector3V = vector => vector.y;
                    Vector3D = vector => vector.x;
                    VectorHV = (h, v) => new Vector3(0, v, h);
                    VectorHVD = (h, v, d) => new Vector3(d, v, h);
                    break;
            }
        }

        #if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (ProCamera2D == null && Camera.main != null)
                ProCamera2D = Camera.main.GetComponent<ProCamera2D>();

            _gizmosDrawingFailed = false;
            
            if (ProCamera2D == null)
            {
                _gizmosDrawingFailed = true;
                return;
            }

            // Don't draw gizmos on other cameras
            if (Camera.current != ProCamera2D.GameCamera &&
                ((UnityEditor.SceneView.lastActiveSceneView != null && Camera.current != UnityEditor.SceneView.lastActiveSceneView.camera) ||
                (UnityEditor.SceneView.lastActiveSceneView == null)))
            {
                _gizmosDrawingFailed = true;
                return;
            }

            switch (ProCamera2D.Axis)
            {
                case MovementAxis.XY:
                    Vector3H = vector => vector.x;
                    Vector3V = vector => vector.y;
                    Vector3D = vector => vector.z;
                    VectorHV = (h, v) => new Vector3(h, v, 0);
                    VectorHVD = (h, v, d) => new Vector3(h, v, d);
                    break;
                case MovementAxis.XZ:
                    Vector3H = vector => vector.x;
                    Vector3V = vector => vector.z;
                    Vector3D = vector => vector.y;
                    VectorHV = (h, v) => new Vector3(h, 0, v);
                    VectorHVD = (h, v, d) => new Vector3(h, d, v);
                    break;
                case MovementAxis.YZ:
                    Vector3H = vector => vector.z;
                    Vector3V = vector => vector.y;
                    Vector3D = vector => vector.x;
                    VectorHV = (h, v) => new Vector3(0, v, h);
                    VectorHVD = (h, v, d) => new Vector3(d, v, h);
                    break;
            }
        }
        #endif
    }
}