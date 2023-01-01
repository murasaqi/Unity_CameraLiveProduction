using System;
using UnityEngine;

namespace CameraLiveProduction
{
    [Serializable]
    public abstract class CameraPostProductionBase:ICameraPostProduction
    {
        
        public abstract void UpdateEffect(Camera camera);
        public abstract void Initialize();
        
        
    }
}