using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    private Transform _camera;
    private Vector3 _previousCameraPosition;

    [SerializeField] private float baseSpeed = 0.2f; // Base speed for parallax layers
    [SerializeField] private float speedMultiplier = 0.2f; // Increases with layer index

    private void Start()
    {
        _camera = Camera.main.transform;
        _previousCameraPosition = _camera.position;
    }

    private void LateUpdate()
    {
        if (!Globals.Instance.inLevelEditor)
        {
            Vector3 deltaMovement = _camera.position - _previousCameraPosition;

            foreach (Transform layer in transform)
            {
                int layerIndex;
                if (int.TryParse(layer.name, out layerIndex)) // Extract index from GameObject name
                {
                    if (layerIndex == 1) continue; // Skip the main player ground (layer 1)

                    float parallaxSpeed = baseSpeed + (layerIndex * speedMultiplier);
                    layer.position += new Vector3(deltaMovement.x * parallaxSpeed, 0, 0);
                }
            }

            _previousCameraPosition = _camera.position;
        }
        
    }
}
