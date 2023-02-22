using System;
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
            
            foreach (var liveCamera in cameraMixer.cameraList)
            {
                
                if (liveCamera != null && liveCamera.cinemachineVolumeForceLayerChange != null)
                {
                   if(liveCamera.cinemachineVolumeForceLayerChange.volume)liveCamera.cinemachineVolumeForceLayerChange.volume.enabled = false;
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
            volumeLayerMask=CameraLayerUtility.Add(
                volumeLayerMask,
                isCam1 ? cameraALayerID : cameraBLayerID);
            volumeLayerMask=CameraLayerUtility.Remove(
                volumeLayerMask,
                isCam1 ? cameraBLayerID : cameraALayerID);
            hdAdditionalCameraData.volumeLayerMask = volumeLayerMask;
            
#endif
             var cullingMask = liveCamera.TargetCamera.cullingMask;
             cullingMask = CameraLayerUtility.Add(
                 cullingMask,
                 isCam1 ? cameraALayerID : cameraBLayerID);
             cullingMask= CameraLayerUtility.Remove(
                 cullingMask,
                 isCam1 ? cameraBLayerID : cameraALayerID);
             liveCamera.TargetCamera.cullingMask = cullingMask;
//            

            if (liveCamera != null && liveCamera.cinemachineVolumeForceLayerChange != null)
            {
                if(liveCamera.cinemachineVolumeForceLayerChange.volume)liveCamera.cinemachineVolumeForceLayerChange.volume.enabled = true;
            }
        }

    }
}