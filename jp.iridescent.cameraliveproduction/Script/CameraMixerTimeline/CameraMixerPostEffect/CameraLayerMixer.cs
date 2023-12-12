using System;
using UnityEngine;
using UnityEngine.Rendering;


#if USE_HDRP

using UnityEngine.Rendering.HighDefinition;
#elif USE_URP
using UnityEngine.Rendering.Universal;

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

        [ContextMenu("Force Initialize")]
        public void ForceInitialize()
        {
            Init(GetComponent<CameraMixer>());
        }
        public override void Init(CameraMixer cameraMixer)
        {
            base.Init(cameraMixer);

            foreach (var liveCameraBase in cameraMixer.cameraList)
            {
                if (liveCameraBase.GetType() == typeof(LiveCamera))
                {
                    var liveCamera = (LiveCamera)liveCameraBase;


                    if (liveCamera == null) continue;
#if USE_CINEMACHINE
                    if (liveCamera.cinemachineBrain != null)
                    {
                        // destroy brain child objects
                        foreach (Transform child in liveCamera.cinemachineBrain.transform)
                        {
                            if (child != liveCamera.cinemachineBrain.transform) DestroyImmediate(child.gameObject);
                        }
                    }
#endif
                    if (liveCamera.cinemachineVolumeForceLayerChange)
                    {
                        liveCamera.cinemachineVolumeForceLayerChange.Init();
                    }

                }
            }
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
            
            foreach (var liveCameraBase in cameraMixer.cameraList)
            {
                if (liveCameraBase.GetType() == typeof(LiveCamera))
                {
                    var liveCamera = (LiveCamera) liveCameraBase;
                    if (liveCamera != null && liveCamera.cinemachineVolumeForceLayerChange != null)
                    {
                        if(liveCamera.cinemachineVolumeForceLayerChange)liveCamera.cinemachineVolumeForceLayerChange.SetEnable(false);
                    }
                }
                
            }
            if (cameraMixer.cam1 && cameraMixer.cam1.GetType() == typeof(LiveCamera))
             {
                 var liveCamera = (LiveCamera) cameraMixer.cam1;
                 SetLayer(liveCamera, true);
             }
            if(cameraMixer.cam2 && cameraMixer.cam2.GetType() == typeof(LiveCamera))
            {
                var liveCamera = (LiveCamera) cameraMixer.cam2;
                SetLayer(liveCamera, false);
            }  
             
        }


        private void SetLayer(LiveCamera liveCamera, bool isCam1)
        {

            var currentLayerID= isCam1 ? cameraALayerID : cameraBLayerID;
            if (currentLayerID !=liveCamera.gameObject.layer) liveCamera.gameObject.layer = currentLayerID;
#if USE_HDRP

            HDAdditionalCameraData hdAdditionalCameraData = liveCamera.hdAdditionalCameraData;
            if(hdAdditionalCameraData ==null) liveCamera.Initialize();
            var volumeLayerMask = hdAdditionalCameraData.volumeLayerMask;
            volumeLayerMask = CameraLayerUtility.Add(
                volumeLayerMask,
                isCam1 ? cameraALayerID : cameraBLayerID);
            volumeLayerMask = CameraLayerUtility.Remove(
                volumeLayerMask,
                isCam1 ? cameraBLayerID : cameraALayerID);
            hdAdditionalCameraData.volumeLayerMask = volumeLayerMask;

#elif USE_URP

            UniversalAdditionalCameraData universalAdditionalCameraData = liveCamera.universalAdditionalCameraData;
            if(universalAdditionalCameraData ==null) liveCamera.Initialize();
            if (universalAdditionalCameraData == null) return;
            var volumeLayerMask = universalAdditionalCameraData.volumeLayerMask;
            volumeLayerMask = CameraLayerUtility.Add(
                volumeLayerMask,
                isCam1 ? cameraALayerID : cameraBLayerID);
            volumeLayerMask = CameraLayerUtility.Remove(
                volumeLayerMask,
                isCam1 ? cameraBLayerID : cameraALayerID);
            universalAdditionalCameraData.volumeLayerMask = volumeLayerMask;
#endif
            if(liveCamera == null || liveCamera.TargetCamera == null) return;
            var cullingMask = liveCamera.TargetCamera.cullingMask;
            cullingMask = CameraLayerUtility.Add(
             cullingMask,
             isCam1 ? cameraALayerID : cameraBLayerID);
            cullingMask= CameraLayerUtility.Remove(
             cullingMask,
             isCam1 ? cameraBLayerID : cameraALayerID);
            liveCamera.TargetCamera.cullingMask = cullingMask;


            if (liveCamera != null && liveCamera.cinemachineVolumeForceLayerChange != null)
            {
                if(liveCamera.cinemachineVolumeForceLayerChange)liveCamera.cinemachineVolumeForceLayerChange.SetEnable(true);
            }
        }

    }
}