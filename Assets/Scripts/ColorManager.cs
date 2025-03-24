    
using UnityEngine;

[System.Serializable]
public class ColorMapping
{
    public string colorName;
    public Color color;
}

public class ColorManager: MonoBehaviour {

    public ColorMapping[] mappings;
    
    public static ColorManager instance;

    public int GetColorIndex(string colorName)
    {
        for(var i = 0; i < mappings.Length; i++)
        {
            if (mappings[i].colorName == colorName) return i;
        }

        return -1;
    }

    void Awake()
    {
        // If an instance of this singleton already exists and it's not this one, destroy this object
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Ensures there's only one instance
            return;
        }

        // Assign this instance to the static variable
        instance = this;
    }
}