using System;
using System.Collections;
using System.Collections.Generic;
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
        public CameraMixer cameraMixer;

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
                volume = transform.GetComponentInChildren<Volume>();
            }

            if (volume == null || cameraMixer == null) return;
            // Debug.Log(transform.childCount);
            volume.gameObject.layer = gameObject.layer;
            
            


        }
    }
}