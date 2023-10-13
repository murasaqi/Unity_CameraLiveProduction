using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace CameraLiveProduction
{
    public struct CameraMixerClipInfo
    {
        public LiveCamera liveCamera;
        public float inputWeight;
        public int fadeMaterialSettingIndex;

        public CameraMixerClipInfo(LiveCamera liveCamera, float inputWeight, int fadeMaterialSettingIndex)
        {
            this.liveCamera = liveCamera;
            this.inputWeight = inputWeight;
            this.fadeMaterialSettingIndex = fadeMaterialSettingIndex;
        }
    }

    public class CameraMixerTimelineMixerBehaviour : PlayableBehaviour
    {
        public TextMeshProUGUI debugText;
        private StringBuilder stringBuilder;
        private float currentTime;
        public List<TimelineClip> timelineClips = new List<TimelineClip>();
        private CameraMixer cameraMixer;

        readonly List<CameraMixerClipInfo> cameraQue = new List<CameraMixerClipInfo>();

        private TimelineAsset timelineAsset;
        internal PlayableDirector director;
        internal CameraMixerTimelineTrack track;

        private bool isFirstFrameHappened = false;

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            cameraMixer = playerData as CameraMixer;

            if (!cameraMixer || !director)
                return;

            if (isFirstFrameHappened == false)
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
                    cameraMixerTimelineBehaviour.cameraPostProductions.RemoveAll(x =>
                        cameraMixerTimelineBehaviour.cameraPostProductions.Any(
                            y => y.GetType() == x.GetType() && x != y));
                }
            }

            cameraQue.Clear();
            currentTime = (float)director.time;
            var offsetTime = director.time + (1 / timelineAsset.editorSettings.frameRate) * track.preRenderFrame;
            Material material1 = null;
            var timeMax = double.MinValue;
            for (int i = 0; i < timelineClips.Count; i++)
            {
                var clip = timelineClips[i];
                var cameraMixerTimelineClip = clip.asset as CameraMixerTimelineClip;

                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<CameraMixerTimelineBehaviour> inputPlayable =
                    (ScriptPlayable<CameraMixerTimelineBehaviour>)playable.GetInput(i);
                CameraMixerTimelineBehaviour input = inputPlayable.GetBehaviour();
                input.cameraPostProductions.Distinct();
                var inputPlayableTime = inputPlayable.GetTime();

#if UNITY_EDITOR

                if (track.clipNameAsCameraName && cameraMixerTimelineClip != null &&
                    cameraMixerTimelineClip.liveCamera != null)
                {
                    clip.displayName = cameraMixerTimelineClip.liveCamera.gameObject.name;
                }

#endif
                if (input.liveCamera != null && cameraMixer.cameraList.Contains(input.liveCamera) != true)
                {
                    cameraMixer.cameraList.Add(input.liveCamera);
                }


                if (cameraQue.Count == 0)
                {
                    if (clip.start <= currentTime && currentTime < clip.start + clip.duration)
                    {
                        if (input.liveCamera) input.liveCamera.TargetCamera.enabled = true;
                        cameraQue.Add(new CameraMixerClipInfo(cameraMixerTimelineClip.liveCamera, inputWeight,
                            input.fadeMaterialSettingIndex));
                    }
                    else
                    {
                        UpdatePostEffect(input);
                    }
                }
                else
                {
                    if (clip.start <= offsetTime && offsetTime < clip.start + clip.duration)
                    {
                        if (input.liveCamera) input.liveCamera.TargetCamera.enabled = true;
                        cameraQue.Add(new CameraMixerClipInfo(cameraMixerTimelineClip.liveCamera, inputWeight,
                            input.fadeMaterialSettingIndex));
                    }
                    else
                    {
                        UpdatePostEffect(input);
                    }
                }

                if (inputWeight is > 0f and < 1.0f)
                {
                    if (timeMax < inputPlayableTime)
                    {
                        timeMax = inputPlayableTime;
                    }
                }
            }

            cameraQue.Sort((a, b) => a.inputWeight.CompareTo(b.inputWeight));
            SetCameraQueue(cameraQue);

            if (debugText != null)
            {
                stringBuilder.Clear();
                var dateTime = TimeSpan.FromSeconds(director.time);
                stringBuilder.Append($"[{cameraMixer.gameObject.scene.name}]  ");
                stringBuilder.Append(dateTime.ToString(@"hh\:mm\:ss\:ff"));
                stringBuilder.Append(" ");
                stringBuilder.Append(
                    (Mathf.CeilToInt((float)timelineAsset.editorSettings.frameRate * (float)director.time)));
                stringBuilder.Append("f  ");

                debugText.text = stringBuilder.ToString();
            }

            if (cameraMixer.cameraRenderTiming == CameraRenderTiming.Timeline)
            {
                cameraMixer.Render();
            }
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            isFirstFrameHappened = false;
        }

        private void UpdatePostEffect(CameraMixerTimelineBehaviour input)
        {
            foreach (var postProduction in input.cameraPostProductions)
            {
                if (postProduction.progress != 0f)
                {
                    postProduction.UpdateEffect(input.liveCamera, 0f, 0f);
                }
            }
        }

        public void RenameCameraByClipName()
        {
            var cameraClipDic = new Dictionary<Camera, string>();
            foreach (var clip in timelineClips)
            {
                var asset = clip.asset as CameraMixerTimelineClip;
                if (asset == null) continue;
                var clipName = clip.displayName;

                var camera = asset.behaviour.liveCamera.TargetCamera;
                if (camera == null) continue;

                Debug.Log($"{clipName} {camera.gameObject.name}");
                if (cameraClipDic.ContainsKey(camera))
                {
                    Debug.Log("ContainsKey");
                    cameraClipDic[camera] = $"{cameraClipDic[camera]}_{clipName}";
                }
                else
                {
                    Debug.Log("Not ContainsKey");
                    cameraClipDic.Add(camera, clipName);
                }
            }

            foreach (var camera in cameraClipDic.Keys)
            {
                Debug.Log(cameraClipDic[camera]);
                if (camera == null) continue;
                camera.gameObject.name = cameraClipDic[camera];
            }
        }

        private void SetCameraQueue(List<CameraMixerClipInfo> clips)
        {
            if (cameraMixer == null) return;
            if (clips.Count <= 0) return;

            if (clips.Count == 1)
            {
                if (clips[0].liveCamera.TargetCamera == null) return;
                cameraMixer.SetCameraQueue(clips[0].liveCamera, null, 0, clips[0].fadeMaterialSettingIndex);
            }
            else if (clips.Count == 2)
            {
                if (clips[0].liveCamera.TargetCamera == null || clips[1].liveCamera.TargetCamera == null) return;
                var dissolveWeight = clips[1].inputWeight == 0f ? 0f : 1f - clips[0].inputWeight;
                cameraMixer.SetCameraQueue(clips[0].liveCamera, clips[1].liveCamera, dissolveWeight,
                    clips[0].fadeMaterialSettingIndex);
                // Debug.Log($"A:{clips[0].liveCamera.TargetCamera.name} {clips[0].inputWeight}, B:{clips[1].liveCamera.TargetCamera.name} {clips[1].inputWeight}");
            }
        }
    }
}