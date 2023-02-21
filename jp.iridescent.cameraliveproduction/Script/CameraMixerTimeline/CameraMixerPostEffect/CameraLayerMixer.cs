﻿using System;
using UnityEngine;
using UnityEngine.Rendering;


#if USE_HDRP

using UnityEngine.Rendering.HighDefinition;

#endif

#if USE_CINEMACHINE
using Cinemachine;
#endif


namespace CameraLiveProduction
{
    [RequireComponent(typeof(CameraMixer))]
    [ExecuteAlways]
    public class CameraLayerMixer: CameraMixerPostEffectBase
    {
        [SerializeField]private int cameraALayerID = 20;
        [SerializeField]private int cameraBLayerID = 21;
        
        public LayerMask layerMaskA;
        public LayerMask layerMaskB;
        
        // public LayerMask LayerMaskA => LayerMask.GetMask( LayerMask.LayerToName(layerCameraA));
        // public LayerMask LayerMaskB => LayerMask.GetMask( LayerMask.LayerToName(layerCameraB));

// #if USE_HDRP
//         public HDAdditionalCameraData hdAdditionalCameraDataA;
//         public HDAdditionalCameraData hdAdditionalCameraDataB;
// #endif
        public override void Init(CameraMixer cameraMixer)
        {
            base.Init(cameraMixer);
            
            
            
        }

        private void OnEnable()
        {
            Init(GetComponent<CameraMixer>());
        }

        public override void UpdateEffect()
        {
            
            for (int i = 0; i < 32; ++i)
            {
                if ((layerMaskA & (1 << i)) != 0)
                {
                    cameraALayerID = i;
                    break;
                }
            }

            for (int i = 0; i < 32; ++i)
            {
                if ((layerMaskB & (1 << i)) != 0)
                {
                    cameraBLayerID = i;
                    break;
                }
            }

            if (cameraMixer.cam1)
             {
                 SetLayer(cameraMixer.cam1, true);
                 
             }
             
             if(cameraMixer.cam2)
             {
                 SetLayer(cameraMixer.cam2, false);
             }
        }

        
        private void SetLayer(LiveCamera liveCamera, bool isCam1)
        {

            liveCamera.gameObject.layer = isCam1 ? cameraALayerID : cameraBLayerID;
#if USE_HDRP

            HDAdditionalCameraData hdAdditionalCameraData = liveCamera.hdAdditionalCameraData;
            if(hdAdditionalCameraData ==null) liveCamera.Initialize();
            var volumeLayerMask = hdAdditionalCameraData.volumeLayerMask;
            CameraLayerUtility.Add(
                volumeLayerMask,
                isCam1 ? layerMaskA : layerMaskB);
            CameraLayerUtility.Remove(
                volumeLayerMask,
                isCam1 ? layerMaskB : layerMaskA);
            hdAdditionalCameraData.volumeLayerMask = volumeLayerMask;
            
#endif
//             var cullingMask = liveCamera.TargetCamera.cullingMask;
//             liveCamera.TargetCamera.cullingMask = CameraLayerUtility.Add(
//                 cullingMask,
//                 isCam1 ? layerMaskA : layerMaskB);
//             cullingMask = liveCamera.TargetCamera.cullingMask;
//             liveCamera.TargetCamera.cullingMask = CameraLayerUtility.Remove(
//                 cullingMask,
//                 isCam1 ? layerMaskB : layerMaskA);
//            
        }

    }
}