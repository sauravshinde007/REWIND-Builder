using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Reflection;

// NOTE: Extend any class that should be a mechanic from this
public interface IMechanic
{
    public bool Connect(GameObject other);
    void ChangeValues(IMechanic newMechanic);
}

public class Mechanic : MonoBehaviour
{
    [SerializeField] public new string name;
    [SerializeField] public MechanicObject basicConfig;

    public GameObject actualMechanicGO;
    [SerializeField] private GameObject previewPrefab;

    private Vector2 previewOffset = Vector2.zero;

    void Start()
    {
        previewOffset = actualMechanicGO.transform.localPosition;
    }

    public Mechanic ReplaceMechanic(IMechanic newMechanic, Vector3Int position)
    {
        var clone = Instantiate(this, position, Quaternion.identity);
        clone.actualMechanicGO.GetComponent<IMechanic>().ChangeValues(newMechanic);
        return clone;
    }

    public void ShowAttributesInUI()
    {

        var contentUI = UI_Elements_Manager.instance.contentUIParent;

        // Clear All Attributes
        foreach (Transform t in contentUI) Destroy(t.gameObject);

        // Find all attributes from mechanic
        var mechanic = actualMechanicGO.GetComponent<IMechanic>();
        if (mechanic == null)
        {
            Debug.Log("No Attributes found");
            return;
        }
        var type = mechanic.GetType();

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var attrib = field.GetCustomAttribute<MechanicAttribute>();
            if (attrib == null) continue;

            if (field.FieldType == typeof(float))
            {
                CreateSlider(mechanic, field, attrib, contentUI);
            }
            else if (field.FieldType == typeof(int))
            {
                if(!attrib.Dropdown) CreateSlider(mechanic, field, attrib, contentUI, isInt:true);
                else CreateDropdown(mechanic, field, attrib, contentUI);
            }
            else if (field.FieldType == typeof(bool))
            {
                CreateToggle(mechanic, field, attrib, contentUI);
            }
        }
    }

    private static void CreateDropdown(IMechanic mechanic, FieldInfo field, MechanicAttribute attrib, Transform parent)
    {
        var dropdownObject = Instantiate(UI_Elements_Manager.instance.dropdownPrefab, parent);
        var dropdown = dropdownObject.dropdown;
        var labelText = dropdownObject.labelText;
        var colorImage = dropdownObject.colorImage;

        var mappings = ColorManager.instance.mappings;
        dropdown.ClearOptions();
        dropdown.AddOptions(mappings.Select(obj=>obj.colorName).ToList());

        labelText.text = attrib.Tag;
        var currentVal = (int)GetFieldOrPropertyValue(mechanic, field);
        colorImage.color = mappings[currentVal].color;
        dropdown.value = currentVal;
        
        // Listen for changes
        dropdown.onValueChanged.AddListener((int index) =>
        {
            SetFieldOrPropertyValue(mechanic, field, index);
            dropdown.value = index;
            colorImage.color = mappings[index].color;
            dropdown.RefreshShownValue();
        });
    }

    // Create UI
    private static void CreateSlider(IMechanic mechanic, MemberInfo member, MechanicAttribute sliderAttr, Transform uiParent, bool isInt=false)
    {
        // Instantiate slider prefab
        var sliderObj = Instantiate(UI_Elements_Manager.instance.valueSliderPrefab, uiParent);
        var slider = sliderObj.slider;

        // Set slider min, max and current value based on the field
        slider.wholeNumbers = isInt;
        slider.minValue = sliderAttr.Min;
        slider.maxValue = sliderAttr.Max;
        var currentValue = GetFieldOrPropertyValue(mechanic, member);
        var value = isInt ? (int)currentValue : (float)currentValue;
        slider.value = value;

        var format = isInt ? "##" : "##.##";
        sliderObj.valueText.text = value.ToString(format);
        sliderObj.label.text = sliderAttr.Tag;

        // Add listener to update the field's value when slider changes
        slider.onValueChanged.AddListener((newValue) =>
        {
            var newVal = isInt ? Mathf.RoundToInt(newValue) : (float)newValue;
            switch (member)
            {
                case FieldInfo field:
                    if(field.FieldType == typeof(float)) field.SetValue(mechanic, (float)newVal);
                    if(field.FieldType == typeof(int)) field.SetValue(mechanic, (int)newVal);
                    break;
                case PropertyInfo property:
                    if(property.PropertyType == typeof(float)) property.SetValue(mechanic, (float)newVal);
                    if(property.PropertyType == typeof(int)) property.SetValue(mechanic, (int)newVal);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported member type");
            }
            sliderObj.valueText.text = newValue.ToString(format);
        });

    }

    private static void CreateToggle(IMechanic mechanic, MemberInfo member, MechanicAttribute toggleAttr, Transform uiParent)
    {
        var toggleObj = Instantiate(UI_Elements_Manager.instance.toggleButtonPrefab, uiParent);
        var toggle = toggleObj.toggle;

        var currentValue = (bool)GetFieldOrPropertyValue(mechanic, member);
        toggle.isOn = currentValue;
        toggleObj.label.text = toggleAttr.Tag;

        toggle.onValueChanged.AddListener((newValue) =>
        {
            SetFieldOrPropertyValue(mechanic, member, newValue);
            toggle.isOn = newValue;
        });
    }

    // Preview Settings --------------------------------------------------
    public void EnablePreview()
    {
        actualMechanicGO.SetActive(false);
        previewPrefab.SetActive(true);
    }
    public void DisablePreview()
    {
        actualMechanicGO.SetActive(true);
        previewPrefab.SetActive(false);
    }

    public void SetPreviewPosition(Vector2Int gridPos)
    {
        previewPrefab.transform.position = Vector2.Lerp(previewPrefab.transform.position, gridPos + previewOffset, 0.3f);
    }


    // Helpers ---------------------------------------------------------------------------------------------------------------------------------
    // Helper to get the value of either a field or property
    private static object GetFieldOrPropertyValue(object obj, MemberInfo member)
    {
        return member switch
        {
            FieldInfo field => field.GetValue(obj),
            PropertyInfo property => property.GetValue(obj),
            _ => throw new InvalidOperationException("Unsupported member type")
        };
    }

    // Helper to set the value of either a field or property
    private static void SetFieldOrPropertyValue(object obj, MemberInfo member, object value)
    {
        switch (member)
        {
            case FieldInfo field:
                field.SetValue(obj, value);
                break;
            case PropertyInfo property:
                property.SetValue(obj, value);
                break;
            default:
                throw new InvalidOperationException("Unsupported member type");
        }
    }

    // Helper to get the type of either a field or property
    private Type GetFieldOrPropertyType(MemberInfo member)
    {
        return member switch
        {
            FieldInfo field => field.FieldType,
            PropertyInfo property => property.PropertyType,
            _ => throw new InvalidOperationException("Unsupported member type")
        };
    }
}
