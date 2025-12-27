using Assets.Scripts.Cameras;
using UnityEngine;

public class CoreClientData : BaseBehaviour
{

    public WindZone Wind => _windZone;
    [SerializeField] private WindZone _windZone;
    public Light SunLight => _sunLight;
    [SerializeField] private Light _sunLight;
    public Material SkyboxMaterial => _skyboxMaterial;
    [SerializeField] private Material _skyboxMaterial;

    public CameraSettings CameraSettings => _cameraSettings;
    [SerializeField] private CameraSettings _cameraSettings;

    public bool PauseUpdates = false;
    public float LinearFogEnd = 300;
    public float SunlightIntensityMultiplier = 1.1f;
    public float AmbientIntensityMultiplier = 0.5f;

}


