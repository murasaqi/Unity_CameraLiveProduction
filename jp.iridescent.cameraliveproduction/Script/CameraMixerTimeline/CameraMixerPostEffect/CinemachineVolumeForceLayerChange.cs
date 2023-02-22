using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;



namespace CameraLiveProduction
{
    [ExecuteAlways]
    public class CinemachineVolumeForceLayerChange : MonoBehaviour
    {
        // Start is called before the first frame update

        public Volume volume;
        // public CameraMixer cameraMixer;

        void Start()
        {

        }

        public void OnDestroy()
        {
            // DestroyImmediate(volume);
            // RemovedComponent(volume);
        }



        // Update is called once per frame
        void LateUpdate()
        {
            if (volume == null)
            {
                if (transform.childCount > 0)
                {
                    var volumes = transform.gameObject.GetComponentsInChildrenWithoutSelf<Volume>();
                    if (volumes.Length > 0) volume = volumes.First();
                }
            }

            if (volume == null) return;
            // Debug.Log(transform.childCount);
            volume.gameObject.layer = gameObject.layer;
            
            


        }
    }
    
}