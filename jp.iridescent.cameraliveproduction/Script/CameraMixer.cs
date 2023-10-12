using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
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
        public RenderTexture debugOverlayTexture;
        private Texture2D clearTexture;
        [Range(0, 1)] public float fader = 0f;
        public Shader shader;
        [SerializeField] private Material instantiatedFadeMaterial;
        public AntiAliasing antiAliasing = AntiAliasing.x2;
        public RenderTexture outputTarget;
        public RawImage outputImage;

        [SerializeReference]
        public List<CameraMixerPostEffectBase> cameraMixerPostEffectBases = new List<CameraMixerPostEffectBase>();

        public List<LiveCamera> cameraList = new List<LiveCamera>();
        public CameraRenderTiming cameraRenderTiming = CameraRenderTiming.Update;

        public Material fadeMaterial;

        private readonly List<RenderTexture> _renderTexturesToBeDestroyed = new List<RenderTexture>();
        private readonly List<Material> _materialsToBeDestroyed = new List<Material>();

        private void Start()
        {
            Initialize();
        }

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            _renderTexturesToBeDestroyed.RemoveAll(x => x == null);
            _materialsToBeDestroyed.RemoveAll(x => x == null);

            InitMaterial();
            InitRenderTextures();

            if (clearTexture == null) clearTexture = new Texture2D(2, 2, TextureFormat.BGRA32, false);
            clearTexture.SetPixel(0, 0, new Color(0, 0, 0, 0));

            cameraList = cameraList.Where(x => x != null).ToList();
        }


        public Vector2Int Resolution()
        {
            var scale = GetResolutionScale();
            return new Vector2Int(Mathf.CeilToInt(width * scale), Mathf.CeilToInt(height * scale));
        }

        public void InitMaterial()
        {
            SafeDestroyMaterial(instantiatedFadeMaterial);
            if (fadeMaterial == null)
            {
                shader = Resources.Load<Shader>("CameraSwitcherResources/Shader/CameraSwitcherFader");
                fadeMaterial = new Material(shader);
            }
            else
            {
                instantiatedFadeMaterial = new Material(fadeMaterial);
            }

            _materialsToBeDestroyed.Add(instantiatedFadeMaterial);

            instantiatedFadeMaterial.SetTexture("_TextureA", renderTexture1);
            instantiatedFadeMaterial.SetTexture("_TextureB", renderTexture2);
        }

        public void InitRenderTextures()
        {
            RemoveCameraTargetTexture();
            SafeDestroyRenderTexture(renderTexture1);
            SafeDestroyRenderTexture(renderTexture2);

            var res = Resolution();
            renderTexture1 = CreateRenderTextureFromSettings();
            _renderTexturesToBeDestroyed.Add(renderTexture1);

            renderTexture2 = CreateRenderTextureFromSettings();
            _renderTexturesToBeDestroyed.Add(renderTexture2);

            if (instantiatedFadeMaterial != null)
            {
                instantiatedFadeMaterial.SetTexture("_TextureA", renderTexture1);
                instantiatedFadeMaterial.SetTexture("_TextureB", renderTexture2);
            }
        }

        private RenderTexture CreateRenderTextureFromSettings()
        {
            var res = Resolution();
            var renderTexture = new RenderTexture(res.x, res.y, (int)depthStencilFormat, format);
            renderTexture.antiAliasing = (int)antiAliasing;
            return renderTexture;
        }

        private void RemoveCameraTargetTexture()
        {
            foreach (var liveCamera in cameraList)
            {
                if (liveCamera) liveCamera.TargetCamera.targetTexture = null;
            }
        }


        private void OnDestroy()
        {
            RemoveCameraTargetTexture();
            SafeDestroyMaterial(instantiatedFadeMaterial);
            SafeDestroyRenderTexture(renderTexture1);
            SafeDestroyRenderTexture(renderTexture2);

            if (clearTexture) DestroyImmediate(clearTexture);
        }

        private void SafeDestroyRenderTexture(RenderTexture renderTexture)
        {
            if (!renderTexture || !_renderTexturesToBeDestroyed.Contains(renderTexture)) return;
            _renderTexturesToBeDestroyed.Remove(renderTexture);
            DestroyImmediate(renderTexture);
        }

        private void SafeDestroyMaterial(Material material)
        {
            if (!material || !_materialsToBeDestroyed.Contains(material)) return;
            _materialsToBeDestroyed.Remove(material);
            DestroyImmediate(material);
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
        }

        public void BlitOutputTarget(RenderTexture dst)
        {
            if (instantiatedFadeMaterial == null) return;
            Graphics.Blit(Texture2D.blackTexture, dst, instantiatedFadeMaterial);
        }

        public void SetCameraQueue(LiveCamera camera1, LiveCamera camera2 = null, float blend = 0f,
            Material material = null)
        {
            camera1Queue = camera1;
            camera2Queue = camera2;

            fader = blend;
            this.instantiatedFadeMaterial = material ? material : instantiatedFadeMaterial;
        }

        private void UpdateCameraMixerPostEffect()
        {
            foreach (var cameraMixerPostEffectBase in cameraMixerPostEffectBases)
            {
                if (cameraMixerPostEffectBase != null) cameraMixerPostEffectBase.UpdateEffect();
            }
        }

        private void RefreshCamera()
        {
            foreach (var liveCamera in cameraList)
            {
                if (liveCamera == null) continue;

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
            if (instantiatedFadeMaterial == null)
            {
                InitMaterial();
            }


            UpdateCameraMixerPostEffect();
            RefreshCamera();

            if (outputTarget != null)
            {
                BlitOutputTarget(outputTarget);
            }

            if (outputImage)
            {
                outputImage.material = instantiatedFadeMaterial;
            }

            instantiatedFadeMaterial.SetTexture("_TextureDebugOverlay",
                debugOverlayTexture == null ? clearTexture : debugOverlayTexture);


            ApplyCameraQueue();
            instantiatedFadeMaterial.SetFloat("_CrossFade", fader);
        }

        public void Update()
        {
            foreach (var camera in cameraList)
            {
                if (camera && camera.cameraMixer != this) camera.cameraMixer = this;
            }

            if (cameraRenderTiming == CameraRenderTiming.Update)
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