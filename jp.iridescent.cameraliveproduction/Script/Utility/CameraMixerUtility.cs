using UnityEngine;

namespace CameraLiveProduction
{
    public static class CameraMixerUtility
    {
        public static Shader GetDefaultShader()
        {
            return Resources.Load<Shader>("CameraSwitcherResources/Shader/CameraSwitcherFader");
        }


        public static Color ClearColor;
        public static Material DefaultMaterial;
        public static Texture2D AlphaZeroTexture2D;
        public static RenderTexture AlphaZeroRenderTexture;

        static CameraMixerUtility()
        {
            DefaultMaterial = GetDefaultShader() != null ? new Material(GetDefaultShader()) : null;
            ClearColor = new Color(0, 0, 0, 0);
            AlphaZeroRenderTexture =
                new RenderTexture(1, 1, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);

            AlphaZeroTexture2D = new Texture2D(1, 1);
            AlphaZeroTexture2D.SetPixel(0, 0, new Color(0, 0, 0, 0));
            AlphaZeroTexture2D.Apply();
            Graphics.Blit(AlphaZeroTexture2D, AlphaZeroRenderTexture);

            Texture2D tex = new Texture2D(AlphaZeroRenderTexture.width, AlphaZeroRenderTexture.height,
                TextureFormat.RGBAFloat, false);
            RenderTexture.active = AlphaZeroRenderTexture;
            tex.ReadPixels(new Rect(0, 0, AlphaZeroRenderTexture.width, AlphaZeroRenderTexture.height), 0, 0);
            tex.Apply();
        }
    }
}