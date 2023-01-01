using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;

namespace CameraLiveProduction
{
    [TrackColor(0.8042867f, 0.3647798f, 1f)]
    [TrackClipType(typeof(CameraMixerTimelineClip))]
    [TrackBindingType(typeof(CameraMixer))]
    public class CameraMixerTimelineTrack : TrackAsset
    {
        public ExposedReference<TextMeshProUGUI> debugText;
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer= ScriptPlayable<CameraMixerTimelineMixerBehaviour>.Create(graph, inputCount);
            mixer.GetBehaviour().timelineClips = m_Clips;
            mixer.GetBehaviour().debugText = debugText.Resolve(graph.GetResolver());
            mixer.GetBehaviour().director = go.GetComponent<PlayableDirector>();
            // RenameCameraByClipName();
            return mixer;
        }

    }

}