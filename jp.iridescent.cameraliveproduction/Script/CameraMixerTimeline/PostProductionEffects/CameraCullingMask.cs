using System;
using UnityEngine;
// using UnityEngine.Rendering.HighDefinition;

namespace CameraLiveProduction
{
    // [Serializable]
    // public class CameraCullingMask:CameraPostProductionBase
    // {
    //     // [HideInInspector]public Camera camera;
    //     public LayerMask cullingMask = -1;
    //     // private HDAdditionalCameraData hdAdditionalCameraData = null;
    //     public override void UpdateEffect(LiveCameraBase liveCamera, float time,float weight = 1f)
    //     {
    //         // if(liveCamera.TargetCamera == null)
    //         //     return;
    //         #if USE_HDRP
    //         // if(hdAdditionalCameraData == null) hdAdditionalCameraData= camera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
    //         liveCamera.TargetCamera.cullingMask = cullingMask;
    //         #endif
    //     }
    //
    //
    //     public override void OnClipDisable(LiveCameraBase liveCamera)
    //     {
    //     }
    //
    //     public override void Initialize(LiveCameraBase liveCamera)
    //     {
    //         cullingMask = liveCamera.GetLayerMask();
    //     }
    //     
    //     public override void OnDestroy(LiveCameraBase liveCamera)
    //     {
    //     }
    //     
    // }
}