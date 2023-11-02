using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

    [Serializable]
    public class FadeMaterialSetting
    {
        public string name = "";
        public Material material = null;
        public Material instantiatedMaterial = null;

        public void Initialize(RenderTexture A, RenderTexture B)
        {
            if (material == null) return;
            instantiatedMaterial = new Material(material);
            instantiatedMaterial.SetTexture("_TextureA", A);
            instantiatedMaterial.SetTexture("_TextureB", B);
        }

        public void SetFade(float fade)
        {
            if (instantiatedMaterial == null) return;
            instantiatedMaterial.SetFloat("_CrossFade", fade);
        }
    }

    [ExecuteAlways]
    public class CameraMixer : MonoBehaviour
    {
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
        public Color multiplyColorA = new Color(0, 0, 0, 0);
        public Color multiplyColorB = new Color(0, 0, 0, 0);
        public int cameraColorBlendModeA = 0;
        public int cameraColorBlendModeB = 0;
        private Texture2D clearTexture;
        [Range(0, 1)] public float fader = 0f;

        public List<FadeMaterialSetting> fadeMaterialSettings = new List<FadeMaterialSetting>();
        public FadeMaterialSetting currentFadeMaterialSetting = null;
        public AntiAliasing antiAliasing = AntiAliasing.x2;
        public RenderTexture outputTarget;
        public RawImage outputImage;

        [SerializeReference]
        public List<CameraMixerPostEffectBase> cameraMixerPostEffectBases = new List<CameraMixerPostEffectBase>();

        public List<LiveCamera> cameraList = new List<LiveCamera>();
        public CameraRenderTiming cameraRenderTiming = CameraRenderTiming.Update;

        private readonly List<RenderTexture> _renderTexturesToBeDestroyed = new List<RenderTexture>();

        private void Start()
        {
            Initialize();
        }

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            _renderTexturesToBeDestroyed.RemoveAll(x => x == null);

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
            // Remove and Destroy instantiate material if Material null

            foreach (var fadeMaterialSetting in fadeMaterialSettings)
            {
                if (fadeMaterialSetting.material == null)
                {
                    if (fadeMaterialSettings.IndexOf(fadeMaterialSetting) == 0)
                    {
                        fadeMaterialSetting.material = CameraMixerUtility.DefaultMaterial;
                    }

                    fadeMaterialSetting.name = "Default";
                }
                else
                {
                    continue;
                }

                if (fadeMaterialSetting.instantiatedMaterial.shader != fadeMaterialSetting.material.shader)
                {
                    DestroyImmediate(fadeMaterialSetting.instantiatedMaterial);
                    fadeMaterialSetting.instantiatedMaterial = new Material(fadeMaterialSetting.material);
                }
            }

            fadeMaterialSettings.RemoveAll(x => x.material == null);


            currentFadeMaterialSetting = fadeMaterialSettings[0];

            foreach (var fadeMaterialSetting in fadeMaterialSettings)
            {
                fadeMaterialSetting.Initialize(renderTexture1, renderTexture2);
            }
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

            foreach (var fadeMaterialSetting in fadeMaterialSettings)
            {
                fadeMaterialSetting.Initialize(renderTexture1, renderTexture2);
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
                if (liveCamera != null && liveCamera.TargetCamera != null) liveCamera.TargetCamera.targetTexture = null;
            }
        }


        private void OnDestroy()
        {
            RemoveCameraTargetTexture();
            foreach (var fadeMaterialSetting in fadeMaterialSettings)
            {
                DestroyImmediate(fadeMaterialSetting.instantiatedMaterial);
            }

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

        private void ApplyCameraQueue()
        {
            cam1 = camera1Queue;
            cam2 = camera2Queue;
            if (cam1 != null)
            {
                cam1.enabled = true;
                cam1.TargetCamera.targetTexture = renderTexture1;

                currentFadeMaterialSetting.instantiatedMaterial.SetColor("_MultiplyColorA", multiplyColorA);
                currentFadeMaterialSetting.instantiatedMaterial.SetInt("_BlendModeA", cameraColorBlendModeA);
            }

            if (cam2 != null)
            {
                cam2.enabled = true;
                cam2.TargetCamera.targetTexture = renderTexture2;
                currentFadeMaterialSetting.instantiatedMaterial.SetColor("_MultiplyColorB", multiplyColorB);
                currentFadeMaterialSetting.instantiatedMaterial.SetInt("_BlendModeB", cameraColorBlendModeB);
            }
        }

        public void BlitOutputTarget(RenderTexture dst)
        {
            if (currentFadeMaterialSetting == null) return;
            if (currentFadeMaterialSetting.instantiatedMaterial == null)
            {
                InitMaterial();
            }

            Graphics.Blit(Texture2D.blackTexture, dst, currentFadeMaterialSetting.instantiatedMaterial);
        }

        public void SetCameraQueue(LiveCamera camera1, LiveCamera camera2 = null, float blend = 0f,
            int fadeSettingIndex = 0)
        {
            camera1Queue = camera1;
            camera2Queue = camera2;

            fader = blend;
            currentFadeMaterialSetting = fadeSettingIndex < fadeMaterialSettings.Count
                ? fadeMaterialSettings[fadeSettingIndex]
                : fadeMaterialSettings[0];
        }

        public void SetMultiplyColor(Color multiplyColorA, CameraColorBlendMode blendModeA, Color multiplyColorB,
            CameraColorBlendMode blendModeB)
        {
            this.multiplyColorA = multiplyColorA;
            this.multiplyColorB = multiplyColorB;
            cameraColorBlendModeA = (int)blendModeA;
            cameraColorBlendModeB = (int)blendModeB;
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

        private List<string> fadeMaterialNameCash = new List<string>();

        private void RefreshFadeMaterialSettings()
        {
            fadeMaterialNameCash.Clear();
            var sameNameCount = 0;
            foreach (var fadeMaterialSetting in fadeMaterialSettings)
            {
                if (fadeMaterialNameCash.Contains(fadeMaterialSetting.name))
                {
                    fadeMaterialSetting.name = "New Material";
                    if (sameNameCount > 0) fadeMaterialSetting.name += $" ({sameNameCount})";
                    sameNameCount++;
                }

                if (fadeMaterialSetting.material == null)
                {
                    if (fadeMaterialSettings.IndexOf(fadeMaterialSetting) == 0 && fadeMaterialSetting.name == "Default")
                    {
                        fadeMaterialSetting.material = CameraMixerUtility.DefaultMaterial;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (fadeMaterialSetting.instantiatedMaterial == null || fadeMaterialSetting.material.shader !=
                    fadeMaterialSetting.instantiatedMaterial.shader)
                {
                    if (fadeMaterialSetting.instantiatedMaterial != null)
                        DestroyImmediate(fadeMaterialSetting.instantiatedMaterial);
                    if (fadeMaterialSetting.material == null)
                    {
                        if (fadeMaterialSettings.IndexOf(fadeMaterialSetting) == 0 &&
                            fadeMaterialSetting.name == "Default")
                            fadeMaterialSetting.material = CameraMixerUtility.DefaultMaterial;
                        else
                            continue;
                    }

                    fadeMaterialSetting.instantiatedMaterial = new Material(fadeMaterialSetting.material);
                }

                fadeMaterialNameCash.Add(fadeMaterialSetting.name);
            }
        }

        public void Render()
        {
            if (currentFadeMaterialSetting == null)
            {
                currentFadeMaterialSetting = fadeMaterialSettings[0];
            }


            UpdateCameraMixerPostEffect();
            RefreshCamera();

            if (outputTarget != null)
            {
                BlitOutputTarget(outputTarget);
            }

            if (outputImage)
            {
                outputImage.material = fadeMaterialSettings[0].instantiatedMaterial;
            }

            currentFadeMaterialSetting.instantiatedMaterial.SetTexture("_TextureDebugOverlay",
                debugOverlayTexture != null ? debugOverlayTexture : clearTexture);


            ApplyCameraQueue();
            currentFadeMaterialSetting.SetFade(fader);
        }

        public void Update()
        {
            foreach (var camera in cameraList)
            {
                if (camera && camera.cameraMixer != this) camera.cameraMixer = this;
            }

            RefreshFadeMaterialSettings();
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