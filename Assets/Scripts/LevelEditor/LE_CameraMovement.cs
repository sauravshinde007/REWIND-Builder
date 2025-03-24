using UnityEngine;
using UnityEngine.EventSystems;

public class LE_CameraMovement : MonoBehaviour {
    [SerializeField] private float starOrthoSize = 20;
    [SerializeField] private float zoomSpeed = 2;
    [SerializeField] private float panSpeed = 20f;

    [SerializeField] private Vector2 sizeLimits = new Vector2(0, 50);
    
    [SerializeField] private Camera mainCamera;

    private void CameraControls(){
        // Camera Controls
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        var currentSize = mainCamera.orthographicSize;
        switch (scroll)
        {
            case < 0:
                currentSize += zoomSpeed * Time.unscaledDeltaTime;
                break;
            case > 0:
                currentSize -= zoomSpeed * Time.unscaledDeltaTime;
                break;
        }

        currentSize = Mathf.Clamp(currentSize, sizeLimits.x, sizeLimits.y);

        mainCamera.orthographicSize = currentSize;

        var ver = Input.GetAxisRaw("Vertical");
        var hor = Input.GetAxisRaw("Horizontal");
        var movement = new Vector3(hor, ver, 0f);

        mainCamera.transform.Translate(panSpeed * Time.unscaledDeltaTime * movement);

        if (!Input.GetKeyDown(KeyCode.C)) return;
        var player = GameObject.FindGameObjectWithTag("Player");
        
        if (!player) return;
        var pos = player.transform.position;
        pos.z = mainCamera.transform.position.z;
        mainCamera.transform.position = pos;
    }
    // Start is called before the first frame update
    void Start() {
        mainCamera.orthographicSize = starOrthoSize;
    }

    // Update is called once per frame
    private void Update() {
        if(EventSystem.current.IsPointerOverGameObject()) return;
        
        // Only Allow camera to move if mouse not over gameobject
        CameraControls();
    }
}
