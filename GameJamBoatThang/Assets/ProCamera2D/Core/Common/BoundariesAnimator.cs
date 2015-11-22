using UnityEngine;
using System;
using System.Collections;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class BoundariesAnimator
    {
        public Action OnTransitionStarted;
        public Action OnTransitionFinished;

        public bool UseTopBoundary;
        public float TopBoundary;
        public bool UseBottomBoundary;
        public float BottomBoundary;
        public bool UseLeftBoundary;
        public float LeftBoundary;
        public bool UseRightBoundary;
        public float RightBoundary;

        public float TransitionDuration = 1f;
        public EaseType TransitionEaseType;

        public int AnimsCount;

        ProCamera2D ProCamera2D;
        ProCamera2DNumericBoundaries NumericBoundaries;

        Func<Vector3, float> Vector3H;
        Func<Vector3, float> Vector3V;

        bool _hasFiredTransitionFinished;

        public BoundariesAnimator(ProCamera2D proCamera2D, ProCamera2DNumericBoundaries numericBoundaries)
        {
            ProCamera2D = proCamera2D;
            NumericBoundaries = numericBoundaries;

            switch (ProCamera2D.Axis)
            {
                case MovementAxis.XY:
                    Vector3H = vector => vector.x;
                    Vector3V = vector => vector.y;
                    break;
                case MovementAxis.XZ:
                    Vector3H = vector => vector.x;
                    Vector3V = vector => vector.z;
                    break;
                case MovementAxis.YZ:
                    Vector3H = vector => vector.z;
                    Vector3V = vector => vector.y;
                    break;
            }
        }

        public void Transition()
        {
            if (OnTransitionStarted != null)
                OnTransitionStarted();

            _hasFiredTransitionFinished = false;

            NumericBoundaries.UseNumericBoundaries = true;

            // Animate boundaries
            AnimsCount = 0;
            if (UseLeftBoundary)
            {
                NumericBoundaries.UseLeftBoundary = true;

                if (NumericBoundaries.LeftBoundaryAnimRoutine != null)
                    NumericBoundaries.StopCoroutine(NumericBoundaries.LeftBoundaryAnimRoutine);

                NumericBoundaries.LeftBoundaryAnimRoutine = NumericBoundaries.StartCoroutine(LeftTransitionRoutine(TransitionDuration));
                AnimsCount++;
            }
            else if (!UseLeftBoundary && NumericBoundaries.UseLeftBoundary && UseRightBoundary && RightBoundary < NumericBoundaries.TargetLeftBoundary) 
            {
                NumericBoundaries.UseLeftBoundary = true;
                UseLeftBoundary = true;

                LeftBoundary = RightBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.x * 100f;

                if (NumericBoundaries.LeftBoundaryAnimRoutine != null)
                    NumericBoundaries.StopCoroutine(NumericBoundaries.LeftBoundaryAnimRoutine);

                NumericBoundaries.LeftBoundaryAnimRoutine = NumericBoundaries.StartCoroutine(LeftTransitionRoutine(TransitionDuration, true));
                AnimsCount++;
            }

            if (UseRightBoundary)
            {
                NumericBoundaries.UseRightBoundary = true;

                if (NumericBoundaries.RightBoundaryAnimRoutine != null)
                    NumericBoundaries.StopCoroutine(NumericBoundaries.RightBoundaryAnimRoutine);

                NumericBoundaries.RightBoundaryAnimRoutine = NumericBoundaries.StartCoroutine(RightTransitionRoutine(TransitionDuration));
                AnimsCount++;
            }
            else if (!UseRightBoundary && NumericBoundaries.UseRightBoundary && UseLeftBoundary && LeftBoundary > NumericBoundaries.TargetRightBoundary) 
            {
                NumericBoundaries.UseRightBoundary = true;
                UseRightBoundary = true;

                RightBoundary = LeftBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.x * 100f;

                if (NumericBoundaries.RightBoundaryAnimRoutine != null)
                    NumericBoundaries.StopCoroutine(NumericBoundaries.RightBoundaryAnimRoutine);

                NumericBoundaries.RightBoundaryAnimRoutine = NumericBoundaries.StartCoroutine(RightTransitionRoutine(TransitionDuration, true));
                AnimsCount++;
            }

            if (UseTopBoundary)
            {
                NumericBoundaries.UseTopBoundary = true;

                if (NumericBoundaries.TopBoundaryAnimRoutine != null)
                    NumericBoundaries.StopCoroutine(NumericBoundaries.TopBoundaryAnimRoutine);

                NumericBoundaries.TopBoundaryAnimRoutine = NumericBoundaries.StartCoroutine(TopTransitionRoutine(TransitionDuration));
                AnimsCount++;
            }
            else if (!UseTopBoundary && NumericBoundaries.UseTopBoundary && UseBottomBoundary && BottomBoundary > NumericBoundaries.TargetTopBoundary) 
            {
                NumericBoundaries.UseTopBoundary = true;
                UseTopBoundary = true;

                TopBoundary = BottomBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.y * 100f;

                if (NumericBoundaries.TopBoundaryAnimRoutine != null)
                    NumericBoundaries.StopCoroutine(NumericBoundaries.TopBoundaryAnimRoutine);

                NumericBoundaries.TopBoundaryAnimRoutine = NumericBoundaries.StartCoroutine(TopTransitionRoutine(TransitionDuration, true));
                AnimsCount++;
            }

            if (UseBottomBoundary)
            {
                NumericBoundaries.UseBottomBoundary = true;

                if (NumericBoundaries.BottomBoundaryAnimRoutine != null)
                    NumericBoundaries.StopCoroutine(NumericBoundaries.BottomBoundaryAnimRoutine);

                NumericBoundaries.BottomBoundaryAnimRoutine = NumericBoundaries.StartCoroutine(BottomTransitionRoutine(TransitionDuration));
                AnimsCount++;
            }
            else if (!UseBottomBoundary && NumericBoundaries.UseBottomBoundary && UseTopBoundary && TopBoundary < NumericBoundaries.TargetBottomBoundary) 
            {
                NumericBoundaries.UseBottomBoundary = true;
                UseBottomBoundary = true;

                BottomBoundary = TopBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.y * 100f;

                if (NumericBoundaries.BottomBoundaryAnimRoutine != null)
                    NumericBoundaries.StopCoroutine(NumericBoundaries.BottomBoundaryAnimRoutine);

                NumericBoundaries.BottomBoundaryAnimRoutine = NumericBoundaries.StartCoroutine(BottomTransitionRoutine(TransitionDuration, true));
                AnimsCount++;
            }
        }

        IEnumerator LeftTransitionRoutine(float duration, bool turnOffBoundaryAfterwards = false)
        {
            var initialLeftBoundary = Vector3H(ProCamera2D.CameraPosition) - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2;

            NumericBoundaries.TargetLeftBoundary = LeftBoundary;

            var waitForFixedUpdate = new WaitForFixedUpdate();
            var t = 0f;
            while (t <= 1.0f)
            {
                t += (ProCamera2D.UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime) / duration;

                // Move left
                if (UseLeftBoundary && UseRightBoundary && LeftBoundary < initialLeftBoundary)
                {
                    NumericBoundaries.LeftBoundary = LeftBoundary;
                }
                // Move right
                else if (UseLeftBoundary)
                {
                    NumericBoundaries.LeftBoundary = Utils.EaseFromTo(initialLeftBoundary, LeftBoundary, t, TransitionEaseType);

                    var currentCamLeftEdge = Vector3H(ProCamera2D.CameraPosition) - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2;
                    if (currentCamLeftEdge < NumericBoundaries.TargetLeftBoundary &&
                        NumericBoundaries.LeftBoundary < currentCamLeftEdge)
                        NumericBoundaries.LeftBoundary = currentCamLeftEdge;
                }

                yield return (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }

            if (turnOffBoundaryAfterwards)
            {
                NumericBoundaries.UseLeftBoundary = false;
                UseLeftBoundary = false;
            }

            if (!_hasFiredTransitionFinished && OnTransitionFinished != null)
                OnTransitionFinished();
        }

        IEnumerator RightTransitionRoutine(float duration, bool turnOffBoundaryAfterwards = false)
        {
            var initialRightBoundary = Vector3H(ProCamera2D.CameraPosition) + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2;

            NumericBoundaries.TargetRightBoundary = RightBoundary;

            var waitForFixedUpdate = new WaitForFixedUpdate();
            var t = 0f;
            while (t <= 1.0f)
            {
                t += (ProCamera2D.UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime) / duration;

                // Move right
                if (UseRightBoundary && UseLeftBoundary && RightBoundary > initialRightBoundary)
                {
                    NumericBoundaries.RightBoundary = RightBoundary;
                }
                // Move left
                else if (UseRightBoundary)
                {
                    NumericBoundaries.RightBoundary = Utils.EaseFromTo(initialRightBoundary, RightBoundary, t, TransitionEaseType);

                    var currentCamRightEdge = Vector3H(ProCamera2D.CameraPosition) + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2;
                    if (currentCamRightEdge > NumericBoundaries.TargetRightBoundary &&
                        NumericBoundaries.RightBoundary > currentCamRightEdge)
                        NumericBoundaries.RightBoundary = currentCamRightEdge;
                }

                yield return (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }

            if (turnOffBoundaryAfterwards)
            {
                NumericBoundaries.UseRightBoundary = false;
                UseRightBoundary = false;
            }

            if (!_hasFiredTransitionFinished && OnTransitionFinished != null)
                OnTransitionFinished();
        }

        IEnumerator TopTransitionRoutine(float duration, bool turnOffBoundaryAfterwards = false)
        {
            var initialTopBoundary = Vector3V(ProCamera2D.CameraPosition) + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2;

            NumericBoundaries.TargetTopBoundary = TopBoundary;

            var waitForFixedUpdate = new WaitForFixedUpdate();
            var t = 0f;
            while (t <= 1.0f)
            {
                t += (ProCamera2D.UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime) / duration;

                // Move up
                if (UseTopBoundary && UseBottomBoundary && TopBoundary > initialTopBoundary)
                {
                    NumericBoundaries.TopBoundary = TopBoundary;
                }
                // Move down
                else if (UseTopBoundary)
                {
                    NumericBoundaries.TopBoundary = Utils.EaseFromTo(initialTopBoundary, TopBoundary, t, TransitionEaseType);

                    var currentCamTopEdge = Vector3V(ProCamera2D.CameraPosition) + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2;
                    if (currentCamTopEdge > NumericBoundaries.TargetTopBoundary &&
                        NumericBoundaries.TopBoundary > currentCamTopEdge)
                        NumericBoundaries.TopBoundary = currentCamTopEdge;
                }

                yield return (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }

            if (turnOffBoundaryAfterwards)
            {
                NumericBoundaries.UseTopBoundary = false;
                UseTopBoundary = false;
            }

            if (!_hasFiredTransitionFinished && OnTransitionFinished != null)
                OnTransitionFinished();
        }

        IEnumerator BottomTransitionRoutine(float duration, bool turnOffBoundaryAfterwards = false)
        {
            var initialBottomBoundary = Vector3V(ProCamera2D.CameraPosition) - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2;

            NumericBoundaries.TargetBottomBoundary = BottomBoundary;

            var waitForFixedUpdate = new WaitForFixedUpdate();
            var t = 0f;
            while (t <= 1.0f)
            {
                t += (ProCamera2D.UpdateType == UpdateType.LateUpdate ? Time.deltaTime : Time.fixedDeltaTime) / duration;

                // Move down
                if (UseBottomBoundary && UseTopBoundary && BottomBoundary < initialBottomBoundary)
                {
                    NumericBoundaries.BottomBoundary = BottomBoundary;
                }
                // Move up
                else if (UseBottomBoundary)
                {
                    NumericBoundaries.BottomBoundary = Utils.EaseFromTo(initialBottomBoundary, BottomBoundary, t, TransitionEaseType);

                    var currentCamBottomEdge = Vector3V(ProCamera2D.CameraPosition) - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2;
                    if (currentCamBottomEdge < NumericBoundaries.TargetBottomBoundary &&
                        NumericBoundaries.BottomBoundary < currentCamBottomEdge)
                        NumericBoundaries.BottomBoundary = currentCamBottomEdge;
                }

                yield return (ProCamera2D.UpdateType == UpdateType.FixedUpdate) ? waitForFixedUpdate : null;
            }

            if (turnOffBoundaryAfterwards)
            {
                NumericBoundaries.UseBottomBoundary = false;
                UseBottomBoundary = false;
            }

            if (!_hasFiredTransitionFinished && OnTransitionFinished != null)
                OnTransitionFinished();
        }
    }
}