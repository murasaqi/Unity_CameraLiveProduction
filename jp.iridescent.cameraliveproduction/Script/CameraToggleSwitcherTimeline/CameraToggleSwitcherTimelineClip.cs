using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CameraLiveProduction;
using UnityEngine.Serialization;

[Serializable]
public class CameraToggleSwitcherTimelineClip : PlayableAsset, ITimelineClipAsset
{
    public ExposedReference<Camera> cameraA;
    public ExposedReference<Camera> cameraB;
    public CameraToggleSwitcherTimelineBehaviour toggleParameter = new CameraToggleSwitcherTimelineBehaviour ();

    internal Camera resolvedCameraA;
    internal Camera resolvedCameraB;
    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CameraToggleSwitcherTimelineBehaviour>.Create (graph, toggleParameter);
        CameraToggleSwitcherTimelineBehaviour clone = playable.GetBehaviour ();
        resolvedCameraA = cameraA.Resolve (graph.GetResolver ());
        resolvedCameraB = cameraB.Resolve (graph.GetResolver ());
        return playable;
    }
}
