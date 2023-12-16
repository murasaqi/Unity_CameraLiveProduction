using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Serialization;

namespace CameraLiveProduction
{

    [Serializable]
    public class CameraMixerTimelineClip : PlayableAsset, ITimelineClipAsset
    {
        public CameraMixerTimelineBehaviour behaviour = new CameraMixerTimelineBehaviour();
        [FormerlySerializedAs("newExposedReference")] public ExposedReference<LiveCameraBase> camera;
        public CameraMixerTimelineBehaviour clone;
        public CameraMixerTimelineTrack track;
        public LiveCameraBase liveCameraBase;
        // public Camera Camera => clone.camera;
        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CameraMixerTimelineBehaviour>.Create(graph, behaviour);
            clone = playable.GetBehaviour();
            liveCameraBase = camera.Resolve(graph.GetResolver());
            clone.liveCameraBase = liveCameraBase;
            clone.Initialize();
            return playable;
        }
        
  
        
    }

}