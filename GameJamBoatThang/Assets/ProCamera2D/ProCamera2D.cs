using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    /// <summary>
    /// Main class of the plugin. Everything starts and happens here.
    /// All plugins and helpers will need a reference to an instance of this class.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ProCamera2D : MonoBehaviour
    {
        public const string VERSION = "1.6.2";

        public bool Enabled = true;

        public Action OnExclusiveFocusStarted;
        public Action OnExclusiveFocusFinished;

        public Action OnBoundariesTransitionStarted;
        public Action OnBoundariesTransitionFinished;

        public MovementAxis Axis;
        Func<Vector3, float> Vector3H;
        Func<Vector3, float> Vector3V;
        Func<Vector3, float> Vector3D;
        Func<float, float, Vector3> VectorHV;
        Func<float, float, float, Vector3> VectorHVD;

        public bool FollowHorizontal = true;
        public bool FollowVertical = true;
        
        public Camera GameCamera;

        public float HorizontalFollowSmoothness = 0.15f;
        public float VerticalFollowSmoothness = 0.15f;

        public bool LimitMaxHorizontalSpeed;
        public float MaxHorizontalSpeed = .2f;
        
        public bool LimitMaxVerticalSpeed;
        public float MaxVerticalSpeed = .2f;

        public List<CameraTarget> CameraTargets = new List<CameraTarget>();

        public Vector2 ScreenSizeInWorldCoordinates { get; set; }

        public Vector2 OverallOffset;

        public UpdateType UpdateType;

        Coroutine _updateScreenSizeCoroutine;

        public Vector3 PreviousTargetsMidPoint { get { return _previousTargetsMidPoint; } }

        Vector3 _previousTargetsMidPoint;

        public Vector3 TargetsMidPoint { get { return _targetsMidPoint; } }

        Vector3 _targetsMidPoint;

        List<Vector3> _influences = new List<Vector3>();
        Vector3 _influencesSum = Vector3.zero;

        public Vector3 CameraTargetPosition { get { return _cameraTargetPosition; } }

        Vector3 _cameraTargetPosition;

        float _cameraTargetHorizontalPositionSmoothed;
        float _previousCameraTargetHorizontalPositionSmoothed;
        float _cameraTargetVerticalPositionSmoothed;
        float _previousCameraTargetVerticalPositionSmoothed;

        public Vector2 CameraTargetPositionSmoothed { get { return new Vector2(_cameraTargetHorizontalPositionSmoothed, _cameraTargetVerticalPositionSmoothed); } }

        public Vector3 CameraPosition { get { return _transform.localPosition; } set { _transform.localPosition = value; } }

        float _originalCameraDepthSign;

        public float CameraDepthPos { get { return _cameraDepthPos; } }
        float _cameraDepthPos;

        public Vector3? ExclusiveTargetPosition;

        public bool LimitHorizontalCameraDistance;
        public float MaxHorizontalTargetDistance = .8f;
        public bool LimitVerticalCameraDistance;
        public float MaxVerticalTargetDistance = .8f;

        public bool CenterTargetOnStart;

        public int CurrentZoomTriggerID;

        Transform _transform;

        #if PC2D_TK2D_SUPPORT
        public tk2dCamera Tk2dCam;
        #endif

        Vector3 _deltaMovement;
        public Vector3 DeltaMovement { get { return _deltaMovement; } }

        public float GameCameraSize
        {
            get
            {
                if (GameCamera.orthographic)
                {
                    #if PC2D_TK2D_SUPPORT
                    if (Tk2dCam != null)
                        return Tk2dCam.CameraSettings.orthographicSize / Tk2dCam.ZoomFactor;
                    #endif

                    return GameCamera.orthographicSize;
                }
                else
                    return Mathf.Abs(Vector3D(CameraPosition)) * Mathf.Tan(GameCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            }
        }

        static ProCamera2D _instance;

        public static ProCamera2D Instance
        {
            get
            {
                if (Equals(_instance, null))
                {
                    _instance = FindObjectOfType(typeof(ProCamera2D)) as ProCamera2D;

                    if (Equals(_instance, null))
                        throw new UnityException("ProCamera2D does not exist.");
                }

                return _instance;
            }
        }

        void Awake()
        {
            _instance = this;
            _transform = transform;

            if (GameCamera == null)
                GameCamera = GetComponent<Camera>();
            if (GameCamera == null)
                Debug.LogError("Unity Camera not set and not found on the GameObject: " + gameObject.name);

            #if PC2D_TK2D_SUPPORT
            Tk2dCam = GetComponent<tk2dCamera>();
            #endif

            switch (Axis)
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

            // Remove empty targets
            for (int i = 0; i < CameraTargets.Count; i++)
            {
                if (CameraTargets[i].TargetTransform == null)
                {
                    CameraTargets.RemoveAt(i);
                }
            }

            ScreenSizeInWorldCoordinates = Utils.GetScreenSizeInWorldCoords(GameCamera, Mathf.Abs(Vector3D(_transform.localPosition)));

            _cameraDepthPos = Vector3D(_transform.localPosition);
            _originalCameraDepthSign = Mathf.Sign(_cameraDepthPos);

            // Center on target
            if (CenterTargetOnStart && CameraTargets.Count > 0)
            {
                var targetPos = GetTargetsWeightedMidPoint(CameraTargets);
                var cameraTargetPositionX = FollowHorizontal ? Vector3H(targetPos) : Vector3H(_transform.localPosition);
                var cameraTargetPositionY = FollowVertical ? Vector3V(targetPos) : Vector3V(_transform.localPosition);
                targetPos = VectorHV(cameraTargetPositionX, cameraTargetPositionY);
                targetPos += VectorHV(OverallOffset.x, OverallOffset.y);
                MoveCameraInstantlyToPosition(targetPos);
            }
            else
            {
                _cameraTargetPosition = _transform.localPosition;
                _cameraTargetHorizontalPositionSmoothed = Vector3H(_cameraTargetPosition);
                _previousCameraTargetHorizontalPositionSmoothed = _cameraTargetHorizontalPositionSmoothed;
                _cameraTargetVerticalPositionSmoothed = Vector3V(_cameraTargetPosition);
                _previousCameraTargetVerticalPositionSmoothed = _cameraTargetVerticalPositionSmoothed;
            }
        }

        void LateUpdate()
        {
            if (Enabled && UpdateType == UpdateType.LateUpdate)
                Move();
        }

        void FixedUpdate()
        {
            if (Enabled && UpdateType == UpdateType.FixedUpdate)
                Move();
        }

        void OnApplicationQuit()
        {
            _instance = null;
        }

        /// <summary>Apply the given influence to the camera during this frame.</summary>
        /// <param name="influence">The vector representing the influence to be applied</param>
        public void ApplyInfluence(Vector2 influence)
        {
            if (Time.deltaTime < .0001f || float.IsNaN(influence.x) || float.IsNaN(influence.y))
                return;

            _influences.Add(VectorHV(influence.x, influence.y));
        }

        /// <summary>Apply the given influences to the camera during the corresponding durations.</summary>
        /// <param name="influences">An array of the vectors representing the influences to be applied</param>
        /// <param name="durations">An array with the durations of the influences to be applied</param>
        public Coroutine ApplyInfluencesTimed(Vector2[] influences, float[] durations)
        {
            return StartCoroutine(ApplyInfluencesTimedRoutine(influences, durations));
        }

        /// <summary>Add a target for the camera to follow.</summary>
        /// <param name="targetTransform">The Transform of the target</param>
        /// <param name="targetInfluenceH">The influence this target horizontal position should have when calculating the average position of all the targets</param>
        /// <param name="targetInfluenceV">The influence this target vertical position should have when calculating the average position of all the targets</param>
        /// <param name="duration">The time it takes for this target to reach it's influence. Use for a more progressive transition.</param>
        public CameraTarget AddCameraTarget(Transform targetTransform, float targetInfluenceH = 1f, float targetInfluenceV = 1f, float duration = 0f)
        {
            var newCameraTarget = new CameraTarget
            {
                TargetTransform = targetTransform,
                TargetInfluenceH = targetInfluenceH,
                TargetInfluenceV = targetInfluenceV,
            };

            CameraTargets.Add(newCameraTarget);

            if (duration > 0f)
            {
                newCameraTarget.TargetInfluence = 0f;
                StartCoroutine(AdjustTargetInfluenceRoutine(newCameraTarget, targetInfluenceH, targetInfluenceV, duration));
            }

            return newCameraTarget;
        }

        /// <summary>Gets the corresponding CameraTarget from an object's transform.</summary>
        /// <param name="targetTransform">The Transform of the target</param>
        public CameraTarget GetCameraTarget(Transform targetTransform)
        {
            for (int i = 0; i < CameraTargets.Count; i++)
            {
                if (CameraTargets[i].TargetTransform.GetInstanceID() == targetTransform.GetInstanceID())
                {
                    return CameraTargets[i];
                }
            }
            return null;
        }

        /// <summary>Remove a target from the camera.</summary>
        /// <param name="targetTransform">The Transform of the target</param>
        /// <param name="duration">The time it takes for this target to reach a zero influence. Use for a more progressive transition.</param>
        public void RemoveCameraTarget(Transform targetTransform, float duration = 0f)
        {
            for (int i = 0; i < CameraTargets.Count; i++)
            {
                if (CameraTargets[i].TargetTransform.GetInstanceID() == targetTransform.GetInstanceID())
                {
                    if (duration > 0)
                    {
                        StartCoroutine(AdjustTargetInfluenceRoutine(CameraTargets[i], 0, 0, duration, true));
                    }
                    else
                        CameraTargets.Remove(CameraTargets[i]);
                }
            }
        }

        /// <summary>Adjusts a target influence</summary>
        /// <param name="cameraTarget">The CameraTarget of the target</param>
        /// <param name="targetInfluenceH">The influence this target horizontal position should have when calculating the average position of all the targets</param>
        /// <param name="targetInfluenceV">The influence this target vertical position should have when calculating the average position of all the targets</param>
        /// <param name="duration">The time it takes for this target to reach it's influence. Don't use a duration if calling every frame.</param>
        public Coroutine AdjustCameraTargetInfluence(CameraTarget cameraTarget, float targetInfluenceH, float targetInfluenceV, float duration = 0)
        {
            if (duration > 0)
                return StartCoroutine(AdjustTargetInfluenceRoutine(cameraTarget, targetInfluenceH, targetInfluenceV, duration));
            else
            {
                cameraTarget.TargetInfluenceH = targetInfluenceH;
                cameraTarget.TargetInfluenceV = targetInfluenceH;

                return null;
            }
        }

        /// <summary>Adjusts a target influence, finding it first by its transform.</summary>
        /// <param name="cameraTargetTransf">The Transform of the target</param>
        /// <param name="targetInfluenceH">The influence this target horizontal position should have when calculating the average position of all the targets</param>
        /// <param name="targetInfluenceV">The influence this target vertical position should have when calculating the average position of all the targets</param>
        /// <param name="duration">The time it takes for this target to reach it's influence. Don't use a duration if calling every frame.</param>
        public Coroutine AdjustCameraTargetInfluence(Transform cameraTargetTransf, float targetInfluenceH, float targetInfluenceV, float duration = 0)
        {
            var cameraTarget = GetCameraTarget(cameraTargetTransf);

            if (cameraTarget == null)
                return null;

            return AdjustCameraTargetInfluence(cameraTarget, targetInfluenceH, targetInfluenceV, duration);
        }

        /// <summary>Moves the camera instantly to the supplied position</summary>
        /// <param name="cameraPos">The final position of the camera</param>
        public void MoveCameraInstantlyToPosition(Vector3 cameraPos)
        {
            _transform.localPosition = VectorHVD(Vector3H(cameraPos), Vector3V(cameraPos), Vector3D(_transform.localPosition));

            _cameraTargetPosition = _transform.localPosition;

            _cameraTargetHorizontalPositionSmoothed = Vector3H(_cameraTargetPosition);
            _cameraTargetVerticalPositionSmoothed = Vector3V(_cameraTargetPosition);

            _previousCameraTargetHorizontalPositionSmoothed = _cameraTargetHorizontalPositionSmoothed;
            _previousCameraTargetVerticalPositionSmoothed = _cameraTargetVerticalPositionSmoothed;
        }

        /// <summary>Resize the camera to the supplied size</summary>
        /// <param name="newSize">Half of the wanted size in world units</param>
        /// <param name="duration">How long it should take to reach the provided size</param>
        /// <param name="easeType">The easing method to apply when the duration is bigger than 0</param>
        public void UpdateScreenSize(float newSize, float duration = 0, EaseType easeType = EaseType.Linear)
        {
            if (duration > 0)
            {
                if (_updateScreenSizeCoroutine != null)
                    StopCoroutine(_updateScreenSizeCoroutine);
                
                _updateScreenSizeCoroutine = StartCoroutine(UpdateScreenSizeRoutine(newSize, duration, easeType));
                return;
            }

            if (GameCamera.orthographic)
            {
                GameCamera.orthographicSize = newSize;
                _cameraDepthPos = Vector3D(_transform.localPosition);
            }
            else
            {
                _cameraDepthPos = (newSize / Mathf.Tan(GameCamera.fieldOfView * 0.5f * Mathf.Deg2Rad)) * _originalCameraDepthSign;
                _transform.localPosition = VectorHVD(Vector3H(_transform.localPosition), Vector3V(_transform.localPosition), _cameraDepthPos);
            }

            ScreenSizeInWorldCoordinates = new Vector2(newSize * 2 * GameCamera.aspect, newSize * 2);

            #if PC2D_TK2D_SUPPORT
            if (Tk2dCam == null)
                return;
            
            if (Tk2dCam.CameraSettings.projection == tk2dCameraSettings.ProjectionType.Orthographic)
                Tk2dCam.ZoomFactor = Tk2dCam.CameraSettings.orthographicSize / newSize;
            #endif
        }

        /// <summary>
        /// Move the camera to the average position of all the targets.
        /// This method is automatically called every frame on LateUpdate/FixedUpdate.
        /// Use only if you want to manually control the update frequency.
        /// </summary>
        public void Move()
        {
            // Delta time
            var deltaTime = UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime;
            if (deltaTime < .0001f)
                return;

            // Calculate targets mid point
            _previousTargetsMidPoint = _targetsMidPoint;
            _targetsMidPoint = GetTargetsWeightedMidPoint(CameraTargets);
            _cameraTargetPosition = _targetsMidPoint;

            // Calculate influences
            _influencesSum = Utils.GetVectorsSum(_influences);
            _cameraTargetPosition += _influencesSum;
            _influences.Clear();

            // Follow only on selected axis
            var cameraTargetPositionX = FollowHorizontal ? Vector3H(_cameraTargetPosition) : Vector3H(_transform.localPosition);
            var cameraTargetPositionY = FollowVertical ? Vector3V(_cameraTargetPosition) : Vector3V(_transform.localPosition);
            _cameraTargetPosition = VectorHV(cameraTargetPositionX, cameraTargetPositionY);

            // Ignore targets and influences if exclusive position is set
            if (ExclusiveTargetPosition.HasValue)
            {
                _cameraTargetPosition = ExclusiveTargetPosition.Value;
                ExclusiveTargetPosition = null;
            }

            // Add offset
            _cameraTargetPosition += VectorHV(FollowHorizontal ? OverallOffset.x : 0, FollowVertical ? OverallOffset.y : 0);

            // Tween camera final position
            _cameraTargetHorizontalPositionSmoothed = Utils.SmoothApproach(_cameraTargetHorizontalPositionSmoothed, _previousCameraTargetHorizontalPositionSmoothed, Vector3H(_cameraTargetPosition), 1f / HorizontalFollowSmoothness, deltaTime);
            _previousCameraTargetHorizontalPositionSmoothed = _cameraTargetHorizontalPositionSmoothed;

            _cameraTargetVerticalPositionSmoothed = Utils.SmoothApproach(_cameraTargetVerticalPositionSmoothed, _previousCameraTargetVerticalPositionSmoothed, Vector3V(_cameraTargetPosition), 1f / VerticalFollowSmoothness, deltaTime);
            _previousCameraTargetVerticalPositionSmoothed = _cameraTargetVerticalPositionSmoothed;

            // Limit camera distance to target
            if (LimitHorizontalCameraDistance)
            {
                var horizontalCompensation = Vector3H(_cameraTargetPosition) - Vector3H(_transform.localPosition) - (ScreenSizeInWorldCoordinates.x / 2) * MaxHorizontalTargetDistance;
                if (horizontalCompensation > 0)
                {
                    _cameraTargetHorizontalPositionSmoothed += horizontalCompensation;
                    _previousCameraTargetHorizontalPositionSmoothed = _cameraTargetHorizontalPositionSmoothed;
                }
                else if (horizontalCompensation < -ScreenSizeInWorldCoordinates.x * MaxHorizontalTargetDistance)
                {
                    _cameraTargetHorizontalPositionSmoothed += horizontalCompensation + ScreenSizeInWorldCoordinates.x * MaxHorizontalTargetDistance;
                    _previousCameraTargetHorizontalPositionSmoothed = _cameraTargetHorizontalPositionSmoothed;
                }
            }
            if (LimitVerticalCameraDistance)
            {
                var verticalCompensation = Vector3V(_cameraTargetPosition) - Vector3V(_transform.localPosition) - (ScreenSizeInWorldCoordinates.y / 2) * MaxVerticalTargetDistance;
                if (verticalCompensation > 0)
                {
                    _cameraTargetVerticalPositionSmoothed += verticalCompensation;
                    _previousCameraTargetVerticalPositionSmoothed = _cameraTargetVerticalPositionSmoothed;
                }
                else if (verticalCompensation < -ScreenSizeInWorldCoordinates.y * MaxVerticalTargetDistance)
                {
                    _cameraTargetVerticalPositionSmoothed += verticalCompensation + ScreenSizeInWorldCoordinates.y * MaxVerticalTargetDistance;
                    _previousCameraTargetVerticalPositionSmoothed = _cameraTargetVerticalPositionSmoothed;
                }
            }

            // Movement this step
            var horizontalDeltaMovement = _cameraTargetHorizontalPositionSmoothed - Vector3H(_transform.localPosition);
            var verticalDeltaMovement = _cameraTargetVerticalPositionSmoothed - Vector3V(_transform.localPosition);

            // Limit max speed
            if (LimitMaxHorizontalSpeed)
                horizontalDeltaMovement = Mathf.Clamp(horizontalDeltaMovement, -MaxHorizontalSpeed, MaxHorizontalSpeed);
                
            if (LimitMaxVerticalSpeed)
                verticalDeltaMovement = Mathf.Clamp(verticalDeltaMovement, -MaxVerticalSpeed, MaxVerticalSpeed);

            // Apply the delta movement
            _deltaMovement = VectorHV(horizontalDeltaMovement, verticalDeltaMovement);
            _transform.Translate(_deltaMovement, Space.World);
        }

        Vector3 GetTargetsWeightedMidPoint(IList<CameraTarget> targets)
        {
            var midPointH = 0f;
            var midPointV = 0f;
            var totalTargets = targets.Count;

            if (totalTargets == 0)
                return transform.localPosition;

            var totalInfluencesH = 0f;
            var totalInfluencesV = 0f;
            var totalAccountableTargetsH = 0;
            var totalAccountableTargetsV = 0;
            for (int i = 0; i < totalTargets; i++)
            {
                if (targets[i] == null)
                    continue;
                
                midPointH += (Vector3H(targets[i].TargetPosition) + targets[i].TargetOffset.x) * targets[i].TargetInfluenceH;
                midPointV += (Vector3V(targets[i].TargetPosition) + targets[i].TargetOffset.y) * targets[i].TargetInfluenceV;
                    
                totalInfluencesH += targets[i].TargetInfluenceH;
                totalInfluencesV += targets[i].TargetInfluenceV;

                if (targets[i].TargetInfluenceH > 0)
                    totalAccountableTargetsH++;
                
                if (targets[i].TargetInfluenceV > 0)
                    totalAccountableTargetsV++;
            }

            if (totalInfluencesH < 1 && totalAccountableTargetsH == 1)
                totalInfluencesH += (1 - totalInfluencesH);

            if (totalInfluencesV < 1 && totalAccountableTargetsV == 1)
                totalInfluencesV += (1 - totalInfluencesV);

            if (totalInfluencesH > .0001f)
                midPointH /= totalInfluencesH;

            if (totalInfluencesV > .0001f)
                midPointV /= totalInfluencesV;

            return VectorHV(midPointH, midPointV);
        }

        IEnumerator ApplyInfluencesTimedRoutine(IList<Vector2> influences, float[] durations)
        {
            var count = -1;
            while (count < durations.Length - 1)
            {
                count++;
                var duration = durations[count];

                yield return StartCoroutine(ApplyInfluenceTimedRoutine(influences[count], duration));
            }
        }

        IEnumerator ApplyInfluenceTimedRoutine(Vector2 influence, float duration)
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (duration > 0)
            {
                duration -= UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime;

                ApplyInfluence(influence);

                yield return (UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }
        }

        IEnumerator AdjustTargetInfluenceRoutine(CameraTarget cameraTarget, float influenceH, float influenceV, float duration, bool removeIfZeroInfluence = false)
        {
            var startInfluenceH = cameraTarget.TargetInfluenceH;
            var startInfluenceV = cameraTarget.TargetInfluenceV;

            var waitForFixedUpdate = new WaitForFixedUpdate();
            var t = 0f;
            while (t <= 1.0f)
            {
                t += (UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime) / duration;
                cameraTarget.TargetInfluenceH = Utils.EaseFromTo(startInfluenceH, influenceH, t, EaseType.Linear);
                cameraTarget.TargetInfluenceV = Utils.EaseFromTo(startInfluenceV, influenceV, t, EaseType.Linear);

                yield return (UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }

            if (removeIfZeroInfluence && cameraTarget.TargetInfluenceH <= 0 && cameraTarget.TargetInfluenceV <= 0)
                CameraTargets.Remove(cameraTarget);
        }

        IEnumerator UpdateScreenSizeRoutine(float finalSize, float duration, EaseType easeType)
        {
            var startSize = 0f;
            if (GameCamera.orthographic)
            {
                startSize = GameCamera.orthographicSize;

                #if PC2D_TK2D_SUPPORT
                if (Tk2dCam != null)
                    startSize = Tk2dCam.CameraSettings.orthographicSize / Tk2dCam.ZoomFactor;
                #endif
            }
            else
                startSize = Mathf.Abs(Vector3D(CameraPosition)) * Mathf.Tan(GameCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            
            var waitForFixedUpdate = new WaitForFixedUpdate();
            var t = 0f;
            var newSize = startSize;
            while (t <= 1.0f)
            {
                t += (UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime) / duration;
                newSize = Utils.EaseFromTo(startSize, finalSize, t, easeType);

                UpdateScreenSize(newSize);

                yield return (UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }

            _updateScreenSizeCoroutine = null;
        }

        #if UNITY_EDITOR
        int _drawGizmosCounter;

        void OnDrawGizmos()
        {
            // HACK to prevent Unity bug on startup: http://forum.unity3d.com/threads/screen-position-out-of-view-frustum.9918/
            _drawGizmosCounter++;
            if (_drawGizmosCounter < 5 && UnityEditor.EditorApplication.timeSinceStartup < 60f)
                return;

            switch (Axis)
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

            var gameCamera = GetComponent<Camera>();

            // Don't draw gizmos on other cameras
            if (Camera.current != gameCamera &&
                ((UnityEditor.SceneView.lastActiveSceneView != null && Camera.current != UnityEditor.SceneView.lastActiveSceneView.camera) ||
                (UnityEditor.SceneView.lastActiveSceneView == null)))
                return;
            
            var cameraDimensions = Utils.GetScreenSizeInWorldCoords(gameCamera, Mathf.Abs(Vector3D(transform.localPosition)));
            float cameraDepthOffset = Vector3D(transform.localPosition) + Mathf.Abs(Vector3D(transform.localPosition)) * Vector3D(transform.forward);

            // Targets mid point
            Gizmos.color = EditorPrefsX.GetColor(PrefsData.TargetsMidPointColorKey, PrefsData.TargetsMidPointColorValue);
            var targetsMidPoint = GetTargetsWeightedMidPoint(CameraTargets);
            targetsMidPoint = VectorHVD(Vector3H(targetsMidPoint), Vector3V(targetsMidPoint), cameraDepthOffset);
            Gizmos.DrawWireSphere(targetsMidPoint, .01f * cameraDimensions.y);

            // Influences sum
            Gizmos.color = EditorPrefsX.GetColor(PrefsData.InfluencesColorKey, PrefsData.InfluencesColorValue);
            if (_influencesSum != Vector3.zero)
                Utils.DrawArrowForGizmo(targetsMidPoint, _influencesSum, .04f * cameraDimensions.y);

            // Overall offset line
            Gizmos.color = EditorPrefsX.GetColor(PrefsData.OverallOffsetColorKey, PrefsData.OverallOffsetColorValue);
            if (OverallOffset != Vector2.zero)
                Utils.DrawArrowForGizmo(targetsMidPoint, VectorHV(OverallOffset.x, OverallOffset.y), .04f * cameraDimensions.y);

            // Limit cam distance
            if (LimitHorizontalCameraDistance)
            {
                Gizmos.color = EditorPrefsX.GetColor(PrefsData.CamDistanceColorKey, PrefsData.CamDistanceColorValue);
                Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) + (cameraDimensions.x / 2) * MaxHorizontalTargetDistance, Vector3V(transform.position) - cameraDimensions.y / 2, cameraDepthOffset), transform.up * cameraDimensions.y); 
                Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) - (cameraDimensions.x / 2) * MaxHorizontalTargetDistance, Vector3V(transform.position) - cameraDimensions.y / 2, cameraDepthOffset), transform.up * cameraDimensions.y); 
            }

            if (LimitVerticalCameraDistance)
            {
                Gizmos.color = EditorPrefsX.GetColor(PrefsData.CamDistanceColorKey, PrefsData.CamDistanceColorValue);
                Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) - cameraDimensions.x / 2, Vector3V(transform.position) - (cameraDimensions.y / 2) * MaxVerticalTargetDistance, cameraDepthOffset), transform.right * cameraDimensions.x);
                Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) - cameraDimensions.x / 2, Vector3V(transform.position) + (cameraDimensions.y / 2) * MaxVerticalTargetDistance, cameraDepthOffset), transform.right * cameraDimensions.x);
            }

            // Camera target position
            Gizmos.color = EditorPrefsX.GetColor(PrefsData.CamTargetPositionColorKey, PrefsData.CamTargetPositionColorValue);
            var cameraTargetPosition = targetsMidPoint + _influencesSum + VectorHV(OverallOffset.x, OverallOffset.y);
            var cameraTargetPos = VectorHVD(Vector3H(cameraTargetPosition), Vector3V(cameraTargetPosition), cameraDepthOffset);
            Gizmos.DrawWireSphere(cameraTargetPos, .015f * cameraDimensions.y);
            
            // Camera target position smoothed
            if (Application.isPlaying)
            {
                Gizmos.color = EditorPrefsX.GetColor(PrefsData.CamTargetPositionSmoothedColorKey, PrefsData.CamTargetPositionSmoothedColorValue);
                var cameraTargetPosSmoothed = VectorHVD(_cameraTargetHorizontalPositionSmoothed, _cameraTargetVerticalPositionSmoothed, cameraDepthOffset);
                Gizmos.DrawWireSphere(cameraTargetPosSmoothed, .02f * cameraDimensions.y);
                Gizmos.DrawLine(cameraTargetPos, cameraTargetPosSmoothed);
            }

            // Current camera position
            Gizmos.color = EditorPrefsX.GetColor(PrefsData.CurrentCameraPositionColorKey, PrefsData.CurrentCameraPositionColorValue);
            var currentCameraPos = VectorHVD(Vector3H(transform.localPosition), Vector3V(transform.localPosition), cameraDepthOffset);
            Gizmos.DrawWireSphere(currentCameraPos, .025f * cameraDimensions.y);
        }
        #endif
    }
}