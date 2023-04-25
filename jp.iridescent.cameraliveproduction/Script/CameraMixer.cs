using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace CameraLiveProduction
{
    [Serializable]
    public enum DepthStencilFormat
    {
        NONE = 1,
        D16_UNORM = 16,
        D24_UNORM = 32,
        D24_UNORM_S8_UINT = 24,
        D32_SFLOAT = 32,
        D32_SFLOAT_S8_UINT = 32,
    }

    [Serializable]
    public enum ResolutionScale
    {
        x0_25,
        x0_5,
        x1,
        x1_5,
        x2,
    }

    [Serializable]
    public enum AntiAliasing
    {
        NONE = 0,
        x2 = 2,
        x4 = 4,
        x8 = 8,
    }

    [Serializable]

    public class CameraRenderQueue
    {
        public LiveCamera camera = null;
        public float inputWeight = 0.0f;
    }
    [ExecuteAlways]
    public class CameraMixer : MonoBehaviour
    {
        public bool useTimeline = false;
        public LiveCamera camera1Queue;
        public LiveCamera camera2Queue;
        public LiveCamera cam1;
        public LiveCamera cam2;
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
        public AntiAliasing antiAliasing = AntiAliasing.x2;
        public RenderTexture outputTarget;
        public RawImage outputImage;
        [SerializeReference]public List<CameraMixerPostEffectBase> cameraMixerPostEffectBases = new List<CameraMixerPostEffectBase>();
        public List<LiveCamera> cameraList = new List<LiveCamera>();
        public CameraRenderTiming cameraRenderTiming = CameraRenderTiming.Update;
        void Start()
        {

        }

        public Vector2Int Resolution()
        {
            var scale = GetResolutionScale();
            return new Vector2Int(Mathf.CeilToInt(width * scale),  Mathf.CeilToInt(height * scale));
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
            if(material != null) material.SetTexture("_TextureA", renderTexture1);
            if(material != null) material.SetTexture("_TextureB", renderTexture2);
            
        }

        public void RemoveCameraTargetTexture()
        {

            foreach (var liveCamera in cameraList)
            {
                if(liveCamera)liveCamera.TargetCamera.targetTexture = null;
            }
            if (cam1 != null)
            {
                cam1.TargetCamera.targetTexture = null;
            }
            if (cam2 != null)
            {
                cam2.TargetCamera.targetTexture = null;
            }
        }

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            if(material)DestroyImmediate(material);
            shader = Resources.Load<Shader>("CameraSwitcherResources/Shader/CameraSwitcherFader");
            material = new Material(shader);
            InitRenderTextures();
            if(cam1 != null)cam1.TargetCamera.targetTexture = renderTexture1;
            if(cam2 != null)cam2.TargetCamera.targetTexture = renderTexture2;
            material.SetTexture("_TextureA", renderTexture1);
            material.SetTexture("_TextureB", renderTexture2);
            // if contains null in camera list, remove it
            cameraList = cameraList.Where(x => x != null).ToList();
            
        }

        private void OnDestroy()
        {
            DestroyImmediate(material);
            if(cam1)cam1.TargetCamera.targetTexture = null;
            if(cam2)cam2.TargetCamera.targetTexture = null;
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
                cam1.TargetCamera.targetTexture = renderTexture1; 
            }

            if (cam2 != null)
            {
                cam2.enabled = true;
                cam2.TargetCamera.targetTexture = renderTexture2;
            }
            
            // material.SetFloat("_CrossFade", fader);
            
        }

        public void BlitOutputTarget(RenderTexture dst)
        {
            Graphics.Blit(Texture2D.blackTexture, dst, material);
        }
        public void SetCameraQueue(LiveCamera camera1, LiveCamera camera2 = null, float blend = 0f)
        { 
            camera1Queue = camera1; 
            camera2Queue = camera2;
            
            fader = blend;
        }
        
        private void UpdateCameraMixerPostEffect()
        {
            foreach (var cameraMixerPostEffectBase in cameraMixerPostEffectBases)
            {
                if(cameraMixerPostEffectBase!=null)cameraMixerPostEffectBase.UpdateEffect();
            }
        }
        
        private void RefreshCamera()
        {
        
            foreach (var liveCamera in cameraList)
            {
                if(liveCamera == null) continue;

                if (liveCamera.TargetCamera == null)
                {
                    liveCamera.Initialize();
                }
                if (liveCamera == cam1 || liveCamera == cam2)
                {
                  liveCamera.TargetCamera.enabled = true;
                }
                else
                {
                    liveCamera.TargetCamera.enabled = false;
                }
                
                liveCamera.TargetCamera.targetTexture = null;    
                
            }
        }

        public void Render()
        {

            UpdateCameraMixerPostEffect();
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
            foreach (var camera in cameraList)
            {
                if(camera)camera.cameraMixer = this;
            }
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
        // public void RenameCameraByClipName()
        // {
        //     var cameraClipDic = new Dictionary<Camera, string>();
        //     
        //     foreach (var clip in m_Clips)
        //     {
        //         var asset = clip.asset as CameraMixerTimelineClip;
        //         if(asset == null)continue;
        //         var clipName = clip.displayName;
        //         var camera = asset.behaviour.camera;
        //         if(camera == null) continue;
        //         // Debug.Log($"{asset.newExposedReference.Resolve( playableDirector.playableGraph.GetResolver()).gameObject.name} {clipName}");
        //         if (cameraClipDic.ContainsKey(camera))
        //         {
        //             Debug.Log("ContainsKey");
        //             cameraClipDic[camera] = $"{cameraClipDic[camera]}_{clipName}";
        //         }
        //         else
        //         {
        //             Debug.Log("Not ContainsKey");
        //             cameraClipDic.Add(camera,clipName);
        //         }
        //     }
        //
        //     foreach (var camera in cameraClipDic.Keys)
        //     {
        //         Debug.Log(cameraClipDic[camera]);
        //         if(camera == null) continue;
        //         camera.gameObject.name = cameraClipDic[camera];
        //     }
        // }
    }

}