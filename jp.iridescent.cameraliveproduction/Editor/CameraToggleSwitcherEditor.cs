using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace CameraLiveProduction
{
    [CustomEditor(typeof(CameraToggleSwitcher))]
    [CanEditMultipleObjects]
    public class CameraToggleSwitcherEditor : Editor
    {
        private CameraToggleSwitcher cameraToggleSwitcher;
        private Image camera1Image;
        private Image camera2Image;
        private Image outputImage;
        private float previewWidth = -1;
        private VisualElement root;

        private RenderTexture outputThumbnail;
        private Vector2 aspectRatio = Vector2.one;
        public DropdownField popUpField1;
        public DropdownField popUpField2;
        public VisualElement camera1Preview;
        public VisualElement camera2Preview;
        
        public override VisualElement CreateInspectorGUI()
        {

            DestroyInstantiateObjects();
            cameraToggleSwitcher = serializedObject.targetObject as CameraToggleSwitcher;
            
            outputThumbnail = new RenderTexture((int)(cameraToggleSwitcher.width*0.1f), (int)(cameraToggleSwitcher.height*0.1), 0,RenderTextureFormat.DefaultHDR);
            root = Resources.Load<VisualTreeAsset>("CameraSwitcherResources/CameraToggleSwitcherEditorGUI")
                .CloneTree("CameraToggleSwitcher");
            
            camera1Preview = root.Q<VisualElement>("Camera1Preview");
            camera1Image = new Image();
            camera1Preview.Add(camera1Image);
            camera1Image.image = cameraToggleSwitcher.renderTexture1;
            
            
            camera2Preview = root.Q<VisualElement>("Camera2Preview");
            camera2Image = new Image();
            camera2Preview.Add(camera2Image);
            camera2Image.image = cameraToggleSwitcher.renderTexture2;

            outputImage = new Image();
            var outputPreview = root.Q<VisualElement>("OutputPreview");
            outputPreview.Add(outputImage);
            if(cameraToggleSwitcher.outputTarget)outputImage.image = cameraToggleSwitcher.outputTarget;
            
            
            var resolutionField = root.Q<Vector2IntField>("ResolutionField");
            resolutionField.value = new Vector2Int(cameraToggleSwitcher.width, cameraToggleSwitcher.height);
            resolutionField.RegisterValueChangedCallback((v) =>
            {
                cameraToggleSwitcher.width = v.newValue.x;
                cameraToggleSwitcher.height = v.newValue.y;
                cameraToggleSwitcher.InitRenderTextures();
                RefreshGUI();
            });
            var xInput = resolutionField.Q<VisualElement>("unity-x-input");
            var yInput = resolutionField.Q<VisualElement>("unity-y-input");
            xInput.Q<Label>().text = "W";
            yInput.Q<Label>().text = "H";
            
            var resolutionScaleField = root.Q<EnumField>("ResolutionScaleField");
            resolutionScaleField.RegisterValueChangedCallback((v) =>
            {
                cameraToggleSwitcher.Initialize();
                RefreshGUI();
            });
            
            var antiAliasingField = root.Q<EnumField>("RTAntiAliasingField");
            antiAliasingField.RegisterValueChangedCallback((v) =>
            {
                cameraToggleSwitcher.Initialize();
                RefreshGUI();
            });

            resolutionField.RegisterValueChangedCallback((v) =>
            {
                cameraToggleSwitcher.Initialize();
                RefreshGUI();
            });
            var cam1Field = root.Q<ObjectField>("Cam1Field");
            cam1Field.objectType = typeof(Camera);
            cam1Field.RegisterValueChangedCallback((v) =>
            {
                cameraToggleSwitcher.ApplyRenderTextureToTargets();
                RefreshGUI();
            });
            var cam2Field = root.Q<ObjectField>("Cam2Field");
            cam2Field.objectType = typeof(Camera);
            cam2Field.RegisterValueChangedCallback((v) =>
            {
                cameraToggleSwitcher.ApplyRenderTextureToTargets();
                RefreshGUI();
            });

            
            var depthStencilFormatField = root.Q<EnumField>("RTDepthField");
            depthStencilFormatField.RegisterValueChangedCallback((v) =>
            {
                cameraToggleSwitcher.Initialize();
                RefreshGUI();
            });
            
            root.Q<Button>("InitializeButton").clicked += () =>
            {
                cameraToggleSwitcher.Initialize();
                RefreshGUI();
            };
            
            root.Q<PropertyField>("OutputImage").RegisterValueChangeCallback((v) =>
            {
                cameraToggleSwitcher.Initialize();
                RefreshGUI();
            });
            
            
            root.Q<PropertyField>("OutputTarget").RegisterValueChangeCallback((v) =>
            {
                cameraToggleSwitcher.Initialize();
                RefreshGUI();
            });
            popUpField1 = root.Q<DropdownField>("CameraList1");
            var cameraList = new List<string>();
            // convert cameraMixer.cameraList to camera name list
            if (cameraToggleSwitcher.cameraList != null)
            {
                foreach (var camera in cameraToggleSwitcher.cameraList)
                {
                    if(camera != null)cameraList.Add(camera.name);
                }
    
            }
            
            popUpField1.choices = cameraList;
            popUpField1.index = cameraToggleSwitcher.camera1Queue == null ? -1 : cameraToggleSwitcher.cameraList.IndexOf(cameraToggleSwitcher.camera1Queue);
            popUpField1.RegisterValueChangedCallback((v) =>
            {
                if(cameraToggleSwitcher.cameraList.IndexOf(cameraToggleSwitcher.camera1Queue) == popUpField1.index) return;
                var index = popUpField1.index;
                cameraToggleSwitcher.camera1Queue = index >= 0 && index < cameraToggleSwitcher.cameraList.Count ? cameraToggleSwitcher.cameraList[index] : null;
            });
            popUpField2 = root.Q<DropdownField>("CameraList2");
            popUpField2.choices = cameraList;
            popUpField2.index = cameraToggleSwitcher.camera2Queue == null ? -1 : cameraToggleSwitcher.cameraList.IndexOf(cameraToggleSwitcher.camera2Queue);
            popUpField2.RegisterValueChangedCallback((v) =>
            {
                if(cameraToggleSwitcher.cameraList.IndexOf(cameraToggleSwitcher.camera2Queue) == popUpField2.index) return;
                var index = popUpField2.index;
                cameraToggleSwitcher.camera2Queue = index >= 0 && index < cameraToggleSwitcher.cameraList.Count ? cameraToggleSwitcher.cameraList[index] : null;
            });
            
            Resize();
            
            EditorApplication.update -= RefreshGUI; //増殖を防ぐ
            EditorApplication.update += RefreshGUI;
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
            
            return root;
        }

        public void Resize()
        {
            if(root == null) return;
            
            var camera1 = root.Q<VisualElement>("Camera1");
            var camera2 = root.Q<VisualElement>("Camera2");
            
            var aspectRatio = (float)cameraToggleSwitcher.height / (float)cameraToggleSwitcher.width;
            
            if (float.IsNaN(camera1.layout.width) || float.IsNaN(camera2.layout.width)) return;
            camera1Image.image = cameraToggleSwitcher.camera1Queue != null ? cameraToggleSwitcher.renderTexture1 : Texture2D.grayTexture;
            camera2Image.image = cameraToggleSwitcher.camera2Queue != null ? cameraToggleSwitcher.renderTexture2 : Texture2D.grayTexture;
            camera1Image.style.height =  camera1.layout.width * aspectRatio;
            camera2Image.style.height =  camera2.layout.width * aspectRatio;
            camera1Preview.style.height =   camera1Image.style.height;
            camera2Preview.style.height =   camera1Image.style.height;

            if (cameraToggleSwitcher.outputTarget != null)
            {
                outputImage.image = cameraToggleSwitcher.outputTarget;
            }
            else if (cameraToggleSwitcher.outputImage != null)
            {
                outputImage.image = outputThumbnail;

            }
            
            outputImage.style.height = camera1.layout.width * aspectRatio;
            
            previewWidth = camera1.layout.width;
            
        }

        private void OnDestroy()
        {
            DestroyInstantiateObjects();
        }

        private void OnDisable()
        {
            DestroyInstantiateObjects();
        }

        private void DestroyInstantiateObjects()
        {
            if (outputThumbnail != null)
            {
                outputThumbnail.Release();
                DestroyImmediate(outputThumbnail);
                
            }
        }
        void OnSelectionChanged()
        {
            if (serializedObject.targetObject != Selection.activeObject)
            {
                EditorApplication.update -= RefreshGUI;
                Selection.selectionChanged -= OnSelectionChanged;
            }
        }
        void RefreshGUI()
        {
            if(cameraToggleSwitcher == null) return;
            
            if (popUpField1 != null && popUpField1.index != cameraToggleSwitcher.cameraList.IndexOf(cameraToggleSwitcher.camera1Queue))
            {
                popUpField1.index = cameraToggleSwitcher.camera1Queue == null ? -1 : cameraToggleSwitcher.cameraList.IndexOf(cameraToggleSwitcher.camera1Queue);
                serializedObject.ApplyModifiedProperties();
            }
            if (popUpField2 != null && popUpField2.index != cameraToggleSwitcher.cameraList.IndexOf(cameraToggleSwitcher.camera2Queue))
            {
                popUpField2.index = cameraToggleSwitcher.camera2Queue == null ? -1 : cameraToggleSwitcher.cameraList.IndexOf(cameraToggleSwitcher.camera2Queue);
                serializedObject.ApplyModifiedProperties();
            }
            
            if (cameraToggleSwitcher.outputImage != null && cameraToggleSwitcher.outputTarget == null)
            {
                cameraToggleSwitcher.BlitOutputTarget(outputThumbnail);
            }
            Resize();
        }
        private void OnEnable()
        {
            Resize();
        }
        
    }
}