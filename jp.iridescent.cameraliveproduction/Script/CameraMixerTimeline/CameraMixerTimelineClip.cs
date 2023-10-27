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

        [FormerlySerializedAs("newExposedReference")]
        public ExposedReference<LiveCamera> camera;

        public RenderTexture debugRenderTexture = null;
        public int fadeMaterialSettingIndex = 0;
        public CameraMixerTimelineBehaviour clone;
        public CameraMixerTimelineTrack track;

        public LiveCamera liveCamera;
        // public Material material;
        // public Camera Camera => clone.camera;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CameraMixerTimelineBehaviour>.Create(graph, behaviour);
            clone = playable.GetBehaviour();
            liveCamera = camera.Resolve(graph.GetResolver());
            clone.liveCamera = liveCamera;
            clone.fadeMaterialSettingIndex = fadeMaterialSettingIndex;
            return playable;
        }
    }
}