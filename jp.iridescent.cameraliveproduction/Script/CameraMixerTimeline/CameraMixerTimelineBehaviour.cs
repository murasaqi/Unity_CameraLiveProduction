using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CameraLiveProduction
{

    [Serializable]
    public class CameraMixerTimelineBehaviour : PlayableBehaviour
    {
        // public Camera camera;
        [HideInInspector] public LiveCameraBase liveCameraBase = null;
        [SerializeReference] public List<CameraPostProductionBase> cameraPostProductions = new List<CameraPostProductionBase>();
        public override void OnPlayableCreate(Playable playable)
        {
           
        }


        public void Initialize()
        {
            // if(camera == null) return;
            // liveCamera = camera.GetComponent<LiveCamera>();
            // if( liveCamera == null)
            // {
            //     liveCamera = camera.gameObject.AddComponent<LiveCamera>();
            // }
        }
    }
}