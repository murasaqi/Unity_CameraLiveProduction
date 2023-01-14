using System;
using UnityEngine;
// using UnityEngine.Rendering.HighDefinition;

namespace CameraLiveProduction
{
    [Serializable]
    public class CameraCullingMask:CameraPostProductionBase
    {
        // [HideInInspector]public Camera camera;
        public LayerMask cullingMask = -1;
        // private HDAdditionalCameraData hdAdditionalCameraData = null;
        public override void UpdateEffect(LiveCamera liveCamera, float time)
        {
            if(liveCamera.TargetCamera == null)
                return;
            #if USE_HDRP
            // if(hdAdditionalCameraData == null) hdAdditionalCameraData= camera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
            liveCamera.TargetCamera.cullingMask = cullingMask;
            #endif
        }
        
        public override void Initialize(LiveCamera liveCamera)
        {
            cullingMask = liveCamera.TargetCamera.cullingMask;
        }
        
        public override void OnDestroy(LiveCamera liveCamera)
        {
        }
        
    }
}