using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;

namespace CameraLiveProduction
{

    [Serializable]
    public class CameraMixerTimelineBehaviour : PlayableBehaviour
    {
        [HideInInspector]public Camera camera;
        [HideInInspector] public LiveCamera liveCamera = null;
        [SerializeReference] public List<CameraPostProductionBase> cameraPostProductions = new List<CameraPostProductionBase>();
        public override void OnPlayableCreate(Playable playable)
        {
           
        }


        public void Initialize()
        {
            liveCamera = camera.GetComponent<LiveCamera>();
            if( liveCamera == null)
            {
                liveCamera = camera.gameObject.AddComponent<LiveCamera>();
            }
        }
    }
}