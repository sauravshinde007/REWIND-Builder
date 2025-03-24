using UnityEngine;

public class UI_Elements_Manager : MonoBehaviour
{
    [SerializeField] public ToggleSetting toggleButtonPrefab;
    [SerializeField] public SliderSetting valueSliderPrefab;
    [SerializeField] public DropdownSettings dropdownPrefab;

    [SerializeField] public Transform contentUIParent;

    public static UI_Elements_Manager instance;

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
