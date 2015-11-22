using UnityEngine;
using System.Collections;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class ProCamera2DCameraWindow : BasePC2D
    {
        public Rect CameraWindowRect = new Rect(0f, 0f, .3f, .3f);
        Rect _cameraWindowRectInWorldCoords;

        void LateUpdate()
        {
            if (ProCamera2D.UpdateType == UpdateType.LateUpdate)
                CalculateOffset();
        }

        void FixedUpdate()
        {
            if (ProCamera2D.UpdateType == UpdateType.FixedUpdate)
                CalculateOffset();
        }

        void CalculateOffset()
        {
            // Remove the delta movement
            _transform.Translate(-ProCamera2D.DeltaMovement, Space.World);

            // Calculate the window rect
            _cameraWindowRectInWorldCoords = GetRectAroundTransf(CameraWindowRect, ProCamera2D.ScreenSizeInWorldCoordinates, _transform);

            // If camera final horizontal position outside camera window rect
            var horizontalDeltaMovement = 0f;
            if (ProCamera2D.CameraTargetPositionSmoothed.x >= _cameraWindowRectInWorldCoords.x + _cameraWindowRectInWorldCoords.width)
            {
                horizontalDeltaMovement = ProCamera2D.CameraTargetPositionSmoothed.x - (Vector3H(_transform.localPosition) + _cameraWindowRectInWorldCoords.width / 2 + CameraWindowRect.x);
            }
            else if (ProCamera2D.CameraTargetPositionSmoothed.x <= _cameraWindowRectInWorldCoords.x)
            {
                horizontalDeltaMovement = ProCamera2D.CameraTargetPositionSmoothed.x - (Vector3H(_transform.localPosition) - _cameraWindowRectInWorldCoords.width / 2 + CameraWindowRect.x);
            }

            // If camera final vertical position outside camera window rect
            var verticalDeltaMovement = 0f;
            if (ProCamera2D.CameraTargetPositionSmoothed.y >= _cameraWindowRectInWorldCoords.y + _cameraWindowRectInWorldCoords.height)
            {
                verticalDeltaMovement = ProCamera2D.CameraTargetPositionSmoothed.y - (Vector3V(_transform.localPosition) + _cameraWindowRectInWorldCoords.height / 2 + CameraWindowRect.y);
            }
            else if (ProCamera2D.CameraTargetPositionSmoothed.y <= _cameraWindowRectInWorldCoords.y)
            {
                verticalDeltaMovement = ProCamera2D.CameraTargetPositionSmoothed.y - (Vector3V(_transform.localPosition) - _cameraWindowRectInWorldCoords.height / 2 + CameraWindowRect.y);
            }

            var deltaMovement = VectorHV(horizontalDeltaMovement, verticalDeltaMovement);
            _transform.Translate(deltaMovement, Space.World);
        }

        Rect GetRectAroundTransf(Rect rectNormalized, Vector2 rectSize, Transform transf)
        {
            var finalRectSize = new Vector2(rectNormalized.width * rectSize.x, rectNormalized.height * rectSize.y);
            var rectPositionX = Vector3H(transf.localPosition) - finalRectSize.x / 2 + rectNormalized.x;
            var rectPositionY = Vector3V(transf.localPosition) - finalRectSize.y / 2 + rectNormalized.y;

            return new Rect(rectPositionX, rectPositionY, finalRectSize.x, finalRectSize.y);
        }

        #if UNITY_EDITOR
        int _drawGizmosCounter;

        override protected void OnDrawGizmos()
        {
            // HACK to prevent Unity bug on startup: http://forum.unity3d.com/threads/screen-position-out-of-view-frustum.9918/
            _drawGizmosCounter++;
            if (_drawGizmosCounter < 5 && UnityEditor.EditorApplication.timeSinceStartup < 60f)
                return;

            base.OnDrawGizmos();

            if (_gizmosDrawingFailed)
                return;

            var gameCamera = ProCamera2D.GetComponent<Camera>();
            var cameraDimensions = gameCamera.orthographic ? Utils.GetScreenSizeInWorldCoords(gameCamera) : Utils.GetScreenSizeInWorldCoords(gameCamera, Mathf.Abs(Vector3D(transform.localPosition)));
            float cameraDepthOffset = Vector3D(ProCamera2D.transform.localPosition) + Mathf.Abs(Vector3D(transform.localPosition)) * Vector3D(ProCamera2D.transform.forward);

            // Draw camera window
            Gizmos.color = EditorPrefsX.GetColor(PrefsData.CameraWindowColorKey, PrefsData.CameraWindowColorValue);
            var cameraRect = GetRectAroundTransf(CameraWindowRect, cameraDimensions, transform);
            Gizmos.DrawWireCube(VectorHVD(cameraRect.x + cameraRect.width / 2, cameraRect.y + cameraRect.height / 2, cameraDepthOffset), VectorHV(cameraRect.width, cameraRect.height));
        }
        #endif
    }
}