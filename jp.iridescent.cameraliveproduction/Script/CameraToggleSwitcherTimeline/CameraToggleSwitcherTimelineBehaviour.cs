using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CameraLiveProduction;

[Serializable]
public class CameraToggleSwitcherTimelineBehaviour : PlayableBehaviour
{
    public AnimationCurve fader = new AnimationCurve(new []{new Keyframe(0,0), new Keyframe(1, 1)});

    public override void OnPlayableCreate (Playable playable)
    {
        
    }
}
