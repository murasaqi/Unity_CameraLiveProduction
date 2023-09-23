using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;



namespace CameraLiveProduction
{
    [ExecuteAlways]
    public class CinemachineVolumeForceLayerChange : MonoBehaviour
    {
        // Start is called before the first frame update

        public List<Volume> volumes = new List<Volume>();
        // public CameraMixer cameraMixer;

        void Start()
        {

        }

        public void Init()
        {
            volumes.Clear();
        }
        

        private void OnEnable()
        {
            Init();
        }

        public void OnDestroy()
        {
            Init();
        }

        
        public void SetEnable(bool enable)
        {
            if (volumes == null || volumes.Count == 0) return;
            foreach (var volume in volumes)
            {
                volume.enabled = enable;
            }
        }


        // Update is called once per frame
        void LateUpdate()
        {
            if (volumes.Count==0)
            {
                if (transform.childCount > 0)
                {
                    var _volumes = transform.gameObject.GetComponentsInChildrenWithoutSelf<Volume>();
                    if (_volumes.Length > 0)
                    {
                        foreach(var volume in _volumes)
                        {
                            // Debug.Log($"{volume.name}, {volume.hideFlags}");
                            if(volume.hideFlags == HideFlags.HideAndDontSave)volumes.Add(volume);
                        }
                    }
                }
            }

            if (volumes == null || volumes.Count == 0) return;
            // Debug.Log(transform.childCount);
            foreach (var volume in volumes)
            {
                volume.gameObject.layer = gameObject.layer;   
            }
            
            


        }
    }
    
}