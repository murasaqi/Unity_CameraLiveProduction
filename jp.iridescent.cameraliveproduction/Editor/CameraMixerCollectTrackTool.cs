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

        renameCameraByClipNameButton.text = "Collect and Create new Track";
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
    

    // Scene内の全てのPlayableDirectorのうち、Binding対象に CameraMixerTimelineTrack を持つものを取得
    public List<PlayableDirector> GetPlayableDirectors()
    {
        var result = new List<PlayableDirector>();
        var playableDirectors = FindObjectsOfType<PlayableDirector>(true);
        foreach (var playableDirector in playableDirectors)
        {
            var playableAsset = playableDirector.playableAsset as TimelineAsset;
            if (playableAsset == null) continue;
            foreach (var track in playableAsset.GetOutputTracks())
            {
                if (track is CameraMixerTimelineTrack)
                {
                    result.Add(playableDirector);
                    break;
                }
            }
        }

        return result;
    }


    // private List<TimelineClip> newClipList =　new List<TimelineClip>();

    [ContextMenu("Collect")]
    public void CollectAllTimelineClips(CameraMixerTimelineTrack targetCameraSwitcherControlTrack)
    {
        var timelineAsset = playableDirector.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            // Debug.LogWarning("TimelineAsset is null");
            return;
        }

        // cameraSwitcherControlTrack のクリップをすべて削除
        foreach (var clip in targetCameraSwitcherControlTrack.GetClips())
        {
            targetCameraSwitcherControlTrack.DeleteClip(clip);
        }

        AssetDatabase.SaveAssets();
        // Debug.Log("Delete All Clips");
        var playableDirectors = GetPlayableDirectors();

        foreach (var track in timelineAsset.GetOutputTracks())
        {
            // トラックが CustomControlTrack の場合
            if (track is CustomControlTrack)
            {
                foreach (var clip in track.GetClips())
                {
                    // CustomControlClip の開始offset
                    var startTimeOffset = clip.start - clip.clipIn;
                    // CustomControlClip にセットされたゲームオブジェクトの取得
                    var playableAsset = clip.asset as CustomControlPlayableAsset;
                    if (playableAsset == null) continue;
                    var gameObject = playableAsset.sourceGameObject.Resolve(playableDirector);

                    foreach (var director in playableDirectors)
                    {
                        // CustomControlClip にセットされたゲームオブジェクトと同じゲームオブジェクトのPlayableDirectorを探す
                        if (director.gameObject.Equals(gameObject) == false)
                            continue;

                        // CustomControlClip にセットされたPlayableDirectorのTimelineAssetを取得
                        var nestedTimelineAsset = director.playableAsset as TimelineAsset;
                        if (nestedTimelineAsset == null)
                            continue;

                        // CustomControlClip にセットされたTimelineAssetのトラックを取得
                        foreach (var nestedTrack in nestedTimelineAsset.GetOutputTracks())
                        {
                            if (nestedTrack is not CameraMixerTimelineTrack)
                                continue;
                            // Debug.Log("GetCameraMixerTimelineClips: " + nestedTrack.name +
                            //           " is CameraMixerTimelineTrack");
                            var cameraSwitcherControlTrack = nestedTrack as CameraMixerTimelineTrack;
                            cameraSwitcherControlTrack.muted = true;

                            // CustomControlClip 内の CameraSwitcherControlTrack のクリップを処理
                            foreach (var cameraSwitcherClip in cameraSwitcherControlTrack.GetClips())
                            {
                                var newClip = targetTrack.CreateDefaultClip();
                                newClip.displayName = cameraSwitcherClip.displayName;
                                newClip.duration = cameraSwitcherClip.duration;
                                newClip.start = cameraSwitcherClip.start + startTimeOffset;
                                var clipAsset = newClip.asset as CameraMixerTimelineClip;
                                var refAsset = cameraSwitcherClip.asset as CameraMixerTimelineClip;

                                // Debug.Log(refAsset.liveCamera);

                                // ここでカメラの参照を設定する

                                var resolvedCamera = refAsset.camera.Resolve(director);
                                // Debug.Log("resolvedCam: "+ resolvedCamera);
                                    clipAsset.camera = new ExposedReference<LiveCamera>()
                                {
                                    defaultValue = resolvedCamera,
                                    // exposedName = refAsset.camera.exposedName
                                };

                                clipAsset.material = refAsset.material;
                                EditorUtility.SetDirty(newClip.asset);
                            }
                        }
                    }
                }
            }

            EditorUtility.SetDirty(timelineAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }


    [ContextMenu("Collect and Create new Track")]
    public void CollectAllClipsToNewTrack()
    {
        var timelineAsset = playableDirector.playableAsset　as TimelineAsset;
        // 新しいトラックを作成
        var newTrack = timelineAsset.CreateTrack<CameraMixerTimelineTrack>(null, "CameraSwitcherControlTrack");
        CollectAllTimelineClips(newTrack);
    }
}