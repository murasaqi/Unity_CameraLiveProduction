using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;


namespace CameraLiveProduction
{
    [SerializeField]
    public enum CameraColorBlendMode
    {
        DISABLE = 0,
        MULTIPLY = 1,
        ADD = 2,
        SUBTRACT = 3,
        OVERWRITE = 4,
    }

    [Serializable]
    public class CameraMixerTimelineBehaviour : PlayableBehaviour
    {
        // public Camera camera;
        [HideInInspector] public LiveCamera liveCamera = null;

        [SerializeReference]
        public List<CameraPostProductionBase> cameraPostProductions = new List<CameraPostProductionBase>();

        public int fadeMaterialSettingIndex = 0;
        public Color multiplyColor = new Color(0, 0, 0, 0);
        public CameraColorBlendMode colorBlendMode = CameraColorBlendMode.DISABLE;

        public override void OnPlayableCreate(Playable playable)
        {
        }
    }
}