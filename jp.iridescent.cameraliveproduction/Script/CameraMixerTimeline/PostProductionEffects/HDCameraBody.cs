using System;
using UnityEngine;

#if USE_HDRP

using UnityEngine.Rendering.HighDefinition;

namespace CameraLiveProduction
{
    public class HDCameraBody: CameraPostProductionBase
    {
        public Vector2 sensorSize = new Vector2(24,32);
        public int ISO = 200;
        public float shutterSpeed;
        public Camera.GateFitMode gateFit;
        public HDAdditionalCameraData hdAdditionalCameraData;
        public override void UpdateEffect(LiveCamera liveCamera, float time, float weight = 1f)
        {
            if(liveCamera == null)
                return;

            // if (!initialized) Initialize(liveCamera);
            liveCamera.TargetCamera.sensorSize = sensorSize;
            liveCamera.TargetCamera.iso = ISO;
            liveCamera.TargetCamera.shutterSpeed = shutterSpeed;
            liveCamera.TargetCamera.gateFit = gateFit;
        }

        public override void OnClipDisable(LiveCamera liveCamera)
        {
        }

        public override void Initialize(LiveCamera liveCamera)
        {
            
            if(liveCamera == null) return;
            liveCamera.TargetCamera.usePhysicalProperties = true;
            hdAdditionalCameraData = liveCamera.TargetCamera.GetComponent<HDAdditionalCameraData>();
            sensorSize = liveCamera.TargetCamera.sensorSize;
            ISO = liveCamera.TargetCamera.iso;
            shutterSpeed = liveCamera.TargetCamera.shutterSpeed;
            gateFit = liveCamera.TargetCamera.gateFit;
            // initialized = true;
        }
        
        public override void OnDestroy(LiveCamera camera)
        {
        }
    }
}

#endif