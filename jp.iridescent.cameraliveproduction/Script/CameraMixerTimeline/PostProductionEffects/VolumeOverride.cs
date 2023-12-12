using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace CameraLiveProduction
{
    [Serializable]
    public class VolumeOverride:CameraPostProductionBase
    {


        [SerializeReference]public VolumeProfile volumeProfile;
        [HideInInspector]public Volume volume;
        [SerializeField] [Range(0f,1f)]public float volumeWeight = 1;
        public override void UpdateEffect(LiveCameraBase liveCamera, float time,float weight = 1f)
        {
            // if(liveCamera.TargetCamera == null)
            //     return;

            progress = weight;
            if (volume == null)
            {
                Initialize(liveCamera);
            }
            volume.enabled = weight > 0;
            volume.weight = progress * volumeWeight;
            volume.profile = volumeProfile;
            
            
#if USE_HDRP

#endif
        }
        
        public override void Initialize(LiveCameraBase liveCamera)
        {
            if(liveCamera == null)
                return;
            
            volume = liveCamera.GetComponent<UnityEngine.Rendering.Volume>();
            if (volume == null)
            {
                volume = liveCamera.gameObject.AddComponent<Volume>();
                volumeProfile = volume.sharedProfile;
            }
        }

        public override void OnClipDisable(LiveCameraBase liveCamera)
        {
            if(volume == null) return;
            volume.weight = 0;
            volume.enabled = false;
            progress = 0;
        }

        public override void OnDestroy(LiveCameraBase liveCamera)
        {
        }
    }
}