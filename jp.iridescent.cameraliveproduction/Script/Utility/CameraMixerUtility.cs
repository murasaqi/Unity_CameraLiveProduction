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

        static CameraMixerUtility()
        {
            DefaultMaterial = GetDefaultShader() != null ? new Material(GetDefaultShader()) : null;
            ClearColor = new Color(0, 0, 0, 0);
        }

        public static RenderTexture GetAlphaZeroRenderTexture()
        {
            var rTex = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);

            Texture2D texBuf = new Texture2D(1, 1);
            texBuf.SetPixel(0, 0, new Color(0, 0, 0, 0));
            texBuf.Apply();
            Graphics.Blit(texBuf, rTex);

            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBAFloat, false);
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            Debug.Log(tex.GetPixel(0, 0).ToString("f6"));
            return rTex;
        }
    }
}