using UnityEngine;

namespace CameraLiveProduction
{
    public static class CameraLayerUtility
    {
        public static bool Contains(LayerMask self, int layerId)
        {
            return ((1 << layerId) & self) != 0;
        }

        public static bool Contains(LayerMask self, string layerName)
        {
            int layerId = LayerMask.NameToLayer(layerName);
            return ((1 << layerId) & self) != 0;
        }

        public static LayerMask Add(LayerMask self, LayerMask layerId)
        {
            return self | (1 << layerId);
        }

        public static LayerMask Toggle(LayerMask self, LayerMask layerId)
        {
            return self ^ (1 << layerId);
        }

        public static LayerMask Remove(LayerMask self, LayerMask layerId)
        {
            return self & ~(1 << layerId);
        }
        
       
    }


}