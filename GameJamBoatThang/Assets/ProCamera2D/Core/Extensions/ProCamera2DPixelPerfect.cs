using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [ExecuteInEditMode]
    public class ProCamera2DPixelPerfect : BasePC2D
    {
        public float PixelsPerUnit = 32;

        #if PC2D_TK2D_SUPPORT
        public float Tk2DPixelsPerMeter = 32;
        #endif

        public bool ViewportAutoScale = true;

        public Vector2 TargetViewportSizeInPixels = new Vector2(80.0f, 50.0f);

        [Range(1, 32)]
        public int Zoom = 1;

        public bool SnapMovementToGrid;
        public bool DrawGrid;
        public Color GridColor = new Color(1f, 0f, 0f, .1f);
        public float GridDensity;

        public float PixelStep
        {
            get
            { 
                #if UNITY_EDITOR
                if (!Application.isPlaying && _pixelStep < 0)
                    ResizeCameraToPixelPerfect();
                #endif
                return _pixelStep;
            }
        }

        float _pixelStep = -1;

        Transform _parent;

        override protected void Start()
        {
            base.Start();

            #if PC2D_TK2D_SUPPORT
            if (ProCamera2D.Tk2dCam != null && ProCamera2D.Tk2dCam.CameraSettings.projection != tk2dCameraSettings.ProjectionType.Orthographic)
            {
                enabled = false;
                return;
            }
            #else
            if (!ProCamera2D.GameCamera.orthographic)
            {
                enabled = false;
                return;
            }
            #endif

            ResizeCameraToPixelPerfect();
        }

        void LateUpdate()
        {
            if (ProCamera2D.UpdateType == UpdateType.LateUpdate)
                AlignPositionToPixelPerfect();

            #if UNITY_EDITOR
            if (!Application.isPlaying)
                ResizeCameraToPixelPerfect();
            #endif
        }

        void FixedUpdate()
        {
            if (ProCamera2D.UpdateType == UpdateType.FixedUpdate)
                AlignPositionToPixelPerfect();
        }

        /// <summary>
        /// Resizes the camera to a pixel perfect size
        /// </summary>
        public void ResizeCameraToPixelPerfect()
        {
            var viewportScale = CalculateViewportScale();

            CalculatePixelStep(viewportScale);

            var newSize = ((ProCamera2D.GameCamera.pixelHeight * .5f) * (1f / PixelsPerUnit)) / (Zoom * viewportScale);

            ProCamera2D.GameCamera.orthographicSize = newSize;

            ProCamera2D.ScreenSizeInWorldCoordinates = new Vector2(newSize * 2 * ProCamera2D.GameCamera.aspect, newSize * 2);

            #if PC2D_TK2D_SUPPORT
            if (ProCamera2D.Tk2dCam == null)
                return;

            ProCamera2D.Tk2dCam.CameraSettings.orthographicPixelsPerMeter = PixelsPerUnit * Zoom * viewportScale;
            ProCamera2D.Tk2dCam.CameraSettings.orthographicSize = newSize;
            #endif
        }

        public float CalculateViewportScale()
        {
            if (!ViewportAutoScale)
                return Zoom;

            float percentageX = ProCamera2D.GameCamera.pixelWidth / TargetViewportSizeInPixels.x;
            float percentageY = ProCamera2D.GameCamera.pixelHeight / TargetViewportSizeInPixels.y;

//            float percentageX = Screen.width / TargetViewportSizeInPixels.x;
//            float percentageY = Screen.height / TargetViewportSizeInPixels.y;

            Debug.Log("X: " + percentageX + " Y: " + percentageY);

            float viewportScale = percentageX > percentageY ? percentageY : percentageX;
            viewportScale = Mathf.FloorToInt(viewportScale);
            if (viewportScale < 1)
                viewportScale = 1;
            
            return viewportScale;
        }

        void CalculatePixelStep(float viewportScale)
        {
            _pixelStep = SnapMovementToGrid ? 1f / PixelsPerUnit : 1f / (PixelsPerUnit * viewportScale * Zoom);
        }

        void AlignPositionToPixelPerfect()
        {
            // If shaking
            _parent = _transform.parent;
            if (_parent != null && _parent.position != Vector3.zero)
                _parent.position = VectorHVD(Utils.AlignToGrid(Vector3H(_parent.position), _pixelStep), Utils.AlignToGrid(Vector3V(_parent.position), _pixelStep), Vector3D(_parent.position));
            
            _transform.localPosition = VectorHVD(Utils.AlignToGrid(Vector3H(_transform.localPosition), _pixelStep), Utils.AlignToGrid(Vector3V(_transform.localPosition), _pixelStep), Vector3D(_transform.localPosition));
        }

        #if UNITY_EDITOR
        override protected void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (DrawGrid)
            {
                Gizmos.color = GridColor;
                var gridW = ProCamera2D.ScreenSizeInWorldCoordinates.x / 2;
                var gridH = ProCamera2D.ScreenSizeInWorldCoordinates.y / 2;

                float step = 1 / PixelsPerUnit;

                GridDensity = ProCamera2D.GameCamera.pixelWidth / (gridW * 2 / step);
                if (GridDensity < 4)
                    return;

                Vector3 origin = transform.localPosition + 1 * transform.forward - VectorHV(gridW, -gridH);
                origin = VectorHVD(Utils.AlignToGrid(Vector3H(origin), step), Utils.AlignToGrid(Vector3V(origin), step), Vector3D(origin));

                for (float i = 0; i <= 2 * gridW; i += step)
                {
                    Gizmos.DrawLine(origin + VectorHV(i, 0), origin + VectorHV(i, -2 * gridH));
                }

                for (float j = 0; j <= 2 * gridH; j += step)
                {
                    Gizmos.DrawLine(origin + VectorHV(0, -j), origin + VectorHV(2 * gridW, -j));
                }
            }
        }
        #endif
    }
}