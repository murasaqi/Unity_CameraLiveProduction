using System;
using UnityEngine;

namespace CameraLiveProduction
{
    [Serializable]
    public abstract class CameraPostProductionBase:ICameraPostProduction
    {
        
        public float progress { get;  set; }
        public abstract void UpdateEffect(LiveCameraBase liveCamera, float time,float weight = 1);
        public abstract void Initialize(LiveCameraBase liveCamera);

        public abstract void OnDestroy(LiveCameraBase liveCamera);
        
        public abstract void OnClipDisable(LiveCameraBase liveCamera);


    }
}