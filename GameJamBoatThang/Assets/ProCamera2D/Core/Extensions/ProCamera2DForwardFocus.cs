using System;
using System.Collections;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class ProCamera2DForwardFocus : BasePC2D
    {
        const float EPSILON = .001f;

        public bool Progressive = true;
        public float SpeedMultiplier = 1f;

        public float TransitionSmoothness = .5f;

        public bool MaintainInfluenceOnStop = true;

        [RangeAttribute(EPSILON, .5f)]
        public float LeftFocus = .25f;

        [RangeAttribute(EPSILON, .5f)]
        public float RightFocus = .25f;

        [RangeAttribute(EPSILON, .5f)]
        public float TopFocus = .25f;

        [RangeAttribute(EPSILON, .5f)]
        public float BottomFocus = .25f;

        float _hVel;
        float _hVelSmooth;
        float _vVel;
        float _vVelSmooth;

        float _targetHVel;
        float _targetVVel;

        bool _isFirstHorizontalCameraMovement;
        bool _isFirstVerticalCameraMovement;

        bool _enabled;

        override protected void Start()
        {
            base.Start();

            StartCoroutine(Enable());
        }

        IEnumerator Enable()
        {
            yield return new WaitForEndOfFrame();

            _enabled = true;
        }

        void LateUpdate()
        {
            if (_enabled && ProCamera2D.UpdateType == UpdateType.LateUpdate)
                ApplyInfluence();
        }

        void FixedUpdate()
        {
            if (_enabled && ProCamera2D.UpdateType == UpdateType.FixedUpdate)
                ApplyInfluence();
        }

        void ApplyInfluence()
        {
            var deltaTime = (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? Time.fixedDeltaTime : Time.deltaTime;
            if(Time.deltaTime < .0001f)
                return;
            
            var currentHVel = (Vector3H(ProCamera2D.TargetsMidPoint) - Vector3H(ProCamera2D.PreviousTargetsMidPoint)) / deltaTime;
            var currentVVel = (Vector3V(ProCamera2D.TargetsMidPoint) - Vector3V(ProCamera2D.PreviousTargetsMidPoint)) / deltaTime;
            if (Progressive)
            {
                currentHVel = Mathf.Clamp(currentHVel * SpeedMultiplier, -LeftFocus * ProCamera2D.ScreenSizeInWorldCoordinates.x, RightFocus * ProCamera2D.ScreenSizeInWorldCoordinates.x);
                currentVVel = Mathf.Clamp(currentVVel * SpeedMultiplier, -BottomFocus * ProCamera2D.ScreenSizeInWorldCoordinates.y, TopFocus * ProCamera2D.ScreenSizeInWorldCoordinates.y);
                
                if (MaintainInfluenceOnStop)
                {
                    if ((Mathf.Sign(currentHVel) == 1 && currentHVel < _hVel) ||
                            (Mathf.Sign(currentHVel) == -1 && currentHVel > _hVel) ||
                            (Mathf.Abs(currentHVel) < EPSILON))
                    {
                        currentHVel = _hVel;
                    }

                    if ((Mathf.Sign(currentVVel) == 1 && currentVVel < _vVel) ||
                            (Mathf.Sign(currentVVel) == -1 && currentVVel > _vVel) ||
                            (Mathf.Abs(currentVVel) < EPSILON))
                    {
                        currentVVel = _vVel;
                    }   
                }
            }
            else
            {
                if (MaintainInfluenceOnStop)
                {
                    bool switchedHorizontalDirection;
                    if (!_isFirstHorizontalCameraMovement && !(Mathf.Abs(currentHVel) < EPSILON))
                    {
                        _isFirstHorizontalCameraMovement = true;
                        switchedHorizontalDirection = true;
                    }
                    else
                    {
                        switchedHorizontalDirection = Mathf.Sign(currentHVel) != Mathf.Sign(_targetHVel);
                    }

                    if (!(Mathf.Abs(currentHVel) < EPSILON) && switchedHorizontalDirection)
                    {
                        _targetHVel = (currentHVel < 0f ? -LeftFocus : RightFocus) * ProCamera2D.ScreenSizeInWorldCoordinates.x;
                    }
                    currentHVel = _targetHVel;

                    bool switchedVerticalDirection;
                    if (!_isFirstVerticalCameraMovement && !(Mathf.Abs(currentVVel) < EPSILON))
                    {
                        _isFirstVerticalCameraMovement = true;
                        switchedVerticalDirection = true;
                    }
                    else
                    {
                        switchedVerticalDirection = Mathf.Sign(currentVVel) != Mathf.Sign(_targetVVel);
                    }

                    if (!(Mathf.Abs(currentVVel) < EPSILON) && switchedVerticalDirection)
                    {
                        _targetVVel = (currentVVel < 0f ? -BottomFocus : TopFocus) * ProCamera2D.ScreenSizeInWorldCoordinates.y;
                    }
                    currentVVel = _targetVVel;
                }
                else
                {
                    if (!(Mathf.Abs(currentHVel) < EPSILON))
                        currentHVel = (currentHVel < 0f ? -LeftFocus : RightFocus) * ProCamera2D.ScreenSizeInWorldCoordinates.x;
                    else
                        currentHVel = 0;

                    if (!(Mathf.Abs(currentVVel) < EPSILON))
                        currentVVel = (currentVVel < 0f ? -BottomFocus : TopFocus) * ProCamera2D.ScreenSizeInWorldCoordinates.y;
                    else
                        currentVVel = 0;
                }
            }

            // We need to clamp the values again to account for camera zooms
            currentHVel = Mathf.Clamp(currentHVel, -LeftFocus * ProCamera2D.ScreenSizeInWorldCoordinates.x, RightFocus * ProCamera2D.ScreenSizeInWorldCoordinates.x);
            currentVVel = Mathf.Clamp(currentVVel, -BottomFocus * ProCamera2D.ScreenSizeInWorldCoordinates.y, TopFocus * ProCamera2D.ScreenSizeInWorldCoordinates.y);

            // Smooth the values
            _hVel = Mathf.SmoothDamp(_hVel, currentHVel, ref _hVelSmooth, TransitionSmoothness);
            _vVel = Mathf.SmoothDamp(_vVel, currentVVel, ref _vVelSmooth, TransitionSmoothness);

            // Apply the influence
            ProCamera2D.ApplyInfluence(new Vector2(_hVel, _vVel));
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
            var cameraCenter = VectorHVD(Vector3H(transform.position), Vector3V(transform.position), cameraDepthOffset);

            Gizmos.color = EditorPrefsX.GetColor(PrefsData.ForwardFocusColorKey, PrefsData.ForwardFocusColorValue);

            if (LeftFocus > EPSILON)
            {
                Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) + cameraDimensions.x * LeftFocus, Vector3V(transform.position) - cameraDimensions.y / 2, cameraDepthOffset), transform.up * cameraDimensions.y);	
                Utils.DrawArrowForGizmo(cameraCenter + VectorHV(cameraDimensions.x * LeftFocus, 0), -transform.right * .3f);
            }

            if (RightFocus > EPSILON)
            {
                Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) - cameraDimensions.x * RightFocus, Vector3V(transform.position) - cameraDimensions.y / 2, cameraDepthOffset), transform.up * cameraDimensions.y);
                Utils.DrawArrowForGizmo(cameraCenter - VectorHV(cameraDimensions.x * RightFocus, 0), transform.right * .3f);
            }

            if (TopFocus > EPSILON)
            {
                Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) - cameraDimensions.x / 2, Vector3V(transform.position) - cameraDimensions.y * TopFocus, cameraDepthOffset), transform.right * cameraDimensions.x);
                Utils.DrawArrowForGizmo(cameraCenter - VectorHV(0, cameraDimensions.y * TopFocus), transform.up * .3f);
            }

            if (BottomFocus > EPSILON)
            {
                Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) - cameraDimensions.x / 2, Vector3V(transform.position) + cameraDimensions.y * BottomFocus, cameraDepthOffset), transform.right * cameraDimensions.x);
                Utils.DrawArrowForGizmo(cameraCenter + VectorHV(0, cameraDimensions.y * BottomFocus), -transform.up * .3f);
            }
        }
        #endif
    }
}
