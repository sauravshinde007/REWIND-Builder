using System.Collections.ObjectModel;
using Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResponsiveCamera : MonoBehaviour {


    [SerializeField] private string zoomOutKey;
    [SerializeField] private CinemachineVirtualCamera _cam;
    [SerializeField] private Camera _mainCam;
    [SerializeField] private float _buffer = 4;
    [SerializeField] private float _smoothing = 0.08f;

    [Header("Things to encapsulate")]
    [SerializeField] private Tilemap levelTiles;
    [SerializeField] private Collider2D[] extraObjects;

    private Vector3 camPos;
    private float camSize;
    private float defaultOrthoSize;

    private Transform playerTransform;

    // Start is called before the first frame update
    void Start() {
        defaultOrthoSize = _cam.m_Lens.OrthographicSize;
        camSize = _cam.m_Lens.OrthographicSize;
        camPos = _cam.transform.position;

        // All Colliders
        // Collider2D[] objectsToEncapsulate = FindObjectsOfType<Collider2D>();
        // (camPos, camSize) = CalculateOrthoSize(extraObjects, levelTiles);
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetButtonDown("ZoomOut")){
            ToggleFullView(true);
        }
        if(Input.GetButtonUp("ZoomOut")){
            ToggleFullView(false);
        }

        _cam.transform.position = Vector3.Lerp(_cam.transform.position, camPos, _smoothing);
        _cam.m_Lens.OrthographicSize = Mathf.Lerp(_cam.m_Lens.OrthographicSize, camSize, _smoothing);
    }

    void ToggleFullView(bool zoomedOut){
        if(zoomedOut){
            (camPos, camSize) = CalculateOrthoSize(extraObjects, levelTiles);
            if (_cam.Follow != null) playerTransform = _cam.Follow;
            else playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            _cam.Follow = null;
            _cam.LookAt = null;
        }
        else{
            (camPos, camSize) = (new Vector3(0, 0, -10), defaultOrthoSize);
            _cam.Follow = playerTransform;
            _cam.LookAt = playerTransform;
        }
    }

    private (Vector3 center, float size) CalculateOrthoSize(Collider2D[] objectsToEncapsulate=null, Tilemap tilemap=null){

        var bounds = new Bounds();

        if(objectsToEncapsulate!=null) 
            foreach(var col in objectsToEncapsulate) 
                bounds.Encapsulate(col.bounds);

        if(tilemap != null){
            tilemap.CompressBounds();
            bounds.Encapsulate(tilemap.localBounds);
        }

        bounds.Expand(_buffer);
        
        var vertical = bounds.size.y;
        var horizontal = bounds.size.x * _mainCam.pixelHeight / _mainCam.pixelWidth;

        var size = Mathf.Max(vertical, horizontal) * 0.5f;
        var center = bounds.center + new Vector3(0, 0, -10);

        return (center, size);
    }
}
