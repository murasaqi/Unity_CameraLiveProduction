using System;
using System.Collections.Generic;

#if USE_HDRP
using Cinemachine;
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace CameraLiveProduction
{
    [Serializable]
    public abstract class LiveCameraBase:MonoBehaviour
    {
       
        public Camera TargetCamera => cloneCamera ? cloneCamera: originalCamera;
        public Camera OriginalCamera => originalCamera;
#if USE_CINEMACHINE
        public CinemachineBrain cinemachineBrain;
        // public Volume cinemachineVolume;
#endif

#if USE_HDRP
        public HDAdditionalCameraData hdAdditionalCameraData;
#endif
        protected Camera originalCamera;
        [SerializeField]public Camera cloneCamera;
        public List<CameraPostProductionBase> postProduction = new List<CameraPostProductionBase>();
        // public virtual Camera camera { get; }
        public bool hasCloneCamera => cloneCamera != null;
        public virtual void Initialize(){}
        public Camera CreateCameraClone(bool disableOriginalCamera = false)
        {
            Debug.Log($"Create {originalCamera.name}");
            if (originalCamera == null) return null;
            cloneCamera = Instantiate(originalCamera);
            cloneCamera.gameObject.AddComponent<CloneLiveCamera>();
            originalCamera.enabled = !disableOriginalCamera;
            return cloneCamera;
        }

        public void DestroyAllCloneCameraInChildren()
        {
            var cloneCameras = transform.GetComponentsInChildren<CloneLiveCamera>();
            
            for( int i = cloneCameras.Length - 1; i >= 0; --i ){
                DestroyImmediate( cloneCameras[i].gameObject );
            }
            
        }
        // public Camera camera => targetCamera;


    }
}