using System;
using UnityEngine;
namespace CameraLiveProduction
{
    public class CameraShake:CameraPostProductionBase
    {
        
        public Vector3 offsetPosition = Vector3.zero;
        public Vector3 offsetRotation = Vector3.zero;
        // public Vector3 shakeDirection = new Vector3(1, 1, 1);
        public Vector3 positionShakeRange = new Vector3(0.3f, 0.3f, 0f);
        public Vector3 rotationShakeRange = new Vector3(10f, 10f, 0f);
        public Vector3 noiseSeed = Vector3.zero;
        // public Vector3 noiseScale = new Vector3(1, 1, 1);
        public Vector3 positionRoughness = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 rotationRoughness = new Vector3(0.5f, 0.5f, 0.5f);
        public override void UpdateEffect(LiveCamera liveCamera,float time, float weight = 1f)
        {
            if(liveCamera == null)
                return;

            if (!liveCamera.hasCloneCamera)
            {
                // Debug.Log(liveCamera.cloneCamera);
                Initialize(liveCamera);
            }
            // culculate 3d wiggler noise
            var positionNoise = new Vector3(
                (Mathf.PerlinNoise(noiseSeed.x + time * positionRoughness.x, 0)-0.5f) * positionShakeRange.x,
                (Mathf.PerlinNoise(noiseSeed.y + time * positionRoughness.y, 0) - 0.5f ) * positionShakeRange.y,
                (Mathf.PerlinNoise(noiseSeed.z + time * positionRoughness.z, 0)  - 0.5f ) * positionShakeRange.z
            );
            
            var rotationNoise = new Vector3(
                (Mathf.PerlinNoise(noiseSeed.x + time * rotationRoughness.x, 0) - 0.5f) * rotationShakeRange.x,
                (Mathf.PerlinNoise(noiseSeed.y + time * rotationRoughness.y, 0) - 0.5f) * rotationShakeRange.y,
                (Mathf.PerlinNoise(noiseSeed.z + time * rotationRoughness.z, 0) - 0.5f) * rotationShakeRange.z
            );
            
            // apply noise
            liveCamera.TargetCamera.transform.localPosition = offsetPosition;// + positionNoise*weight;
            liveCamera.TargetCamera.transform.localEulerAngles = offsetRotation;// + rotationNoise*weight;
            
            liveCamera.TargetCamera.transform.localPosition += positionNoise*weight;
            liveCamera.TargetCamera.transform.localEulerAngles += rotationNoise*weight;
            
        }

        public override void OnClipDisable(LiveCamera liveCamera)
        {
            if (liveCamera.hasCloneCamera)
            {
                liveCamera.TargetCamera.transform.localPosition = Vector3.zero;
                liveCamera.TargetCamera.transform.localEulerAngles = Vector3.zero;
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
            
            noiseSeed = new Vector3(
                UnityEngine.Random.Range(0f, 100f),
                UnityEngine.Random.Range(0f, 100f),
                UnityEngine.Random.Range(0f, 100f)
            );
            // var clone = Instantiate(camera.gameObject);
        }
        
        
        public override void OnDestroy(LiveCamera camera)
        {
        }
        
    }
}