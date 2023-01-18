using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace CameraLiveProduction
{
    public class VolumeOverride:CameraPostProductionBase
    {


        public VolumeProfile volumeProfile;
        public override void UpdateEffect(LiveCamera liveCamera, float time,float weight = 1f)
        {
            if(liveCamera.TargetCamera == null)
                return;
#if USE_HDRP
            
#endif
        }
        
        public override void Initialize(LiveCamera liveCamera)
        {
            if(liveCamera == null)
                return;
            
            var volume = liveCamera.GetComponent<UnityEngine.Rendering.Volume>();
            if (volume == null)
                volumeProfile = volume.profile;
        }
        
        public override void OnDestroy(LiveCamera liveCamera)
        {
        }
    }
}