using UnityEngine;

namespace CameraLiveProduction
{
    public static class CameraMixerUtility
    {
        public static Shader GetDefaultShader()
        {
            return Resources.Load<Shader>("CameraSwitcherResources/Shader/CameraSwitcherFader");
        }

        public static Material DefaultMaterial;

        static CameraMixerUtility()
        {
            DefaultMaterial = GetDefaultShader() != null ? new Material(GetDefaultShader()) : null;
        }
    }
}