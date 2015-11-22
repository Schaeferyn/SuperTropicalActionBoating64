using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [System.Serializable]
    public class CinematicTarget
    {
        public Transform TargetTransform;
        public float EaseInDuration = 1f;
        public float HoldDuration = 1f;
        public float Zoom = 1f;
        public EaseType EaseType = EaseType.EaseOut;
        public string SendMessageName;
        public string SendMessageParam;
    }

    public class ProCamera2DCinematics : BasePC2D
    {
        public Action CinematicStarted;
        public Action<int> CinematicReachedTarget;
        public Action CinematicEnded;

        bool _isActive;

        public bool IsActive { get { return _isActive; } }

        public List<CinematicTarget> CinematicTargets = new List<CinematicTarget>();
        public float EndDuration = 1f;
        public EaseType EndEaseType = EaseType.EaseOut;

        public bool UseNumericBoundaries;

        public bool UseLetterbox = true;

        [Range(0f, .5f)]
        public float LetterboxAmount = .1f;

        public float LetterboxAnimDuration = 1f;

        public Color LetterboxColor = Color.black;

        ProCamera2DPositionAndSizeOverrider _overridePosition;
        float _initialCameraSize;

        ProCamera2DLetterbox _letterbox;

        Coroutine _startCinematicRoutine;
        Coroutine _goToCinematicRoutine;
        Coroutine _endCinematicRoutine;

        bool _skipTarget;

        override protected void Start()
        {
            base.Start();

            var positionAndSizeOverrider = ProCamera2D.gameObject.GetComponent<ProCamera2DPositionAndSizeOverrider>();
            _overridePosition = positionAndSizeOverrider == null ? ProCamera2D.gameObject.AddComponent<ProCamera2DPositionAndSizeOverrider>() : positionAndSizeOverrider;
            _overridePosition.hideFlags = HideFlags.HideInInspector;
            _overridePosition.enabled = false;

            if (UseLetterbox)
                SetupLetterbox();

            _initialCameraSize = ProCamera2D.GameCameraSize;
        }

        /// <summary>Play the cinematic.</summary>
        public void Play()
        {
            if (_isActive)
                return;

            if (CinematicTargets.Count == 0)
            {
                Debug.LogWarning("No cinematic targets added to the list");
                return;
            }

            var numericBoundaries = ProCamera2D.gameObject.GetComponent<ProCamera2DNumericBoundaries>();
            if (UseNumericBoundaries && numericBoundaries != null)
            {
                _overridePosition.UseNumericBoundaries = true;
                _overridePosition.UseTopBoundary = numericBoundaries.UseTopBoundary;
                _overridePosition.TopBoundary = numericBoundaries.TopBoundary;
                _overridePosition.UseBottomBoundary = numericBoundaries.UseBottomBoundary;
                _overridePosition.BottomBoundary = numericBoundaries.BottomBoundary;
                _overridePosition.UseLeftBoundary = numericBoundaries.UseLeftBoundary;
                _overridePosition.LeftBoundary = numericBoundaries.LeftBoundary;
                _overridePosition.UseRightBoundary = numericBoundaries.UseRightBoundary;
                _overridePosition.RightBoundary = numericBoundaries.RightBoundary;
            }
            else if (!UseNumericBoundaries && numericBoundaries != null)
            {
                _overridePosition.UseNumericBoundaries = false;
            }

            _isActive = true;
            if (_endCinematicRoutine != null)
            {
                StopCoroutine(_endCinematicRoutine);
                _endCinematicRoutine = null;
            }

            if (_startCinematicRoutine == null)
                _startCinematicRoutine = StartCoroutine(StartCinematicRoutine());
        }

        /// <summary>Stop the cinematic.</summary>
        public void Stop()
        {
            if (!_isActive)
                return;

            _isActive = false;
            if (_startCinematicRoutine != null)
            {
                StopCoroutine(_startCinematicRoutine);
                _startCinematicRoutine = null;
            }

            if (_goToCinematicRoutine != null)
            {
                StopCoroutine(_goToCinematicRoutine);
                _goToCinematicRoutine = null;
            }

            if (_endCinematicRoutine == null)
                _endCinematicRoutine = StartCoroutine(EndCinematicRoutine());
        }

        /// <summary>If the cinematic is stopped, it plays it. If it's playing, it stops it.</summary>
        public void Toggle()
        {
            if (_isActive)
                Stop();
            else
                Play();
        }

        /// <summary>Goes to the next cinematic target</summary>
        public void GoToNextTarget()
        {
            _skipTarget = true;
        }

        IEnumerator StartCinematicRoutine()
        {
            if (CinematicStarted != null)
                CinematicStarted();

            _overridePosition.OverridePosition = ProCamera2D.CameraPosition;
            _overridePosition.OverrideSize = ProCamera2D.GameCameraSize;
            _overridePosition.enabled = true;

            if (UseLetterbox)
            {
                if (_letterbox == null)
                    SetupLetterbox();

                _letterbox.Color = LetterboxColor;
                _letterbox.TweenTo(LetterboxAmount, LetterboxAnimDuration);
            }

            var count = -1;
            while (count < CinematicTargets.Count - 1)
            {
                count++;
                _skipTarget = false;
                _goToCinematicRoutine = StartCoroutine(GoToCinematicTargetRoutine(CinematicTargets[count], count));
                yield return _goToCinematicRoutine;
            }

            Stop();
        }

        IEnumerator GoToCinematicTargetRoutine(CinematicTarget cinematicTarget, int targetIndex)
        {
            if (cinematicTarget.TargetTransform == null)
                yield break;

            var initialPosH = Vector3H(ProCamera2D.CameraPosition);
            var initialPosV = Vector3V(ProCamera2D.CameraPosition);

            var currentCameraSize = ProCamera2D.GameCameraSize;

            // Ease in
            var waitForFixedUpdate = new WaitForFixedUpdate();
            var t = 0f;
            while (t <= 1.0f)
            {
                t += (ProCamera2D.UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime) / cinematicTarget.EaseInDuration;

                _overridePosition.OverridePosition = VectorHVD(
                    Utils.EaseFromTo(initialPosH, Vector3H(cinematicTarget.TargetTransform.position), t, cinematicTarget.EaseType), 
                    Utils.EaseFromTo(initialPosV, Vector3V(cinematicTarget.TargetTransform.position), t, cinematicTarget.EaseType),
                    Vector3D(ProCamera2D.CameraPosition));

                _overridePosition.OverrideSize = Utils.EaseFromTo(currentCameraSize, _initialCameraSize / cinematicTarget.Zoom, t, cinematicTarget.EaseType);

                if (_skipTarget)
                    yield break;
                
                yield return (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }

            // Dispatch target reached event
            if (CinematicReachedTarget != null)
                CinematicReachedTarget(targetIndex);

            // Send target reached message
            if (!string.IsNullOrEmpty(cinematicTarget.SendMessageName))
            {
                cinematicTarget.TargetTransform.SendMessage(cinematicTarget.SendMessageName, cinematicTarget.SendMessageParam, SendMessageOptions.DontRequireReceiver);
            }

            // Hold
            t = 0f;
            while (cinematicTarget.HoldDuration < 0 || t <= cinematicTarget.HoldDuration)
            {
                t += ProCamera2D.UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime;

                _overridePosition.OverridePosition = VectorHVD(
                    Vector3H(cinematicTarget.TargetTransform.position), 
                    Vector3V(cinematicTarget.TargetTransform.position),
                    Vector3D(ProCamera2D.CameraPosition));

                if (_skipTarget)
                    yield break;
                
                yield return (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }
        }

        IEnumerator EndCinematicRoutine()
        {
            if (_letterbox != null && LetterboxAmount > 0)
                _letterbox.TweenTo(0f, LetterboxAnimDuration);

            var initialPosH = Vector3H(_overridePosition.OverridePosition);
            var initialPosV = Vector3V(_overridePosition.OverridePosition);

            var currentCameraSize = ProCamera2D.GameCameraSize;

            // Ease out
            var waitForFixedUpdate = new WaitForFixedUpdate();
            var t = 0f;
            while (t <= 1.0f)
            {
                t += (ProCamera2D.UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime) / EndDuration;

                _overridePosition.OverridePosition = VectorHVD(
                    Utils.EaseFromTo(initialPosH, Vector3H(_overridePosition.OverridenPosition), t, EndEaseType), 
                    Utils.EaseFromTo(initialPosV, Vector3V(_overridePosition.OverridenPosition), t, EndEaseType),
                    Vector3D(ProCamera2D.CameraPosition));

                _overridePosition.OverrideSize = Utils.EaseFromTo(currentCameraSize, _initialCameraSize, t, EndEaseType);

                yield return (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }

            _overridePosition.enabled = false;

            if (CinematicEnded != null)
                CinematicEnded();
        }

        void SetupLetterbox()
        {
            var letterbox = ProCamera2D.gameObject.GetComponent<ProCamera2DLetterbox>();
            _letterbox = letterbox == null ? ProCamera2D.gameObject.AddComponent<ProCamera2DLetterbox>() : letterbox;
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
            
            float cameraDepthOffset = Vector3D(ProCamera2D.transform.localPosition) + Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)) * Vector3D(ProCamera2D.transform.forward);

            // Draw cinematic targets
            for (int i = 0; i < CinematicTargets.Count; i++)
            {
                if (CinematicTargets[i].TargetTransform != null)
                {
                    var targetPos = VectorHVD(Vector3H(CinematicTargets[i].TargetTransform.position), Vector3V(CinematicTargets[i].TargetTransform.position), cameraDepthOffset);
                    Gizmos.DrawIcon(targetPos, "ProCamera2D/gizmo_icon_exclusive_free.png", false);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            // HACK to prevent Unity bug on startup: http://forum.unity3d.com/threads/screen-position-out-of-view-frustum.9918/
            _drawGizmosCounter++;
            if (_drawGizmosCounter < 5 && UnityEditor.EditorApplication.timeSinceStartup < 60f)
                return;

            base.OnDrawGizmos();

            if (_gizmosDrawingFailed)
                return;

            var gameCamera = ProCamera2D.GetComponent<Camera>();
            float cameraDepthOffset = Vector3D(ProCamera2D.transform.localPosition) + Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)) * Vector3D(ProCamera2D.transform.forward);
            var cameraDimensions = Utils.GetScreenSizeInWorldCoords(gameCamera, Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)));

            // Draw cinematic path
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.green;
            for (int i = 0; i < CinematicTargets.Count; i++)
            {
                var targetPos = VectorHVD(Vector3H(CinematicTargets[i].TargetTransform.position), Vector3V(CinematicTargets[i].TargetTransform.position), cameraDepthOffset);

                if (i > 0)
                {
                    UnityEditor.Handles.DrawLine(targetPos, VectorHVD(Vector3H(CinematicTargets[i - 1].TargetTransform.position), Vector3V(CinematicTargets[i - 1].TargetTransform.position), cameraDepthOffset));
                }

                UnityEditor.Handles.color = Color.blue;
                if (i < CinematicTargets.Count - 1)
                {
                    var nextTargetPos = VectorHVD(Vector3H(CinematicTargets[i + 1].TargetTransform.position), Vector3V(CinematicTargets[i + 1].TargetTransform.position), cameraDepthOffset);
                    var arrowSize = cameraDimensions.x * .1f;
                    if ((nextTargetPos - targetPos).magnitude > arrowSize)
                    {
                        UnityEditor.Handles.ArrowCap(
                            i, 
                            targetPos, 
                            Quaternion.LookRotation(nextTargetPos - targetPos), 
                            cameraDimensions.x * .1f);
                    }
                }
            }
        }
        #endif
    }
}