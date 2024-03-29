﻿using System;
using UnityEngine;

#if USE_HDRP

using UnityEngine.Rendering.HighDefinition;

namespace CameraLiveProduction
{
    public class HDCameraLens: CameraPostProductionBase
    {
        private HDAdditionalCameraData hdAdditionalCameraData = null;
        public float focalLength = 50f;
        public Vector2 shift = Vector2.zero;
        [Range(0.7f,32f)] public float aperture = 6f;
        public float focusDistance = 10f;
        
        // [SerializeField]private bool initialized = false;
        
        public override void UpdateEffect(LiveCamera liveCamera, float time, float weight = 1f)
        {
            if(liveCamera == null || hdAdditionalCameraData == null)
                return;

            // if (!initialized) Initialize(liveCamera);
            liveCamera.TargetCamera.focalLength = focalLength;
            liveCamera.TargetCamera.lensShift = shift;
            hdAdditionalCameraData.physicalParameters.aperture = aperture;
            hdAdditionalCameraData.physicalParameters.focusDistance = focusDistance;
        }

        public override void OnClipDisable(LiveCamera liveCamera)
        {
        }

        public override void Initialize(LiveCamera liveCamera)
        {
            
            if(liveCamera == null) return;
            liveCamera.TargetCamera.usePhysicalProperties = true;
            hdAdditionalCameraData = liveCamera.TargetCamera.GetComponent<HDAdditionalCameraData>();
            focalLength = liveCamera.TargetCamera.focalLength;
            shift = liveCamera.TargetCamera.lensShift;
            aperture = hdAdditionalCameraData.physicalParameters.aperture;
            focusDistance = hdAdditionalCameraData.physicalParameters.focusDistance;
        }
        
        public override void OnDestroy(LiveCamera camera)
        {
        }
    }
}


#endif