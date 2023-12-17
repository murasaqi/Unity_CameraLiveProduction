using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

#if USE_URP
using UnityEngine.Rendering.Universal;
#endif

#if USE_CINEMACHINE
using Cinemachine;
#endif


namespace CameraLiveProduction
{

    
    [Serializable]
    [ExecuteAlways]
    public class MultiLiveCamera:LiveCameraBase
    {

        [SerializeField] private List<Camera> cameras;
        // private Vector2Int cameraMixerResolution = new Vector2Int(1920,1080);
        [SerializeField] private Vector2Int individualResolution = new Vector2Int  (600,1080);
        // private Vector2Int _individualResolution = new Vector2Int  (0,0);
        // private List<RenderTexture> renderTextures = new List<RenderTexture>();
        Dictionary<Camera,RenderTexture> renderTextures = new Dictionary<Camera, RenderTexture>();
        [SerializeField] private Material multiTextureSplitterMat;
        [SerializeField] private CustomRenderTexture customRenderTexture;
        private void OnEnable()
        {
            Initialize();
        }

        [ContextMenu("Initialize")]
        public override void Initialize()
        {
            foreach (var camera in cameras)
            {
                if(!camera) continue;
                camera.enabled = false;
            }
            
        }

        public override RenderTexture TargetTexture
        {
            get => customRenderTexture;
            set
            {
                
            }
        }


        public override void SetEnableTargetCamera(bool enable)
        {
            foreach (var camera in cameras)
            {
                camera.enabled = enable;
            }

        }

        private void LateUpdate()
        {
            // Render();
        }

        public override void UpdateLiveCamera()
        {
         
            if (cameras.Count != renderTextures.Count)
            {
                InitializeRenderTexture( );
            }
           
            
            foreach (var renderTexture in renderTextures)
            {
                if(renderTexture.Value.width != individualResolution.x ||
                     renderTexture.Value.height != individualResolution.y)
                 { 
                    renderTexture.Value.Release();
                    renderTexture.Key.targetTexture = null;
                    renderTextures[renderTexture.Key] = new RenderTexture(individualResolution.x ,individualResolution.y,(int)cameraMixer.depthStencilFormat, cameraMixer.format);
                    renderTexture.Key.targetTexture = renderTextures[renderTexture.Key];
                     break;
                 }
                    
            }

           var i = 0;
           foreach (var renderTexture in renderTextures)
           {
               if(multiTextureSplitterMat)
               {
                   multiTextureSplitterMat.SetTexture($"_Texture2D_0{i+1}",renderTexture.Value);
               }
               i++;
           }
           
           
           if(multiTextureSplitterMat)
           {
               multiTextureSplitterMat.SetVector("_InputTextureResolution",new Vector4(individualResolution.x,individualResolution.y,0,0));
               multiTextureSplitterMat.SetVector("_OutputResolution",new Vector4(cameraMixer.width,cameraMixer.height,0,0));
           }
           
           // if(customRenderTexture) customRenderTexture.Update();
        }
        [ContextMenu("InitializeRenderTexture")]
        private void InitializeRenderTexture()
        {
            Debug.Log("InitializeRenderTexture");
            if(!cameraMixer)return;
            
            foreach (var camera in cameras)
            {
                if(!camera) continue;
                camera.targetTexture = null;
            }
            foreach (var renderTexture in renderTextures)
            {
                
                if (renderTexture.Value)
                {
                    renderTexture.Value.Release();
                    DestroyImmediate(renderTexture.Value);
                }
            }

            renderTextures.Clear();
            
            foreach (var camera in cameras)
            {
                if(!camera) continue;
                
                var renderTexture = new RenderTexture(individualResolution.x ,individualResolution.y,(int)cameraMixer.depthStencilFormat, cameraMixer.format);
                // renderTexture.Create();
                renderTextures.Add(camera,renderTexture);
                camera.targetTexture = renderTexture;
                
                if(multiTextureSplitterMat)
                {
                    multiTextureSplitterMat.SetTexture($"_Texture2D_0{cameras.IndexOf(camera)+1}",renderTexture);
                }
            }
            
            
        }
        
        private void DestroyRenderTexture()
        {
            foreach (var renderTexture in renderTextures)
            {
                renderTexture.Value.Release();
                DestroyImmediate(renderTexture.Value);
            }

            renderTextures.Clear();
        }


        // public override Camera camera => targetCamera;
    }
}