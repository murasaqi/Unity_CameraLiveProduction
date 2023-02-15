using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CameraLiveProduction;

[TrackColor(0.5628841f, 0.553459f, 1f)]
[TrackClipType(typeof(CameraToggleSwitcherTimelineClip))]
[TrackBindingType(typeof(CameraToggleSwitcher))]
public class CameraToggleSwitcherTimelineTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var behavior= ScriptPlayable<CameraToggleSwitcherTimelineMixerBehaviour>.Create (graph, inputCount);
        
        
        behavior.GetBehaviour().clips = m_Clips;
        
        return behavior;
    }
}
