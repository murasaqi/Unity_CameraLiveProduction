using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CameraLiveProduction;

public class CameraToggleSwitcherTimelineMixerBehaviour : PlayableBehaviour
{

    private CameraToggleSwitcher cameraToggleSwitcher;
    public List<TimelineClip> clips = new List<TimelineClip>();
    // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        cameraToggleSwitcher = playerData as CameraToggleSwitcher;

        if (!cameraToggleSwitcher)
            return;

        int inputCount = playable.GetInputCount ();

        
        var currentTimelineTime = playable.GetTime();
        var  currentIndex = -1;
            
        var preIndex = -1;
        var nextIndex = -1;
        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            var currentClip = clips[i];
            var progress = (currentTimelineTime - currentClip.start) / currentClip.duration;
            if(inputWeight>0)
            {
                currentIndex = i;
                FetchCamera( playable, i, (float)progress);
            }

            if(currentClip.end < currentTimelineTime)
            {
                preIndex    = i;
            }

            if (currentTimelineTime < currentClip.start)
            {
                if (nextIndex == -1)
                {
                    nextIndex = i;
                }else
                {
                    nextIndex = i < nextIndex ? i : nextIndex;
                }
            }
            
        }


        if (currentIndex == -1 && preIndex >=0 && nextIndex == -1)
        {
            FetchCamera(playable, preIndex, 1f);
        }
        
        if (currentIndex == -1 && preIndex == -1 && nextIndex >=0)
        {
            FetchCamera(playable, nextIndex, 0f);
        }   

        if (cameraToggleSwitcher.cameraRenderTiming == CameraRenderTiming.Timeline)
        {
            cameraToggleSwitcher.Render();
           
        }
    }


    public void FetchCamera(Playable playable, int index, float fader)
    {
        ScriptPlayable<CameraToggleSwitcherTimelineBehaviour> inputPlayable = (ScriptPlayable<CameraToggleSwitcherTimelineBehaviour>)playable.GetInput(index);
        CameraToggleSwitcherTimelineBehaviour input = inputPlayable.GetBehaviour ();
        var preClip = clips[index];
        var blend = 0f;
        if (fader == 0f || fader == 1f)
        {
            blend = fader;
        }
        else
        {
            blend = input.fader.Evaluate(fader);
        }
        var preCameraToggleSwitcherTimelineClip = preClip.asset as CameraToggleSwitcherTimelineClip;
        if (preCameraToggleSwitcherTimelineClip != null)
        {
            cameraToggleSwitcher.SetCameraQueue(
                preCameraToggleSwitcherTimelineClip.resolvedCameraA,
                preCameraToggleSwitcherTimelineClip.resolvedCameraB,
                blend
                );
        }
    }
    
}
