using UnityEngine;

namespace CameraLiveProduction
{
    public abstract class CameraMixerPostEffectBase: MonoBehaviour
    {

        public CameraMixer cameraMixer;
        public virtual void Init(CameraMixer cameraMixer)
        {
            this.cameraMixer = cameraMixer;
        }
        
        
        public virtual void UpdateEffect()
        {
            
        }
    }
    
}