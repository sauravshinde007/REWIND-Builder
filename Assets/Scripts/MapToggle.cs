using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapToggle : MonoBehaviour
{
    // Array of objects to toggle
    private GameObject[] objectsToToggle;

    // InputAction for toggling the map
    [SerializeField] private InputActionReference mapInteract;

    void Start()
    {
        // Automatically populate the array with all child objects
        objectsToToggle = GetChildGameObjects();

        // Ensure all objects start inactive
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // Subscribe to the input action
        if (mapInteract != null && mapInteract.action != null)
        {
            mapInteract.action.performed += _ => ToggleMap();
            mapInteract.action.Enable();
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from the input action
        if (mapInteract != null && mapInteract.action != null)
        {
            mapInteract.action.performed -= _ => ToggleMap();
            mapInteract.action.Disable();
        }
    }

    private void ToggleMap()
    {
        // Toggle the active state of all child objects
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
                obj.SetActive(!obj.activeSelf);
        }
    }

    // Helper method to get all child objects
    private GameObject[] GetChildGameObjects()
    {
        List<GameObject> childObjects = new List<GameObject>();
        foreach (Transform child in transform)
        {
            childObjects.Add(child.gameObject);
        }

        return childObjects.ToArray();
    }
}
