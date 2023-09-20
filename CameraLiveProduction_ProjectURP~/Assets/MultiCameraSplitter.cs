using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class MultiCameraSplitter : MonoBehaviour
{
    
    [SerializeField] private List<Camera> cameras = new List<Camera>();
    [SerializeField] private RectTransform cameraContainer;
    [SerializeField] private List<RawImage> rawImages = new List<RawImage>();
    [SerializeField] private RenderTextureFormat renderTextureFormat = RenderTextureFormat.DefaultHDR;
    [SerializeField] private int depthBuffer = 24;
    [SerializeField] private int antiAliasing = 1;
    [SerializeField] private int width = 1920;
    [SerializeField] private int height = 1080;
    private List<RenderTexture> renderTextures = new List<RenderTexture>();
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void UpdateStyle()
    {
        if(cameraContainer == null) return;
        cameraContainer.rect.Set(0,0,0,0);
        cameraContainer.anchoredPosition = new Vector2(0f,0f);
        
        var count = 0;
        foreach (var rawImage in rawImages)
        {
            
            rawImage.rectTransform.anchorMax = new Vector2(1f,1f);
            rawImage.rectTransform.anchorMin = new Vector2(0f,0f);
            rawImage.rectTransform.sizeDelta = new Vector2(0f,0f);
            rawImage.rectTransform.anchoredPosition = new Vector2(0f,0f);
            rawImage.rectTransform.localScale = new Vector3(0.5f,0.5f,1f);

            var mum = count % 4;
            if (mum == 0)
            {
                rawImage.rectTransform.pivot = new Vector2(0f,1f);
            }
            
            if (mum == 1)
            {
                rawImage.rectTransform.pivot = new Vector2(1f,1f);
            }
            
            if (mum == 2)
            {
                rawImage.rectTransform.pivot = new Vector2(0f,0f);
            }
            
            if (mum == 3)
            {
                rawImage.rectTransform.pivot = new Vector2(1f,0f);
            }
            count++;
        }
    }
    
    private void ReGenerateRenderTexture()
    {
        renderTextures.Clear();
        var diff = cameras.Count - renderTextures.Count;
        if( diff == 0) return;
        if (diff > 0)
        {
            for (int i = 0; i < diff; i++)
            {
                var renderTexture = new RenderTexture(width, height, depthBuffer, renderTextureFormat);
                renderTexture.antiAliasing = antiAliasing;
                renderTextures.Add(renderTexture);
            }
        }
        else
        {
            for (int i = 0; i < -diff; i++)
            {
                var renderTexture = renderTextures[renderTextures.Count - 1];
                renderTextures.Remove(renderTexture);
                renderTexture.Release();
                DestroyImmediate(renderTexture);
            }
        }
        
        
            
    }
    
    private void ApplyPreview()
    {
        var count = 0;
        for (int i = 0; i < cameras.Count; i++)
        {
            var camera = cameras[i];
            var renderTexture = renderTextures[i];
            camera.targetTexture = renderTexture;
            if (rawImages.Count > i)
            {
                var rawImage = rawImages[i];
                rawImage.texture = renderTexture;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        ReGenerateRenderTexture();
        UpdateStyle();
        ApplyPreview();
    }
}
