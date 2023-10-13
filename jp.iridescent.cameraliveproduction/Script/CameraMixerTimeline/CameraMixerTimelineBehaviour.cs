using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace CameraLiveProduction
{
    [Serializable]
    public class CameraMixerTimelineBehaviour : PlayableBehaviour
    {
        // public Camera camera;
        [HideInInspector] public LiveCamera liveCamera = null;

        [SerializeReference]
        public List<CameraPostProductionBase> cameraPostProductions = new List<CameraPostProductionBase>();

        public int fadeMaterialSettingIndex = 0;

        public override void OnPlayableCreate(Playable playable)
        {
        }
    }
}