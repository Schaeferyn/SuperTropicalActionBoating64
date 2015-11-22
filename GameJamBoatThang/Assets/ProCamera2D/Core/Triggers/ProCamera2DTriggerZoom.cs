using System.Collections;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class ProCamera2DTriggerZoom : BaseTrigger
    {
        public bool SetSizeAsMultiplier = true;

        public float TargetZoom = 1.5f;

        public float ZoomSmoothness = 1f;

        [RangeAttribute(0, 1)]
        public float ExclusiveInfluencePercentage = .25f;

        public bool ResetSizeOnExit = false;
        public float ResetSizeSmoothness = 1f;

        float _startCamSize;
        float _initialCamSize;
        float _targetCamSize;
        float _targetCamSizeSmoothed;
        float _previousCamSize;
        float _zoomVelocity;

        override protected void Start()
        {
            base.Start();

            if (ProCamera2D == null)
                return;
            
            _startCamSize = ProCamera2D.GameCameraSize;
            _initialCamSize = _startCamSize;
            _targetCamSize = _startCamSize;
            _targetCamSizeSmoothed = _startCamSize;
        }

        protected override void EnteredTrigger()
        {
            base.EnteredTrigger();

            if (ProCamera2D.CurrentZoomTriggerID != _instanceID)
            {
                ProCamera2D.CurrentZoomTriggerID = _instanceID;

                if (ResetSizeOnExit)
                {
                    _initialCamSize = _startCamSize;
                    _targetCamSize = ProCamera2D.GameCameraSize;
                    _targetCamSizeSmoothed = ProCamera2D.GameCameraSize;
                }
                else
                {
                    _initialCamSize = ProCamera2D.GameCameraSize;
                    _targetCamSize = _initialCamSize;
                    _targetCamSizeSmoothed = _initialCamSize;
                }
            }

            StartCoroutine(InsideTriggerRoutine());
        }

        protected override void ExitedTrigger()
        {
            base.ExitedTrigger();

            if (ResetSizeOnExit)
                _targetCamSize = _startCamSize;

            StartCoroutine(OutsideTriggerRoutine());
        }

        IEnumerator InsideTriggerRoutine()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (_insideTrigger &&
                   _instanceID == ProCamera2D.CurrentZoomTriggerID)
            {
                _exclusiveInfluencePercentage = ExclusiveInfluencePercentage;

                var targetPos = new Vector2(Vector3H(UseTargetsMidPoint ? ProCamera2D.TargetsMidPoint : TriggerTarget.position), Vector3V(UseTargetsMidPoint ? ProCamera2D.TargetsMidPoint : TriggerTarget.position));
                var distancePercentage = GetDistanceToCenterPercentage(targetPos);

                float finalTargetSize;
                if (SetSizeAsMultiplier)
                    finalTargetSize = _startCamSize / TargetZoom;
                else if (ProCamera2D.GameCamera.orthographic)
                    finalTargetSize = TargetZoom;
                else
                    finalTargetSize = Mathf.Abs(Vector3D(ProCamera2D.CameraPosition)) * Mathf.Tan(TargetZoom * 0.5f * Mathf.Deg2Rad);

                var newTargetOrtographicSize = (_initialCamSize * distancePercentage) + (finalTargetSize * (1 - distancePercentage));

                var zoomSmoothness = ZoomSmoothness;
                if ((finalTargetSize > ProCamera2D.GameCameraSize
                    && newTargetOrtographicSize > _targetCamSize) ||
                    (finalTargetSize < ProCamera2D.GameCameraSize
                    && newTargetOrtographicSize < _targetCamSize))
                {
                    _targetCamSize = newTargetOrtographicSize;
                }
                else if (ResetSizeOnExit)
                {
                    _targetCamSize = newTargetOrtographicSize;
                    zoomSmoothness = ResetSizeSmoothness;
                }

                _previousCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y;

                if (Mathf.Abs(ProCamera2D.GameCameraSize - _targetCamSize) > .0001f)
                    UpdateScreenSize(zoomSmoothness);

                yield return (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;

                // If the camera is bounded, reset the easing
                if (_previousCamSize == ProCamera2D.ScreenSizeInWorldCoordinates.y)
                {
                    _targetCamSize = ProCamera2D.GameCameraSize;
                    _targetCamSizeSmoothed = _targetCamSize;
                    _zoomVelocity = 0f;
                }
            }
        }

        IEnumerator OutsideTriggerRoutine()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (!_insideTrigger &&
                   _instanceID == ProCamera2D.CurrentZoomTriggerID &&
                   Mathf.Abs(ProCamera2D.GameCameraSize - _targetCamSize) > .0001f)
            {
                UpdateScreenSize(ResetSizeOnExit ? ResetSizeSmoothness : ZoomSmoothness);
                yield return (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }
            _zoomVelocity = 0f;
        }

        protected void UpdateScreenSize(float smoothness)
        {
            _targetCamSizeSmoothed = Mathf.SmoothDamp(_targetCamSizeSmoothed, _targetCamSize, ref _zoomVelocity, smoothness);

            ProCamera2D.UpdateScreenSize(_targetCamSizeSmoothed);
        }

        #if UNITY_EDITOR
        int _drawGizmosCounter;

        override protected void OnDrawGizmos()
        {
            // HACK to prevent Unity bug on startup: http://forum.unity3d.com/threads/screen-position-out-of-view-frustum.9918/
            _drawGizmosCounter++;
            if (_drawGizmosCounter < 5 && UnityEditor.EditorApplication.timeSinceStartup < 60f)
                return;

            _exclusiveInfluencePercentage = ExclusiveInfluencePercentage;

            base.OnDrawGizmos();
            
            if (_gizmosDrawingFailed)
                return;

            var gameCamera = ProCamera2D.GetComponent<Camera>();
            var cameraDimensions = Utils.GetScreenSizeInWorldCoords(gameCamera, Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)));
            float cameraDepthOffset = Vector3D(ProCamera2D.transform.localPosition) + Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)) * Vector3D(ProCamera2D.transform.forward);

            var startOrthoSize = Application.isPlaying ? _startCamSize : cameraDimensions.y / 2;

            float finalTargetSize;
            if (SetSizeAsMultiplier)
                finalTargetSize = startOrthoSize / TargetZoom;
            else if (ProCamera2D.GameCamera.orthographic)
                finalTargetSize = TargetZoom;
            else
                finalTargetSize = Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)) * Mathf.Tan(TargetZoom * 0.5f * Mathf.Deg2Rad);
            
            Gizmos.DrawIcon(
                VectorHVD(
                    Vector3H(transform.position), 
                    Vector3V(transform.position), 
                    cameraDepthOffset), 
                startOrthoSize > finalTargetSize ? "ProCamera2D/gizmo_icon_zoom_in.png" : "ProCamera2D/gizmo_icon_zoom_out.png", 
                false);

            if(ResetSizeOnExit)
                Gizmos.DrawIcon(
                    VectorHVD(
                        Vector3H(transform.position), 
                        Vector3V(transform.position), 
                        cameraDepthOffset), 
                    "ProCamera2D/gizmo_icon_reset.png", 
                    false);
        }

        void OnDrawGizmosSelected()
        {
            // HACK to prevent Unity bug on startup: http://forum.unity3d.com/threads/screen-position-out-of-view-frustum.9918/
            _drawGizmosCounter++;
            if (_drawGizmosCounter < 5 && UnityEditor.EditorApplication.timeSinceStartup < 60f)
                return;

            if (_gizmosDrawingFailed)
                return;

            var gameCamera = ProCamera2D.GetComponent<Camera>();
            var cameraDimensions = Utils.GetScreenSizeInWorldCoords(gameCamera, Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)));
            float cameraDepthOffset = Vector3D(ProCamera2D.transform.localPosition) + Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)) * Vector3D(ProCamera2D.transform.forward);
            var cameraCenter = VectorHVD(Vector3H(transform.position), Vector3V(transform.position), cameraDepthOffset);

            var startOrthoSize = Application.isPlaying ? _startCamSize : cameraDimensions.y / 2;

            float finalTargetSize;
            if (SetSizeAsMultiplier)
                finalTargetSize = startOrthoSize / TargetZoom;
            else if (ProCamera2D.GameCamera.orthographic)
                finalTargetSize = TargetZoom;
            else
                finalTargetSize = Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)) * Mathf.Tan(TargetZoom * 0.5f * Mathf.Deg2Rad);

            Gizmos.color = EditorPrefsX.GetColor(PrefsData.ZoomTriggerColorKey, PrefsData.ZoomTriggerColorValue);
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
            Gizmos.DrawWireCube(cameraCenter, VectorHV(finalTargetSize * 2 * ProCamera2D.GameCamera.aspect, finalTargetSize * 2));
        }

        #endif
    }
}