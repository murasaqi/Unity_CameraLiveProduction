using System;
using System.Collections.Generic;

#if USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

#if USE_CINEMACHINE
using Cinemachine;
#endif

using UnityEngine;

namespace CameraLiveProduction
{
    [Serializable]
    public abstract class LiveCameraBase:MonoBehaviour
    {
       
        public virtual void Initialize(){}
        // public Camera CreateCameraClone(bool disableOriginalCamera = false)
        // {
        //     // Debug.Log($"Create {originalCamera.name}");
        //     if (originalCamera == null) return null;
        //     cloneCamera = Instantiate(originalCamera);
        //     cloneLiveCamera = cloneCamera.gameObject.AddComponent<CloneLiveCamera>();
        //     originalCamera.enabled = !disableOriginalCamera;
        //     return cloneCamera;
        // }
        //
        // public void DestroyAllCloneCameraInChildren()
        // {
        //     var cloneCameras = transform.GetComponentsInChildren<CloneLiveCamera>();
        //     
        //     for( int i = cloneCameras.Length - 1; i >= 0; --i ){
        //         if(cloneCameras[i].gameObject)DestroyImmediate( cloneCameras[i].gameObject );
        //     }
        //     
        // }
        // public Camera camera => targetCamera;


    }
}