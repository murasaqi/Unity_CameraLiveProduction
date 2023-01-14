using System;
using UnityEngine;

namespace CameraLiveProduction
{
    [Serializable]
    public abstract class CameraPostProductionBase:ICameraPostProduction
    {
        
        public abstract void UpdateEffect(LiveCamera liveCamera, float time);
        public abstract void Initialize(LiveCamera liveCamera);

        public abstract void OnDestroy(LiveCamera liveCamera);


    }
}