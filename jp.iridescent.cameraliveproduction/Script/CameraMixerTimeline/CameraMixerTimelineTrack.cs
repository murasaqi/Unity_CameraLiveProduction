using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CameraLiveProduction
{

    public enum CameraRenderTiming
    {
        Timeline =0,
        Update=1
    }
    [TrackColor(0.8042867f, 0.3647798f, 1f)]
    [TrackClipType(typeof(CameraMixerTimelineClip))]
    [TrackBindingType(typeof(CameraMixer))]
    public class CameraMixerTimelineTrack : TrackAsset
    {
        public bool clipNameAsCameraName = true;
        public ExposedReference<TextMeshProUGUI> debugText;
        // public CameraRenderTiming cameraRenderTiming = CameraRenderTiming.Timeline;
        public int preRenderFrame = 4;
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer= ScriptPlayable<CameraMixerTimelineMixerBehaviour>.Create(graph, inputCount);
            m_Clips.Sort( (a,b)=>a.start.CompareTo(b.start));
            foreach (var clip in m_Clips)
            {
                var cameraMixerTimelineClip = clip.asset as CameraMixerTimelineClip;
                if(cameraMixerTimelineClip)cameraMixerTimelineClip.track = this;
            }
            AssetDatabase.SaveAssets();
            

            mixer.GetBehaviour().timelineClips = m_Clips;
            mixer.GetBehaviour().debugText = debugText.Resolve(graph.GetResolver());
            mixer.GetBehaviour().director = go.GetComponent<PlayableDirector>();
            mixer.GetBehaviour().track = this;
            return mixer;
        }

    }

}