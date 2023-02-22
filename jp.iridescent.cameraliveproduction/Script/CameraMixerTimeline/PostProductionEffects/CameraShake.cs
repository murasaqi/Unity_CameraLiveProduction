using System;
using UnityEngine;
namespace CameraLiveProduction
{
    public class CameraShake:CameraPostProductionBase
    {
        
        public Vector3 offsetPosition = Vector3.zero;
        // public Vector3 shakeDirection = new Vector3(1, 1, 1);
        public Vector3 shakeRange = new Vector3(0.3f, 0.3f, 0.3f);
        public Vector3 noiseSeed = Vector3.zero;
        // public Vector3 noiseScale = new Vector3(1, 1, 1);
        public Vector3 roughness = new Vector3(1, 1, 1);
        public override void UpdateEffect(LiveCamera liveCamera,float time, float weight = 1f)
        {
            if(liveCamera == null)
                return;

            if (!liveCamera.hasCloneCamera)
            {
                // Debug.Log(liveCamera.cloneCamera);
                Initialize(liveCamera);
            }

            // make 3d noise
            Vector3 noise = new Vector3(
                (Mathf.PerlinNoise( noiseSeed.x+time * roughness.x,0) - 0.5f)*shakeRange.x,
                (Mathf.PerlinNoise(noiseSeed.y + time*roughness.y,0f) - 0.5f)*shakeRange.x,
                (Mathf.PerlinNoise(noiseSeed.z +time * roughness.z, 0f)-0.5f)*shakeRange.z
            );
            
            // apply noise
            liveCamera.TargetCamera.transform.localPosition = offsetPosition + noise;
            
        }

        public override void OnClipDisable(LiveCamera liveCamera)
        {
            if (liveCamera.hasCloneCamera)
            {
                liveCamera.TargetCamera.transform.localPosition = Vector3.zero;
            }
        }

        public override void Initialize(LiveCamera liveCamera)
        {
            if(liveCamera == null) return;
            
            liveCamera.DestroyAllCloneCameraInChildren();
            
            if (!liveCamera.hasCloneCamera)
            {
                // Debug.Log(liveCamera.cloneCamera);
                var clone = liveCamera.CreateCameraClone(true);
                clone.transform.SetParent(liveCamera.transform);
                offsetPosition = Vector3.zero;
            }
            // var clone = Instantiate(camera.gameObject);
        }
        
        
        public override void OnDestroy(LiveCamera camera)
        {
        }
        
    }
}