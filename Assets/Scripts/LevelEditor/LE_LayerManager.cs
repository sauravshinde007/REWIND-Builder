using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class LayerInfo
{
    public string name;
    public bool collidable;
}

public class LE_LayerManager : MonoBehaviour
{
    public List<LayerInfo> allLayers = new();
    [SerializeField] private TMP_Dropdown layerSelectDropdown;
    [SerializeField] private int layerLimit = 10;
    
    [SerializeField] private Toggle collidableToggle;

    private int _selectedLayer;
    
    private int _layersCount;
    
    public static LE_LayerManager Instance;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }
    // Start is called before the first frame update
    private void Start() {
        _layersCount = 0;
        
        layerSelectDropdown.onValueChanged.AddListener(OnLayerSelectChanged);
        collidableToggle.onValueChanged.AddListener(OnCollidableToggleChanged);
        
        layerSelectDropdown.ClearOptions();
        // Setup Default Layers
        foreach (var l in allLayers)
        {
            AddNewLayer(l.name);
        }
        layerSelectDropdown.RefreshShownValue();

        SetSelectedButton(0);
    }

    private void OnCollidableToggleChanged(bool isOn)
    { 
        allLayers[_selectedLayer].collidable = isOn;
        LE_TileManager.Instance.SetLayerCollisionState(_selectedLayer, isOn);
    }

    private void OnLayerSelectChanged(int value)
    {
        SetSelectedButton(value);
    }

    public void ClearLayers()
    {
        _layersCount = 0;
        _selectedLayer = 0;
        layerSelectDropdown.ClearOptions();
        allLayers.Clear();
        LE_TileManager.Instance.tileLayers.Clear();
    }

    public void SetSelectedButton(int layerIndex)
    {
        _selectedLayer = layerIndex;
        LE_TileManager.Instance.SetCurrentLayer(_selectedLayer, allLayers[layerIndex].collidable);
        layerSelectDropdown.value = layerIndex;
        collidableToggle.isOn = allLayers[layerIndex].collidable;
    }

    public void AddNewLayer(string layerName)
    {
        if(layerSelectDropdown.options.Count >= layerLimit) return;
        
        layerSelectDropdown.options.Add(new TMP_Dropdown.OptionData() { text = layerName });
        _layersCount++;
        LE_TileManager.Instance.AddNewLayer(_layersCount-1);
    }

    public void AddLayer(string layerName="New Layer", bool collidable = false)
    {
        if(layerSelectDropdown.options.Count >= layerLimit) return;
        
        var newLayer = new LayerInfo();
        if (layerName == "") layerName = "Layer" + (_layersCount+1);
        newLayer.name = layerName;
        newLayer.collidable = collidable;
        
        layerSelectDropdown.options.Add(new TMP_Dropdown.OptionData() { text = layerName });
        allLayers.Add(newLayer);
        
        LE_TileManager.Instance.AddNewLayer(_layersCount);
        SetSelectedButton(_layersCount);
        
        _layersCount++;
    }

    public void UIAddLayer()
    {
        AddLayer("", true);
    }
}