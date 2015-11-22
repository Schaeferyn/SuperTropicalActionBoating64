using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [System.Serializable]
    public struct ShakePreset
    {
        public string Name;
        public Vector3 Strength;
        public float Duration;
        public int Vibrato;
        public float Randomness;
        public float Smoothness;
        public float InitialAngle;
        public Vector3 Rotation;
    }

    public class ProCamera2DShake : BasePC2D
    {
        static ProCamera2DShake _instance;

        public static ProCamera2DShake Instance
        {
            get
            {
                if (Equals(_instance, null))
                {
                    _instance = FindObjectOfType(typeof(ProCamera2DShake)) as ProCamera2DShake;

                    if (Equals(_instance, null))
                        throw new UnityException("ProCamera2D does not have a Shake extension.");
                }

                return _instance;
            }
        }

        public Vector2 Strength = new Vector2(10, 10);

        [RangeAttribute(.02f, 3)]
        public float Duration = .5f;

        [RangeAttribute(1, 100)]
        public int Vibrato = 10;

        [RangeAttribute(0, 1)]
        public float Randomness = .1f;

        [RangeAttribute(0, .5f)]
        public float Smoothness = .1f;

        [RangeAttribute(0f, 360f)]
        public float InitialAngle = 0f;

        public bool UseRandomInitialAngle = true;

        public Vector3 Rotation;

        public List<ShakePreset> ShakePresets;

        Transform _shakeParent;

        List<Coroutine> _applyInfluencesCoroutines = new List<Coroutine>();
        Coroutine _shakeCoroutine;

        Vector3 _shakePosition;
        Vector3 _shakePositionSmoothed;
        Vector3 _shakeVelocity;
        List<Vector3> _shakePositions = new List<Vector3>();

        Quaternion _rotationTarget;
        Quaternion _rotationTargetSmoothed;
        Quaternion _originalRotation;
        float _rotationTime;
        float _rotationVelocity;

        void Awake()
        {
            _instance = this;
        }

        protected override void Start()
        {
            base.Start();

            _shakeParent = ProCamera2D.transform.parent = new GameObject("ProCamera2DShakeContainer").transform;
            _originalRotation = _transform.localRotation;
        }

        /// <summary>Shakes the camera position along its horizontal and vertical axes with the values set on the editor.</summary>
        public void Shake()
        {
            Shake(Duration, Strength, Vibrato, Randomness, UseRandomInitialAngle ? -1f : InitialAngle, Rotation, Smoothness);
        }

        /// <summary>Shakes the camera along its horizontal and vertical axes with the given values.</summary>
        /// <param name="duration">The duration of the shake</param>
        /// <param name="strength">The shake strength on each axis</param>
        /// <param name="vibrato">Indicates how much will the shake vibrate</param>
        /// <param name="randomness">Indicates how much random the shake will be</param>
        /// <param name="initialAngle">The initial angle of the shake. Use -1 if you want it to be random.</param>
        /// <param name="rotation">The maximum rotation the camera can reach during shake</param>
        /// <param name="smoothness">How smooth the shake should be, 0 being instant</param>
        public void Shake(float duration, Vector2 strength, int vibrato = 10, float randomness = .1f, float initialAngle = -1f, Vector3 rotation = default(Vector3), float smoothness = .1f)
        {
            vibrato++;
            if (vibrato < 2)
                vibrato = 2;

            // Calculate steps durations
            float[] durations = new float[vibrato];
            float sum = 0;
            for (int i = 0; i < vibrato; ++i)
            {
                float iterationPerc = (i + 1) / (float)vibrato;
                float tDuration = duration * iterationPerc;
                sum += tDuration;
                durations[i] = tDuration;
            }
            float tDurationMultiplier = duration / sum;
            for (int i = 0; i < vibrato; ++i)
                durations[i] = durations[i] * tDurationMultiplier;

            float shakeMagnitude = strength.magnitude;
            float magnitudeDecay = shakeMagnitude / vibrato;

            float ang = initialAngle != -1f ? initialAngle : Random.Range(0f, 360f);
            var positions = new Vector2[vibrato];
            positions[vibrato - 1] = Vector2.zero;
            var rotations = new Quaternion[vibrato];
            rotations[vibrato - 1] = _originalRotation;
            var rotationQtn = Quaternion.Euler(rotation);
            for (int i = 0; i < vibrato - 1; ++i)
            {
                // Position
                if (i > 0)
                    ang = ang - 180 + Random.Range(-90, 90) * randomness;

                Quaternion rndQuaternion = Quaternion.AngleAxis(Random.Range(-90, 90) * randomness, Vector3.up);

                float radians = ang * Mathf.Deg2Rad;
                var dir = new Vector3(shakeMagnitude * Mathf.Cos(radians), shakeMagnitude * Mathf.Sin(radians), 0);

                Vector2 position = rndQuaternion * dir;
                position.x = Vector2.ClampMagnitude(position, strength.x).x;
                position.y = Vector2.ClampMagnitude(position, strength.y).y;
                positions[i] = position;

                shakeMagnitude -= magnitudeDecay;
                strength = Vector2.ClampMagnitude(strength, shakeMagnitude);

                // Rotation
                var sign = i % 2 == 0 ? 1 : -1;
                var percent = (float)i / (vibrato - 1);
                rotations[i] = sign == 1 ? Quaternion.Lerp(rotationQtn, Quaternion.identity, percent) * _originalRotation : Quaternion.Inverse(Quaternion.Lerp(rotationQtn, Quaternion.identity, percent)) * _originalRotation;
            }

            _applyInfluencesCoroutines.Add(ApplyShakesTimed(positions, rotations, durations, smoothness));
        }

        /// <summary>Shakes the camera using the values defined on the provided preset</summary>
        /// <param name="presetIndex">The index of the preset</param>
        public void ShakeUsingPreset(int presetIndex)
        {
            if (presetIndex <= ShakePresets.Count - 1)
            {
                Shake(
                    ShakePresets[presetIndex].Duration, 
                    ShakePresets[presetIndex].Strength, 
                    ShakePresets[presetIndex].Vibrato, 
                    ShakePresets[presetIndex].Randomness, 
                    ShakePresets[presetIndex].InitialAngle, 
                    ShakePresets[presetIndex].Rotation, 
                    ShakePresets[presetIndex].Smoothness);
            }
            else
            {
                Debug.LogWarning("Could not find a shake preset with the index: " + presetIndex);
            }
        }

        /// <summary>Shakes the camera using the values defined on the provided preset</summary>
        /// <param name="presetName">The name of the preset</param>
        public void ShakeUsingPreset(string presetName)
        {
            for (int i = 0; i < ShakePresets.Count; i++)
            {
                if (ShakePresets[i].Name == presetName)
                {
                    Shake(
                        ShakePresets[i].Duration, 
                        ShakePresets[i].Strength, 
                        ShakePresets[i].Vibrato, 
                        ShakePresets[i].Randomness, 
                        ShakePresets[i].InitialAngle, 
                        ShakePresets[i].Rotation, 
                        ShakePresets[i].Smoothness);

                    return;
                }
            }

            Debug.LogWarning("Could not find a shake preset with the name: " + presetName);
        }

        /// <summary>Stops all current shakes</summary>
        public void StopShaking()
        {
            for (int i = 0; i < _applyInfluencesCoroutines.Count; i++)
            {
                StopCoroutine(_applyInfluencesCoroutines[i]);
            }
            _shakePositions.Clear();
        }

        /// <summary>Apply the given influences to the camera during the corresponding durations.</summary>
        /// <param name="shakes">An array of the vectors representing the shakes to be applied</param>
        /// <param name="rotations">An array of the rotations to be applied</param>
        /// <param name="durations">An array with the durations of the influences to be applied</param>
        /// <param name="smoothness">How smooth the shake should be, 0 being instant</param>
        public Coroutine ApplyShakesTimed(Vector2[] shakes, Vector3[] rotations, float[] durations, float smoothness = .1f)
        {
            var rotationsQtn = new Quaternion[rotations.Length];
            for (int i = 0; i < rotations.Length; i++)
                rotationsQtn[i] = Quaternion.Euler(rotations[i]) * _originalRotation;

            return ApplyShakesTimed(shakes, rotationsQtn, durations);
        }

        Coroutine ApplyShakesTimed(Vector2[] shakes, Quaternion[] rotations, float[] durations, float smoothness = .1f)
        {
            var coroutine = StartCoroutine(ApplyShakesTimedRoutine(shakes, rotations, durations));

            if (_shakeCoroutine == null)
                _shakeCoroutine = StartCoroutine(ShakeRoutine(smoothness));

            return coroutine;
        }

        IEnumerator ShakeRoutine(float smoothness)
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (_shakePositions.Count > 0 || _shakeParent.position != Vector3.zero || _transform.localRotation != _originalRotation)
            {
                _shakePosition = Utils.GetVectorsSum(_shakePositions);
                _shakePositionSmoothed = Vector3.SmoothDamp(_shakePositionSmoothed, _shakePosition, ref _shakeVelocity, smoothness);
                _shakeParent.position = _shakePositionSmoothed;
                _shakePositions.Clear();

                _rotationTime = Mathf.SmoothDamp(_rotationTime, 1f, ref _rotationVelocity, smoothness);
                _rotationTargetSmoothed = Quaternion.Slerp(_rotationTargetSmoothed, _rotationTarget, _rotationTime);

                _transform.localRotation = _rotationTargetSmoothed;
                _rotationTarget = _originalRotation;

                yield return (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }

            _shakeCoroutine = null;
        }

        IEnumerator ApplyShakesTimedRoutine(IList<Vector2> shakes, IList<Quaternion> rotations, float[] durations)
        {
            var count = -1;
            while (count < durations.Length - 1)
            {
                count++;
                var duration = durations[count];

                yield return StartCoroutine(ApplyShakeTimedRoutine(shakes[count], rotations[count], duration));
            }
        }

        IEnumerator ApplyShakeTimedRoutine(Vector2 shake, Quaternion rotation, float duration)
        {
            _rotationTime = 0;
            _rotationVelocity = 0;
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (duration > 0)
            {
                duration -= ProCamera2D.UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime;

                _shakePositions.Add(VectorHV(shake.x, shake.y));

                _rotationTarget = rotation;

                yield return (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }
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
            
            var cameraDimensions = Utils.GetScreenSizeInWorldCoords(ProCamera2D.GameCamera, Mathf.Abs(Vector3D(transform.localPosition)));

            // Shake influence
            if (_shakePositionSmoothed != Vector3.zero)
            {
                Gizmos.color = EditorPrefsX.GetColor(PrefsData.ShakeInfluenceColorKey, PrefsData.ShakeInfluenceColorValue);
                Utils.DrawArrowForGizmo(ProCamera2D.TargetsMidPoint, _shakePositionSmoothed, .04f * cameraDimensions.y);
            }
        }
        #endif
    }
}