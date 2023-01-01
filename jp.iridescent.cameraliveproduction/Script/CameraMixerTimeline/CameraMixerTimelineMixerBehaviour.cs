using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CameraLiveProduction
{

    public class CameraMixerClipInfo
    {
        public TimelineClip clip;
        public CameraMixerTimelineBehaviour behaviour;
        public float inputWeight;
        
        public CameraMixerClipInfo(TimelineClip clip, CameraMixerTimelineBehaviour behaviour, float inputWeight)
        {
            this.clip = clip;
            this.behaviour = behaviour;
            this.inputWeight = inputWeight;
        }
        
        
    }
    public class CameraMixerTimelineMixerBehaviour : PlayableBehaviour
    {
        public TextMeshProUGUI debugText;
        private StringBuilder stringBuilder;

        public List<TimelineClip> timelineClips = new List<TimelineClip>();
        private CameraMixer cameraMixer;
        
        readonly List<CameraMixerClipInfo> cameraQue = new List<CameraMixerClipInfo>();
        private TimelineAsset timelineAsset;
        internal PlayableDirector director;
        private bool isFirstFrameHappened = false;
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            cameraMixer = playerData as CameraMixer;

            if (!cameraMixer || !director)
                return;
            
            if(isFirstFrameHappened == false)
            {
                stringBuilder = new StringBuilder();
                
                timelineAsset = director.playableAsset as TimelineAsset;

                isFirstFrameHappened = true;

                cameraMixer.cameraList.Distinct();
                foreach (var clip in timelineClips)
                {
                    var cameraMixerTimelineClip = clip.asset as CameraMixerTimelineClip;
                    var cameraMixerTimelineBehaviour = cameraMixerTimelineClip.behaviour;
                    cameraMixerTimelineBehaviour.cameraPostProductions.RemoveAll(x => x == null);
                    cameraMixerTimelineBehaviour.cameraPostProductions.RemoveAll(x => cameraMixerTimelineBehaviour.cameraPostProductions.Any(y => y.GetType() == x.GetType() && x!= y));

                }

                RenameCameraByClipName();

            }

            // var _director =   playable.GetGraph().GetResolver() as PlayableDirector;
            // Debug.Log($"{director.name}:{director.time},{_director.name}:{_director.time}");
            int inputCount = playable.GetInputCount();
            cameraQue.Clear();
            for (int i = 0; i < inputCount; i++)
            {
                var clip = timelineClips[i];
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<CameraMixerTimelineBehaviour> inputPlayable =
                    (ScriptPlayable<CameraMixerTimelineBehaviour>)playable.GetInput(i);
                CameraMixerTimelineBehaviour input = inputPlayable.GetBehaviour();
                input.cameraPostProductions.Distinct();
                
                
                if (input.camera != null && cameraMixer.cameraList.Contains(input.camera) != true)
                {
                    cameraMixer.cameraList.Add(input.camera);
                }

                if (inputWeight > 0)
                {
                    cameraQue.Add(new CameraMixerClipInfo(clip, input, inputWeight));
                }
                
                // if (clip.start <= director.time && director.time < clip.start + clip.duration)
                // {
                //     cameraQue.Add(new CameraSwitcherClipInfo(clip, input, inputWeight));
                //     Debug.Log($"time: {clip.displayName} {input.camera.name}, {clip.start}, {clip.duration}");
                //    
                // }
                
            }
            
            
            SetCameraQueue(cameraQue);
            ApplyPostEffect(cameraQue);

            if (debugText != null)
            {
                stringBuilder.Clear();
                var dateTime = TimeSpan.FromSeconds(director.time);
                stringBuilder.Append($"[{cameraMixer.gameObject.scene.name}]  ");
                stringBuilder.Append(dateTime.ToString(@"hh\:mm\:ss\:ff"));
                stringBuilder.Append(" ");
                stringBuilder.Append((Mathf.CeilToInt((float)timelineAsset.editorSettings.frameRate * (float) director.time)));
                stringBuilder.Append("f  ");
                
                debugText.text = stringBuilder.ToString();
                // if (A != null && A.behaviour.camera != null) _stringBuilder.Append($"{A.behaviour.camera.name}");
                // if (B != null && B.behaviour.camera != null) _stringBuilder.Append($" >> {B.behaviour.camera.name}");

            }
        }
        
        public override void OnPlayableDestroy (Playable playable)
        {
            isFirstFrameHappened = false;
            if(cameraMixer)cameraMixer.useTimeline = false;
        }

        private void ApplyPostEffect(List<CameraMixerClipInfo> clipInfos)
        {
            foreach (var clipInfo in clipInfos)
            {
                foreach (var cameraPostProduction in clipInfo.behaviour.cameraPostProductions)
                {
                    if (cameraPostProduction != null)
                    {
                        if(clipInfo.behaviour.camera)cameraPostProduction.UpdateEffect(clipInfo.behaviour.camera);
                    }
                }
            }
        }
        
        public void RenameCameraByClipName()
        {
            var cameraClipDic = new Dictionary<Camera, string>();
            foreach (var clip in timelineClips)
            {
                var asset = clip.asset as CameraMixerTimelineClip;
                if(asset == null)continue;
                var clipName = clip.displayName;
                var camera = asset.behaviour.camera;
                if(camera == null) continue;
                
                Debug.Log($"{clipName} {camera.gameObject.name}");
                if (cameraClipDic.ContainsKey(camera))
                {
                    Debug.Log("ContainsKey");
                    cameraClipDic[camera] = $"{cameraClipDic[camera]}_{clipName}";
                }
                else
                {
                    Debug.Log("Not ContainsKey");
                    cameraClipDic.Add(camera,clipName);
                }
            }

            foreach (var camera in cameraClipDic.Keys)
            {
                Debug.Log(cameraClipDic[camera]);
                if(camera == null) continue;
                camera.gameObject.name = cameraClipDic[camera];
            }

        }

        private void SetCameraQueue(List<CameraMixerClipInfo> clips)
        {
            if(cameraMixer == null) return;
            if(clips.Count<=0) return;
            
            if (clips.Count == 1)
            {
               
                if(clips[0].behaviour.camera == null) return;
                cameraMixer.SetCameraQueue(clips[0].behaviour.camera,null,0);
            }
            else if (clips.Count == 2)
            {
                if(clips[0].behaviour.camera == null || clips[1].behaviour.camera == null) return;
                cameraMixer.SetCameraQueue(clips[0].behaviour.camera, clips[1].behaviour.camera, 1f-clips[0].inputWeight);
            }
            
            
        }
    }
}