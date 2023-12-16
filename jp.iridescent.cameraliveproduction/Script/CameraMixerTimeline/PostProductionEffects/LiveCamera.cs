using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

#if USE_URP
using UnityEngine.Rendering.Universal;
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
        public bool useCinemachineVolumeSettings = true;
        public CinemachineVolumeForceLayerChange cinemachineVolumeForceLayerChange;

        
        
        public Camera TargetCamera => cloneCamera ? cloneCamera: originalCamera;
        public Camera OriginalCamera => originalCamera;
#if USE_CINEMACHINE
        public CinemachineBrain cinemachineBrain;
      
#endif

#if USE_HDRP
        public HDAdditionalCameraData hdAdditionalCameraData;
#endif
        protected Camera originalCamera;
        [SerializeField]public Camera cloneCamera;
        public CloneLiveCamera cloneLiveCamera;
        public List<CameraPostProductionBase> postProduction = new List<CameraPostProductionBase>();
        public bool hasCloneCamera => cloneCamera != null;
        public Camera CreateCameraClone(bool disableOriginalCamera = false)
        {
            // Debug.Log($"Create {originalCamera.name}");
            if (originalCamera == null) return null;
            cloneCamera = Instantiate(originalCamera);
            cloneLiveCamera = cloneCamera.gameObject.AddComponent<CloneLiveCamera>();
            originalCamera.enabled = !disableOriginalCamera;
            return cloneCamera;
        }

        public void DestroyAllCloneCameraInChildren()
        {
            var cloneCameras = transform.GetComponentsInChildren<CloneLiveCamera>();
            
            for( int i = cloneCameras.Length - 1; i >= 0; --i ){
                if(cloneCameras[i].gameObject)DestroyImmediate( cloneCameras[i].gameObject );
            }
            
        }
        
        public override LayerMask GetLayerMask()
        {
            return TargetCamera.cullingMask;
        }

        public override RenderTexture TargetTexture
        {
            get
            {
                return TargetCamera ? TargetCamera.targetTexture : null;
            }
            set
            {
                if (TargetCamera)   
                {
                    TargetCamera.targetTexture = value;
                }
            }
        }

        public override void SetEnableTargetCamera(bool enable)
        {
            base.SetEnableTargetCamera(enable);
            TargetCamera.enabled = enable;
        }


#if USE_URP
        public UniversalAdditionalCameraData universalAdditionalCameraData;
#endif
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
            if (useCinemachineVolumeSettings&& cinemachineBrain)
            {
                cinemachineVolumeForceLayerChange = GetComponent<CinemachineVolumeForceLayerChange>();
                if (cinemachineVolumeForceLayerChange == null)
                {
                    cinemachineVolumeForceLayerChange = gameObject.AddComponent<CinemachineVolumeForceLayerChange>();
                }
            }
            else
            {
                cinemachineVolumeForceLayerChange = GetComponent<CinemachineVolumeForceLayerChange>();
                if(cinemachineVolumeForceLayerChange != null)
                    DestroyImmediate(cinemachineVolumeForceLayerChange);
            }
#endif

#if USE_HDRP
            hdAdditionalCameraData = GetComponent<HDAdditionalCameraData>();
#endif
#if USE_URP
            universalAdditionalCameraData = GetComponent<UniversalAdditionalCameraData>();
#endif
            
        }


        private void Update()
        {
            if (originalCamera == null)
            {
                originalCamera = GetComponent<Camera>();
            }
            
           
        }

        public override void Render(Texture outputTexture)
        {
            base.Render(outputTexture);
            
            originalCamera.targetTexture = outputTexture as RenderTexture;
            originalCamera.Render();
        }

        public override void TryInitialize()
        {
            if (!TargetCamera)
            {
                Initialize();
            }
        }

        private void OnDestroy()
        {
            if (cloneCamera != null)
            {
#if USE_HDRP
                hdAdditionalCameraData = null;
#endif
                if (cloneLiveCamera != null)
                {
                    
                }
                DestroyImmediate(cloneCamera);
            }
        }

        // public override Camera camera => targetCamera;
    }
}