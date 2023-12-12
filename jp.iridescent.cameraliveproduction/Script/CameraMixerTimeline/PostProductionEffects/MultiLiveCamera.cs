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
    public class MultiLiveCamera:LiveCameraBase
    {
        
        private void OnEnable()
        {
            Initialize();
        }

        [ContextMenu("Initialize")]
        public override void Initialize()
        {
           
            
        }


        private void Update()
        {
           
        }

        private void OnDestroy()
        {
           
        }

        // public override Camera camera => targetCamera;
    }
}