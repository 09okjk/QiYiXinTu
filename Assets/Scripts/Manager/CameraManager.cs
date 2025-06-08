using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Manager
{
    public class CameraManager : MonoBehaviour
    {
        public CinemachineCamera cinemachineCamera;
        public static CameraManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetCameraActive(false);
        }

        public void SetFollowTarget(Transform target)
        {
            if (cinemachineCamera != null)
            {
                cinemachineCamera.Target.TrackingTarget = target;
            }
        }
        
        public void SetCameraActive(bool isActive)
        {
            if (cinemachineCamera != null)
            {
                cinemachineCamera.gameObject.SetActive(isActive);
            }
        }
    }
}