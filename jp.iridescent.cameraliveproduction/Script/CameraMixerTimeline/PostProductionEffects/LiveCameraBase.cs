using System;
using System.Collections.Generic;

#if USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

#if USE_CINEMACHINE
using Cinemachine;
#endif

using UnityEngine;

namespace CameraLiveProduction
{
    [Serializable]
    public abstract class LiveCameraBase:MonoBehaviour
    {
       
        public virtual void Initialize(){}

        public virtual RenderTexture TargetTexture
        {
            get;
            set;
        }

        public virtual void Render(Texture outputTexture)
        {
        }

        public CameraMixer cameraMixer;
        public virtual void SetEnableTargetCamera(bool enable) { }
        
        public virtual LayerMask GetLayerMask()
        {
            return -1;
        }
        
        public virtual void TryInitialize()
        {
        }
        


    }
}