using System.Linq;
using UnityEngine;

namespace CameraLiveProduction
{
    public static class GameObjectExtensions
    {
        public static T[] GetComponentsInChildrenWithoutSelf<T>(this GameObject self) where T : Component
        {
            return self.GetComponentsInChildren<T>().Where(c => self != c.gameObject).ToArray();
        }
    }
}