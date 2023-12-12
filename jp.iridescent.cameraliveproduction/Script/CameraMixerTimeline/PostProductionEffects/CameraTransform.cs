// using UnityEngine;
//
// namespace CameraLiveProduction
// {
//     public class CameraTransform : CameraPostProductionBase
//     {
//         public Vector3 position;
//         public Vector3 rotation;
//         
//         public Vector3 originalPosition;
//         public Vector3 originalRotation;
//
//         public override void Initialize(LiveCamera liveCamera)
//         {
//             position = liveCamera.transform.localPosition;
//             rotation = liveCamera.transform.localEulerAngles;
//             originalPosition = position;
//             originalRotation = rotation;
//         }
//         
//         public override void OnDestroy(LiveCamera liveCamera)
//         {
//             
//         }
//         
//         public override void OnClipDisable(LiveCamera liveCamera)
//         {
//             liveCamera.TargetCamera.transform.localPosition = originalPosition;
//             liveCamera.TargetCamera.transform.localEulerAngles = originalRotation;
//         }
//             
//         public override void UpdateEffect(LiveCamera liveCamera, float time,float weight = 1f)
//         {
//             if(liveCamera ==null || liveCamera.TargetCamera == null)
//                 return;
//             liveCamera.transform.localPosition = Vector3.zero;
//             liveCamera.transform.localPosition += Vector3.Lerp(liveCamera.TargetCamera.transform.localPosition,position,weight);
//             
//             liveCamera.transform.localEulerAngles = Vector3.zero;
//             liveCamera.transform.localEulerAngles += Vector3.Lerp(liveCamera.TargetCamera.transform.localEulerAngles,rotation,weight);
//         }
//     }
// }