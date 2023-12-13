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
        private Vector2Int resolution = new Vector2Int  (1920,1080);
        private List<RenderTexture> renderTextures = new List<RenderTexture>();
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
                // camera.enabled = false;
            }
            
        }

        

        public override void SetEnableTargetCamera(bool enable)
        {
            
        }
        private void Update()
        {
           if(cameraMixer && cameraMixer.Resolution() != resolution)
               InitializeRenderTexture();
           if(cameras.Count != renderTextures.Count)
               InitializeRenderTexture( );
           
           foreach (var renderTexture in renderTextures)
           {
               if(multiTextureSplitterMat)
               {
                   multiTextureSplitterMat.SetTexture($"_Texture2D_0{renderTextures.IndexOf(renderTexture)+1}",renderTexture);
               } 
           }
           
           // if(customRenderTexture) customRenderTexture.Update();
        }
        [ContextMenu("InitializeRenderTexture")]
        private void InitializeRenderTexture()
        {
            Debug.Log("InitializeRenderTexture");
            if(!cameraMixer)return;
            resolution = cameraMixer.Resolution();
            
            foreach (var camera in cameras)
            {
                if(!camera) continue;
                camera.targetTexture = null;
            }
            foreach (var renderTexture in renderTextures)
            {
                
                if (renderTexture)
                {
                    renderTexture.Release();
                    DestroyImmediate(renderTexture);
                }
            }

            renderTextures.Clear();
            
            var w = resolution.x / cameras.Count;
            foreach (var camera in cameras)
            {
                if(!camera) continue;
                
                var renderTexture = new RenderTexture(w ,resolution.y,(int)cameraMixer.depthStencilFormat, cameraMixer.format);
                // renderTexture.Create();
                renderTextures.Add(renderTexture);
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
                renderTexture.Release();
                DestroyImmediate(renderTexture);
            }

            renderTextures.Clear();
        }


        // public override Camera camera => targetCamera;
    }
}