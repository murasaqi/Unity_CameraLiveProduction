using System;
using UnityEngine;

namespace CameraLiveProduction
{
    [Serializable]
    public abstract class CameraPostProductionBase:ICameraPostProduction
    {
        
        public float progress { get;  set; }
        public abstract void UpdateEffect(LiveCamera liveCamera, float time,float weight = 1);
        public abstract void Initialize(LiveCamera liveCamera);

        public abstract void OnDestroy(LiveCamera liveCamera);
        
        public abstract void OnClipDisable(LiveCamera liveCamera);


    }
}