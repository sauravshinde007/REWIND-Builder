using System.Collections.Generic;
using UnityEngine;

public class GlobalInventory : MonoBehaviour
{
    public static GlobalInventory Instance;

    private void Awake()
    {
        // If an instance of this singleton already exists and it's not this one, destroy this object
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensures there's only one instance
            return;
        }

        // Assign this instance to the static variable
        Instance = this;
    }
    
    public List<int> keyColors = new List<int>();

    public void AddKey(int colorIndex)
    {
        keyColors.Add(colorIndex);
    }

    public bool CanOpenDoor(int index)
    {
        if (keyColors.Contains(index)){
            keyColors.Remove(index);
            return true;
        }

        return false;
    }
}
