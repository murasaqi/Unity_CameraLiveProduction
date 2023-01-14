using System;
using System.Collections.Generic;
using UnityEngine;

namespace CameraLiveProduction
{

    
    [Serializable]
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class LiveCamera:LiveCameraBase
    {
        
        private void OnEnable()
        {
            Initialize();
        }

        public override void Initialize()
        {
            originalCamera = GetComponent<Camera>();
        }


        private void Update()
        {
            if(originalCamera == null) originalCamera = GetComponent<Camera>();
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