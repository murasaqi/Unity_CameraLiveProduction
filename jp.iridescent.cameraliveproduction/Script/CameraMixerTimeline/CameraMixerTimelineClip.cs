using System;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;

namespace CameraLiveProduction
{

    [Serializable]
    public class CameraMixerTimelineClip : PlayableAsset, ITimelineClipAsset
    {
        public CameraMixerTimelineBehaviour behaviour = new CameraMixerTimelineBehaviour();
        public ExposedReference<LiveCamera> newExposedReference;
        public CameraMixerTimelineBehaviour clone;
        public CameraMixerTimelineTrack track;
        public LiveCamera liveCamera;
        // public Camera Camera => clone.camera;
        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CameraMixerTimelineBehaviour>.Create(graph, behaviour);
            clone = playable.GetBehaviour();
            liveCamera = newExposedReference.Resolve(graph.GetResolver());
            clone.liveCamera = liveCamera;
            clone.Initialize();
            return playable;
        }
        
  
        
    }

}