using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Cameras
{
    public class CameraSettings : BaseBehaviour
    {
        public List<CullDistanceOverride> CullDistanceOverrides => _cullDistanceOverrides;
        [SerializeField] private List<CullDistanceOverride> _cullDistanceOverrides;

        public Camera MainCamera => _mainCamera;
        [SerializeField] private Camera _mainCamera = null;

        public GameObject CameraObject => _cameraObject;
        [SerializeField] private GameObject _cameraObject = null;

        public GameObject CameraParent => _cameraParent;
        [SerializeField] private GameObject _cameraParent = null;

        public List<Camera> Cameras => _cameras;
        [SerializeField] private List<Camera> _cameras;
    }
}


