using UnityEngine;
using System.Collections;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class ProCamera2DNumericBoundaries : BasePC2D
    {
        public bool UseNumericBoundaries;
        public bool UseTopBoundary;
        public float TopBoundary = 10f;
        public float TargetTopBoundary;

        public bool UseBottomBoundary;
        public float BottomBoundary = -10f;
        public float TargetBottomBoundary;

        public bool UseLeftBoundary;
        public float LeftBoundary = -10f;
        public float TargetLeftBoundary;

        public bool UseRightBoundary;
        public float RightBoundary = 10f;
        public float TargetRightBoundary;

        public bool IsCameraSizeBounded;
        public bool IsCameraPositionHorizontallyBounded;
        public bool IsCameraPositionVerticallyBounded;

        public Coroutine BoundariesAnimRoutine;
        public Coroutine TopBoundaryAnimRoutine;
        public Coroutine BottomBoundaryAnimRoutine;
        public Coroutine LeftBoundaryAnimRoutine;
        public Coroutine RightBoundaryAnimRoutine;
        public int CurrentBoundariesTriggerID;

        public Coroutine MoveCameraToTargetRoutine;

        void LateUpdate()
        {
            if (UseNumericBoundaries && ProCamera2D.UpdateType == UpdateType.LateUpdate)
                LimitSizeAndPositionToNumericBoundaries();
        }

        void FixedUpdate()
        {
            if (UseNumericBoundaries && ProCamera2D.UpdateType == UpdateType.FixedUpdate)
                LimitSizeAndPositionToNumericBoundaries();
        }

        void LimitSizeAndPositionToNumericBoundaries()
        {
            // Set new size if outside boundaries
            IsCameraSizeBounded = false;
            var cameraBounds = new Vector2(RightBoundary - LeftBoundary, TopBoundary - BottomBoundary);
            if (UseRightBoundary && UseLeftBoundary && ProCamera2D.ScreenSizeInWorldCoordinates.x > cameraBounds.x)
            {
                ProCamera2D.UpdateScreenSize(cameraBounds.x / ProCamera2D.GameCamera.aspect / 2);
                IsCameraSizeBounded = true;
            }

            if (UseTopBoundary && UseBottomBoundary && ProCamera2D.ScreenSizeInWorldCoordinates.y > cameraBounds.y)
            {
                ProCamera2D.UpdateScreenSize(cameraBounds.y / 2);
                IsCameraSizeBounded = true;
            }

            // Check movement in the horizontal dir
            IsCameraPositionHorizontallyBounded = false;
            IsCameraPositionVerticallyBounded = false;
            var newPosH = Vector3H(_transform.localPosition);
            if (UseLeftBoundary && newPosH - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2 < LeftBoundary)
            {
                newPosH = LeftBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2;
                IsCameraPositionHorizontallyBounded = true;
            }
            else if (UseRightBoundary && newPosH + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2 > RightBoundary)
            {
                newPosH = RightBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2;
                IsCameraPositionHorizontallyBounded = true;
            }

            // Check movement in the vertical dir
            var newPosV = Vector3V(_transform.localPosition);
            if (UseBottomBoundary && newPosV - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2 < BottomBoundary)
            {
                newPosV = BottomBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2;
                IsCameraPositionVerticallyBounded = true;
            }
            else if (UseTopBoundary && newPosV + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2 > TopBoundary)
            {
                newPosV = TopBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2;
                IsCameraPositionVerticallyBounded = true;
            }

            // Set to the new position
            if (IsCameraPositionHorizontallyBounded || IsCameraPositionVerticallyBounded)
                ProCamera2D.CameraPosition = VectorHVD(newPosH, newPosV, Vector3D(_transform.localPosition));
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

            // Numeric boundaries
            Gizmos.color = EditorPrefsX.GetColor(PrefsData.NumericBoundariesColorKey, PrefsData.NumericBoundariesColorValue);

            if (UseNumericBoundaries)
            {
                if (UseTopBoundary)
                    Gizmos.DrawRay(VectorHVD(Vector3H(transform.localPosition) - cameraDimensions.x / 2, TopBoundary, cameraDepthOffset), transform.right * cameraDimensions.x);

                if (UseBottomBoundary)
                    Gizmos.DrawRay(VectorHVD(Vector3H(transform.localPosition) - cameraDimensions.x / 2, BottomBoundary, cameraDepthOffset), transform.right * cameraDimensions.x);

                if (UseRightBoundary)
                    Gizmos.DrawRay(VectorHVD(RightBoundary, Vector3V(transform.localPosition) - cameraDimensions.y / 2, cameraDepthOffset), transform.up * cameraDimensions.y);

                if (UseLeftBoundary)
                    Gizmos.DrawRay(VectorHVD(LeftBoundary, Vector3V(transform.localPosition) - cameraDimensions.y / 2, cameraDepthOffset), transform.up * cameraDimensions.y);
            }
        }
        #endif
    }
}