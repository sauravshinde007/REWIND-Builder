using UnityEngine;
using Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera cinemachineCamera;
    private LE_CameraMovement _leCameraController;
    
    public static CameraSwitcher Instance { get; private set; }

    private void Awake(){
        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(this);
        }
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        _leCameraController = GetComponent<LE_CameraMovement>();
        EnableLECameraControl();
    }
    
    public void EnableLECameraControl()
    {
        if (_leCameraController != null)
            _leCameraController.enabled = true;

        if (cinemachineCamera != null)
            cinemachineCamera.gameObject.SetActive(false);
    }
    
    public void EnableCinemachineControl()
    {
        if (_leCameraController != null)
            _leCameraController.enabled = false;

        if (cinemachineCamera != null)
            cinemachineCamera.gameObject.SetActive(true);
        
        // Get Player
        var player = GameObject.FindGameObjectWithTag("Player");
        cinemachineCamera.Follow = player.transform;
        cinemachineCamera.LookAt = player.transform;
    }

}
