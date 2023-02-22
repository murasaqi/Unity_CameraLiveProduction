using System;
using System.Collections.Generic;
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

    
    [Serializable]
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class LiveCamera:LiveCameraBase
    {
        public CameraMixer cameraMixer;
        public bool useCinemachineVolumeSettings = true;
        public CinemachineVolumeForceLayerChange cinemachineVolumeForceLayerChange;
        private void OnEnable()
        {
            Initialize();
        }

        [ContextMenu("Initialize")]
        public override void Initialize()
        {
            originalCamera = GetComponent<Camera>();
#if USE_CINEMACHINE
            cinemachineBrain = GetComponent<CinemachineBrain>();
            if (useCinemachineVolumeSettings)
            {
                cinemachineVolumeForceLayerChange = GetComponent<CinemachineVolumeForceLayerChange>();
                if (cinemachineVolumeForceLayerChange == null)
                {
                    cinemachineVolumeForceLayerChange = gameObject.AddComponent<CinemachineVolumeForceLayerChange>();
                }
            }
#endif

#if USE_HDRP
            hdAdditionalCameraData = GetComponent<HDAdditionalCameraData>();
#endif
            
        }


        private void Update()
        {
            if (originalCamera == null)
            {
                originalCamera = GetComponent<Camera>();
            }

            if (cinemachineVolumeForceLayerChange != null && cinemachineVolumeForceLayerChange.volume != null)
            {
                cinemachineVolumeForceLayerChange.volume.gameObject.SetActive(TargetCamera.enabled);
            }

           
        }

        private void OnDestroy()
        {
            if (cloneCamera != null)
            {
                DestroyImmediate(cloneCamera);
            }
        }

        // public override Camera camera => targetCamera;
    }
}