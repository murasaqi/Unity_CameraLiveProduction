using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CameraLiveProduction
{
    [CustomEditor(typeof(CameraMixerTimelineClip))]
    public class CameraSwitcherTimelineClipEditor : Editor
    {
        public List<string> materialSettingNames = new List<string>();
        public CameraMixerTimelineClip cameraSwitcherTimelineClip;

        public override void OnInspectorGUI()
        {
            cameraSwitcherTimelineClip = serializedObject.targetObject as CameraMixerTimelineClip;

            var settings = cameraSwitcherTimelineClip.liveCamera.cameraMixer.fadeMaterialSettings;
            for (var i = 0; i < settings.Count; i++)
            {
                materialSettingNames.Add(settings[i].name);
            }

            BeginInspector();
        }

        private void BeginInspector()
        {
            serializedObject.Update();


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("camera"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("debugRenderTexture"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }


            var fadeMaterialSettingIndex = serializedObject.FindProperty("fadeMaterialSettingIndex");
            // create a list of strings to choose from
            // var options = new List<string>();
            if (cameraSwitcherTimelineClip.liveCamera != null &&
                cameraSwitcherTimelineClip.liveCamera.cameraMixer != null)
            {
                var index = materialSettingNames.IndexOf(cameraSwitcherTimelineClip.liveCamera.cameraMixer
                    .currentFadeMaterialSetting
                    .name);
                if (index < 0) index = 0;
                index = EditorGUILayout.Popup("Fade Material Setting", index, materialSettingNames.ToArray());

                if (index != fadeMaterialSettingIndex.intValue)
                {
                    fadeMaterialSettingIndex.intValue = index;
                    serializedObject.ApplyModifiedProperties();
                }
            }


            if (fadeMaterialSettingIndex.intValue == 0)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("multiplyColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("colorBlendMode"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }


            // DrawAddPropertyButton(cameraSwitcherTimelineClip);
            // DrawPropertyInInspector(stageLightProfile.FindProperty("stageLightProperties"));
        }

        private void DrawAddPropertyButton(CameraMixerTimelineClip cameraMixerTimelineClip)
        {
            EditorGUI.BeginChangeCheck();

            // var propertyTypes = SlmUtility.GetTypes(typeof(SlmAdditionalProperty));

            // propertyTypes.Remove(typeof(RollProperty));
            var selectList = new List<string>();

            CameraLiveSwitcherUtility.CameraPostProductionTypes.ForEach(t =>
            {
                if (t != typeof(CameraPostProductionBase)) selectList.Add(t.Name);
            });


            // var typeDict = new Dictionary<string, Type>();

            selectList.Insert(0, "Add Effect");
            foreach (var property in cameraMixerTimelineClip.behaviour.cameraPostProductions
                    )
            {
                if (property == null) continue;
                if (selectList.Find(x => x == property.GetType().Name) != null)
                {
                    selectList.Remove(property.GetType().Name);
                }
            }

            EditorGUI.BeginDisabledGroup(selectList.Count <= 1);
            var select = EditorGUILayout.Popup(0, selectList.ToArray());
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                var type = CameraLiveSwitcherUtility.GetTypeByClassName(selectList[select]);
                var property = Activator.CreateInstance(type) as CameraPostProductionBase;
                // Debug.Log(cameraMixerTimelineClip.clone.liveCamera);
                property?.Initialize(cameraMixerTimelineClip.clone.liveCamera);
                cameraMixerTimelineClip.behaviour.cameraPostProductions.Add(property);
            }
        }
    }
}