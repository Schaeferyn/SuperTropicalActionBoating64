using UnityEngine;
using System.Collections;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class ProCamera2DGeometryBoundaries : BasePC2D
    {
        [Tooltip("The layer that contains the (3d) colliders that define the boundaries for the camera")]
        public LayerMask BoundariesLayerMask;

        MoveInColliderBoundaries _cameraMoveInColliderBoundaries;

        override protected void Start()
        {
            base.Start();

            _cameraMoveInColliderBoundaries = new MoveInColliderBoundaries(ProCamera2D);
            _cameraMoveInColliderBoundaries.CameraTransform = _transform;
            _cameraMoveInColliderBoundaries.CameraSize = ProCamera2D.ScreenSizeInWorldCoordinates;
            _cameraMoveInColliderBoundaries.CameraCollisionMask = BoundariesLayerMask;
        }

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
            _cameraMoveInColliderBoundaries.CameraSize = ProCamera2D.ScreenSizeInWorldCoordinates;

            // Remove the delta movement
            _transform.Translate(-ProCamera2D.DeltaMovement, Space.World);

            // Apply movement considering the collider boundaries
            _transform.Translate(_cameraMoveInColliderBoundaries.Move(ProCamera2D.DeltaMovement), Space.World);
        }
    }
}