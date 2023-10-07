using System;
using System.Collections;
using System.Collections.Generic;
using CameraLiveProduction;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
using UnityEngine.UIElements;


using Iridescent.Timeline;


public class CameraMixerCollectTrackTool : EditorWindow
{
    [MenuItem("Window/CameraLiveProduction/Collect Timeline Track Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(CameraMixerCollectTrackTool));
        window.titleContent = new GUIContent("Camera Mixer Collect Track Tool");
    }

    public PlayableDirector playableDirector;

    public PopupField<string> popupField;

    public CameraMixerTimelineTrack targetTrack;

    // public CameraSwitcherControlTrack cameraSwitcherControlTrack;
    public Button collectButton;

    // public Button renameCameraButton;
    private Dictionary<string, CameraMixerTimelineTrack> cameraSwitcherControlTracks =
        new Dictionary<string, CameraMixerTimelineTrack>();

    // public List<CameraSwitcherControlTrack> cameraSwitcherControlTracks = new List<CameraSwitcherControlTrack>();
    public void OnEnable()
    {
        var root = rootVisualElement;

        var objectField = new ObjectField("Playable Director");

        objectField.objectType = typeof(PlayableDirector);
        objectField.RegisterValueChangedCallback(evt =>
        {
            playableDirector = evt.newValue as PlayableDirector;
            var timelineAsset = playableDirector.playableAsset as TimelineAsset;
            var tracks = timelineAsset.GetOutputTracks();
            foreach (var track in tracks)
            {
                if (track is CameraMixerTimelineTrack)
                {
                    cameraSwitcherControlTracks.Add(track.name, track as CameraMixerTimelineTrack);
                }
            }


            InitPopup();
        });
        popupField = new PopupField<string>();

        collectButton = new Button(() =>
        {
            if (targetTrack != null) CollectAllTimelineClips(targetTrack);
        });

        if (targetTrack != null) collectButton.text = "Collect clips to " + targetTrack.name;
        else collectButton.text = "Collect";

        collectButton.SetEnabled(false);


        var renameCameraByClipNameButton = new Button();

        renameCameraByClipNameButton.text = "Rename Camera By Clip Name";
        renameCameraByClipNameButton.clicked += CollectAllClipsToNewTrack;

        root.Add(objectField);
        root.Add(popupField);
        root.Add(collectButton);
        root.Add(renameCameraByClipNameButton);
    }

    public void InitPopup()
    {
        if (cameraSwitcherControlTracks.Count > 0)
        {
            collectButton.SetEnabled(true);
            var options = new List<string>();
            foreach (var cameraSwitcherControlTrack in cameraSwitcherControlTracks)
            {
                options.Add(cameraSwitcherControlTrack.Key);
            }

            popupField.choices = options;
            popupField.RegisterValueChangedCallback(evt =>
            {
                targetTrack = cameraSwitcherControlTracks[evt.newValue];
            });
        }
        else
        {
            collectButton.SetEnabled(false);
            popupField.Clear();
        }
    }


    // 指定したTimelineAsset内のTimelineClipの内、asset に CameraMixerTimelineClip を持つものを時系列順に取得
    // ControlTrack がある場合、その内部の PlayableAsset 内の CameraMixerTimelineClip も再帰的に取得する
    public List<TimelineClip> GetCameraMixerTimelineClips(TimelineAsset timelineAsset)
    {
        Debug.Log("GetCameraMixerTimelineClips");
        var result = new List<TimelineClip>();
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            result.AddRange(GetCameraMixerTimelineClips(track));
        }

        Debug.Log("GetCameraMixerTimelineClips End: " + result.Count + " clips found");
        result.Sort((a, b) => a.start.CompareTo(b.start));
        return result;
    }

    public List<TimelineClip> GetCameraMixerTimelineClips(TrackAsset trackAsset)
    {
        var result = new List<TimelineClip>();
        if (trackAsset is CameraMixerTimelineTrack)
        {
            Debug.Log("GetCameraMixerTimelineClips: " + trackAsset.name + " is CameraMixerTimelineTrack");
            var cameraSwitcherControlTrack = trackAsset as CameraMixerTimelineTrack;
            cameraSwitcherControlTrack.muted = true;
            result.AddRange(cameraSwitcherControlTrack.GetClips());
        }
        else if (trackAsset is ControlTrack)
        {
            // 入れ子になったTimelineAssetを再帰的に取得
            Debug.Log("GetCameraMixerTimelineClips: " + trackAsset.name + " is ControlTrack");
            foreach (var clip in trackAsset.GetClips())
            {
                var playableAsset = clip.asset as ControlPlayableAsset;
                if (playableAsset == null) continue;
                Debug.Log("Find ControlPlayableAsset: " + playableAsset.name);
                // var timelinePlayable = playableAsset.sourceGameObject.Resolve(　

                // var srcGameObject = playableAsset.sourceGameObject.Resolve()
                //
                //
                // var timelineAsset2 = timelinePlayable as TimelineAsset;
                // if (timelineAsset2 == null) continue;
                // Debug.Log("!!Find nested timeline: " + timelineAsset2.name);
                // result.AddRange(GetCameraMixerTimelineClips(timelineAsset2));
            }
        }
        else
        {
            foreach (var clip in trackAsset.GetClips())
            {
                var playableAsset = clip.asset as PlayableAsset;
                if (playableAsset == null) continue;
                var timelineAsset2 = playableAsset as TimelineAsset;
                if (timelineAsset2 == null) continue;
                Debug.Log("??Find nested timeline: " + timelineAsset2.name);
                result.AddRange(GetCameraMixerTimelineClips(timelineAsset2));
            }
        }

        return result;
    }


    // private List<TimelineClip> newClipList =　new List<TimelineClip>();

    [ContextMenu("Collect")]
    public void CollectAllTimelineClips(CameraMixerTimelineTrack cameraSwitcherControlTrack)
    {
        Debug.Log("CollectAllTimelineClips");
        // newClipList.Clear();
        var timelineAsset = playableDirector.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogWarning("TimelineAsset is null");
            return;
        }

        // cameraSwitcherControlTrack のクリップをすべて削除
        foreach (var clip in cameraSwitcherControlTrack.GetClips())
        {
            cameraSwitcherControlTrack.DeleteClip(clip);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Delete All Clips");

        // clip を複製して cameraSwitcherControlTrack に追加
        foreach (var clip in GetCameraMixerTimelineClips(timelineAsset))
        {
            Debug.Log(clip.start + ":" + clip.displayName);

            var newClip = cameraSwitcherControlTrack.CreateDefaultClip();
            newClip.start = clip.start;
            newClip.duration = clip.duration;
            newClip.displayName = clip.displayName;
            // var cameraMixerTimelineClip = clip.asset as CameraMixerTimelineClip;
            // var newCameraMixerTimelineClip = newClip.asset as CameraMixerTimelineClip;
            // newCameraMixerTimelineClip.camera = cameraMixerTimelineClip.camera;
            // newCameraMixerTimelineClip.material = cameraMixerTimelineClip.material;
            var newCameraMixerTimelineClip = Instantiate(clip.asset) as CameraMixerTimelineClip;
            newClip.asset = newCameraMixerTimelineClip;

            EditorUtility.SetDirty(newClip.asset);
        }

        // 変更を保存
        AssetDatabase.SaveAssets();

        cameraSwitcherControlTrack.muted = false;
        // UIを更新
    }


    [ContextMenu("Collect and Create new Track")]
    public void CollectAllClipsToNewTrack()
    {
    }
}