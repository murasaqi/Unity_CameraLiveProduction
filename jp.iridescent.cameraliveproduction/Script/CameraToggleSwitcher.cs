using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CameraLiveProduction
{
    [ExecuteAlways]
    public class CameraToggleSwitcher:MonoBehaviour
    {
        public Camera camera1Queue;
        public Camera camera2Queue;
        public Camera cam1;
        public Camera cam2;
        public int width = 1920;
        public int height = 1080;
        public ResolutionScale resolutionScale = ResolutionScale.x1;
        public RenderTextureFormat format = RenderTextureFormat.ARGB32;
        public DepthStencilFormat depthStencilFormat = DepthStencilFormat.D32_SFLOAT_S8_UINT;
        public RenderTexture renderTexture1;
        public RenderTexture renderTexture2;
        [Range(0, 1)] public float fader = 0f;
        public Shader shader;
        [SerializeField] private Material material;
        public AntiAliasing antiAliasing = AntiAliasing.NONE;
        public RenderTexture outputTarget;
        public RawImage outputImage;
        public CameraRenderTiming cameraRenderTiming = CameraRenderTiming.Update;
        public List<Camera> cameraList = new List<Camera>();
        
        public Vector2Int Resolution()
        {
            var scale = GetResolutionScale();
            return new Vector2Int(Mathf.CeilToInt(width * scale),  Mathf.CeilToInt(height * scale));
        }
        
        public void RemoveCameraTargetTexture()
        {

            foreach (var liveCamera in cameraList)
            {
                if(liveCamera)liveCamera.targetTexture = null;
            }
            if (cam1 != null)
            {
                cam1.targetTexture = null;
            }
            if (cam2 != null)
            {
                cam2.targetTexture = null;
            }
        }
        
        public void InitRenderTextures()
        {
            RemoveCameraTargetTexture();
            if (renderTexture1 != null)
            {
                renderTexture1.Release();
                DestroyImmediate(renderTexture1);
            }
            if (renderTexture2 != null)
            {
                renderTexture2.Release();
                DestroyImmediate(renderTexture2);
            }
            
            var res = Resolution();
            renderTexture1 = new RenderTexture(res.x, res.y, (int)depthStencilFormat, format);
            renderTexture1.antiAliasing = (int)antiAliasing;
            
            renderTexture2 = new RenderTexture(res.x, res.y, (int)depthStencilFormat, format);
            renderTexture2.antiAliasing = (int)antiAliasing;
           
            Render();
        }

        public void ApplyRenderTextureToTargets()
        {
            if(cam1 != null)cam1.targetTexture = renderTexture1;
            if(cam2 != null)cam2.targetTexture = renderTexture2;
            if(material != null) material.SetTexture("_TextureA", renderTexture1);
            if(material != null) material.SetTexture("_TextureB", renderTexture2);
        }
        
  
        
        [ContextMenu("Initialize")]
        public void Initialize()
        {
            if(material)DestroyImmediate(material);
            shader = Resources.Load<Shader>("CameraSwitcherResources/Shader/CameraSwitcherFader");
            material = new Material(shader);
            InitRenderTextures();
            ApplyRenderTextureToTargets();
            if (outputImage != null) outputImage.material = material;
        }

        // public void RefreshRender()
        // {
        //     InitRenderTextures();
        //     ApplyRenderTextureToTargets();
        // }
        //
        private void OnDestroy()
        {
            DestroyImmediate(material);
            if(cam1)cam1.targetTexture = null;
            if(cam2)cam2.targetTexture = null;
            if(renderTexture1)DestroyImmediate(renderTexture1);
            if(renderTexture2)DestroyImmediate(renderTexture2);
           
        }


        private void ApplyCameraQueue()
        {
            cam1 = camera1Queue;
            cam2 = camera2Queue;
            if (cam1 != null)
            {
                cam1.enabled = true;
                cam1.targetTexture = renderTexture1; 
            }

            if (cam2 != null)
            {
                cam2.enabled = true;
                cam2.targetTexture = renderTexture2;
            }
            
            // material.SetFloat("_CrossFade", fader);
            
        }
        
        
        public void BlitOutputTarget(RenderTexture dst)
        {
            Graphics.Blit(Texture2D.blackTexture, dst, material);
        }
        public void SetCameraQueue(Camera camera1, Camera camera2 = null, float blend = 0f)
        { 
            camera1Queue = camera1; 
            camera2Queue = camera2;
            fader = blend;
        }
        
        private void RefreshCamera()
        {
        
            foreach (var liveCamera in cameraList)
            {
                if(liveCamera == null) continue;

              
                if (liveCamera == cam1 || liveCamera == cam2)
                {
                    liveCamera.enabled = true;
                }
                else
                {
                    liveCamera.enabled = false;
                }
                
                liveCamera.targetTexture = null;    
                
            }
        }
        
        public void Render()
        {

            RefreshCamera();
            if(renderTexture1 == null || renderTexture2 == null || material == null)
            {
                Initialize();
            }

            if (outputTarget != null)
            {
                BlitOutputTarget(outputTarget);    
            }

            if (outputImage)
            {
                outputImage.material = material;
            }
            ApplyCameraQueue();
            material.SetFloat("_CrossFade", fader);

        }

      
        public void Update()
        {
            
            
            if(cameraRenderTiming == CameraRenderTiming.Update)
            {
                Render();
            }
        }
        
        public float GetResolutionScale()
        {
            switch (resolutionScale)
            {
                case ResolutionScale.x0_25:
                    return 0.25f;
                case ResolutionScale.x0_5:
                    return 0.5f;
                case ResolutionScale.x1:
                    return 1f;
                case ResolutionScale.x1_5:
                    return 1.5f;        
                case ResolutionScale.x2:
                    return 2f;
                default:
                    return 1f;
            }
        } 
        
    }
}