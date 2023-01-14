using System;
using System.Collections;
using System.Collections.Generic;
using CameraLiveProduction;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class CameraMixerRenameTool : EditorWindow
{
    
    [MenuItem("Window/CameraLiveProduction/Rename Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(CameraMixerRenameTool));
        window.titleContent = new GUIContent("Camera Mixer Rename Tool");
    }

    public string namingRole = "<Scene>_C<ClipIndex>_<StartFrame>_<EndFrame>";
    public PlayableDirector playableDirector;
    
    public PopupField<string> popupField;
    
    public CameraMixerTimelineTrack targetTrack;
    // public CameraSwitcherControlTrack cameraSwitcherControlTrack;
    public Button renameButton;
    
    // public Button renameCameraButton;
    private Dictionary<string, CameraMixerTimelineTrack> cameraSwitcherControlTracks = new Dictionary<string, CameraMixerTimelineTrack>();
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
                    cameraSwitcherControlTracks.Add( track.name, track as CameraMixerTimelineTrack);
                }
            }
            
            
            
            InitPopup();
        });
        var textField = new TextField("Naming Role");
        textField.value = namingRole;
        textField.RegisterValueChangedCallback(evt =>
        {
            namingRole = evt.newValue;
        });
        popupField = new PopupField<string>();
        
        renameButton = new Button(() =>
        {
            if(targetTrack != null)RenameAllTimelineClips(targetTrack);
            
        });
        
        if(targetTrack != null)renameButton.text = "Rename " + targetTrack.name + " Camera";
        else renameButton.text = "Rename";
        
        renameButton.SetEnabled(false);



        var renameCameraByClipNameButton = new Button();

        renameCameraByClipNameButton.text = "Rename Camera By Clip Name";
        renameCameraByClipNameButton.clicked += RenameCameraByClipName;
       
        root.Add(objectField);
        root.Add(popupField);
        root.Add(textField);
        root.Add(renameButton);
        root.Add(renameCameraByClipNameButton);
        
    }

    private string ConvertCameraName(string rule, float frameRate,int clipIndex, TimelineClip clip)
    {
        var result = rule;
        var number = String.Format("{0:D2}", clipIndex);
        var startFrame = String.Format("{0:D4}", Mathf.CeilToInt((float)( frameRate*clip.start)));
        var endFrame = String.Format("{0:D4}", Mathf.CeilToInt((float)(frameRate*clip.end))-1);
        result = result.Replace("<Scene>", playableDirector.gameObject.scene.name);
        result = result.Replace("<Director>", playableDirector.playableAsset.name);
        result = result.Replace("<ClipIndex>", number);
        result = result.Replace("<StartFrame>", startFrame);
        result = result.Replace("<EndFrame>", endFrame);
        return result;
    }
    public void InitPopup()
    {

        if (cameraSwitcherControlTracks.Count > 0)
        {
            renameButton.SetEnabled(true);
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
            renameButton.SetEnabled(false);
            popupField.Clear();
        }
     
        
    }
    
    
    
    
    private Dictionary<CameraMixerTimelineClip,string> newClipNameDict = new Dictionary<CameraMixerTimelineClip,string>();
    [ContextMenu("Rename")]
    public void RenameAllTimelineClips(CameraMixerTimelineTrack cameraSwitcherControlTrack)
    {
        newClipNameDict.Clear();
        var i = 0;
        var timelineAsset = playableDirector.playableAsset as TimelineAsset;
        if(timelineAsset == null) return;
        
        foreach (var clip in
                 cameraSwitcherControlTrack.GetClips())
        {
           var clipName = ConvertCameraName(namingRole, (float)timelineAsset.editorSettings.frameRate, i, clip);
            clip.displayName = clipName;
            clip.asset.name = clip.displayName;
            EditorUtility.SetDirty(clip.asset);
            AssetDatabase.SaveAssets();
            newClipNameDict.Add(clip.asset as CameraMixerTimelineClip,ConvertCameraName("<ClipIndex>_<StartFrame>_<EndFrame", (float)timelineAsset.editorSettings.frameRate, i, clip));
            i++;
        }
        
        RenameCameraByClipName();
    }
    
    
     // public void RenameVolumeProfiles(CameraMixerTimelineTrack cameraSwitcherControlTrack)
     //    {
     //        var i = 0;
     //        foreach (var clip in
     //                 cameraSwitcherControlTrack.GetClips())
     //        {
     //            var cameraSwitcherControlClip = clip.asset as CameraMixerTimelineClip;
     //            if (cameraSwitcherControlClip && cameraSwitcherControlClip.volumeProfile)
     //            {
     //                cameraSwitcherControlClip.volumeProfile.name = $"C{i}_{clip.displayName}_volume";
     //                var path = AssetDatabase.GetAssetPath(cameraSwitcherControlClip.volumeProfile);
     //                AssetDatabase.RenameAsset(path, $"C{i}_{clip.displayName}_volume");    
     //                
     //                EditorUtility.SetDirty(cameraSwitcherControlClip.volumeProfile);
     //                AssetDatabase.SaveAssets();
     //            }
     //
     //            i++;
     //
     //        }
     //        
     //        
     //        
     //    }

    
    [ContextMenu("RenameCameraByClipName")]
    public void RenameCameraByClipName()
    {
        Debug.Log("clicked");
        var cameraClipDic = new Dictionary<Camera, string>();
        var clips = targetTrack.GetClips();
        
        foreach (var clip in clips)
        {
            Debug.Log(clip.asset.GetType());
       
            var asset = clip.asset as CameraMixerTimelineClip;
            if(asset == null) continue;
            var clipName = clip.displayName;
            var camera = asset.behaviour.liveCamera.GetComponent<Camera>();
            if(camera == null) continue;
            // Debug.Log($"{clip.displayName} {asset.newExposedReference.Resolve(playableDirector.playableGraph.GetResolver())}");
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
            camera.gameObject.name = cameraClipDic[camera];
        }
        
    }

    public void SortTransform(CameraMixer cameraSwitcherControl)
    {
        var childrens = new List<Transform>();
        foreach (Transform t in cameraSwitcherControl.transform)
        {
            childrens.Add(t);
        }
        childrens.Sort(delegate(Transform x, Transform y)
        {
            if (x == null && y == null) return 0;
           
            else return x.name.CompareTo(y.name);
        });


        var i = 0;
        foreach (var c in childrens)
        {
            c.SetSiblingIndex(i);
            i++;
        }
    }

    // アルファベット順に並べ替え
   
    static void SortByName()
    {
        Sort((a, b) => string.Compare(a.name, b.name));
    }
    
    static void Sort(Comparison<Transform> compare)
    {
        
        // foreach (Transform child in cameraSwitcherControl.gameObject.transform)
        // {

        // var childrens = new List<Transform>();
       
        //     var sorted = cameraSwitcherControl.gameObject.transform.chil
        //     sorted.Sort(compare);
        //
        //     var indices = sorted.Select(s => s.GetSiblingIndex()).OrderBy(s => s).ToList();
        //
        //     for (int i = 0; i < sorted.Count; i++)
        //     {
        //         Undo.SetTransformParent(sorted[i], sorted[i].parent, "Sort");
        //         sorted[i].SetSiblingIndex(indices[i]);
        //     }
        // // }
    }
}
