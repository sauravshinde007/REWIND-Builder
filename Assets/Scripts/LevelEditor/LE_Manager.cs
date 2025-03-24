using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LE_Manager : MonoBehaviour {

    public static LE_Manager Instance { get; private set; }

    private void Awake(){
        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(this);
        }
    }

    [SerializeField] private TMP_Text modeIndicator;
    [SerializeField] private TMP_Text playPauseText;
    [SerializeField] private Camera _camera;

    [Header("Managers")]
    [SerializeField] private LE_MechanicManager mechanicManager;
    [SerializeField] private LE_TileManager tileManager;
    [SerializeField] private LE_CameraMovement cameraMovement;
    [SerializeField] private GameObject infiniteGridGO;

    [Header("Save/Load")] 
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private TMP_InputField levelName;
    [SerializeField] private GameObject levelObject;

    [Header("Settings UI")]
    [SerializeField] private GameObject settingsUICanvas;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private CanvasGroup levelEditorCanvasGroup;
    [SerializeField] private CanvasGroup saveLoadCanvasGroup;

    [Header("Connection Management")] 
    [SerializeField] private LineRenderer tempRenderer;
    [SerializeField] private MechanicConnection connectionPrefab;
    [SerializeField] private Transform connectionWireParent;
    [HideInInspector] public Mechanic mechanicUnderMouse;

    private bool _isDragging = false;
    private Mechanic _mech;

    private Mechanic _selectedMechanic;

    public bool SelectMode{
        get => _selectMode;
        set{
            if(value) modeIndicator.text = "Select Mode";
            else modeIndicator.text = "Place Mode";
            _selectMode = value;
        }
    }
    private bool _selectMode = true;

    public bool EditMode
    {
        get => _editMode;
        set
        {
            if(value) modeIndicator.text = "Edit Mode";
            else modeIndicator.text = "Place Mode";
            _editMode = value;
        }
    }

    private bool _editMode = true;

    // Start is called before the first frame update
    private void Start() {
        mechanicManager.canPlace = false;       // Cannot place mechanics
        tileManager.canPlace = false;           // Cannot place tiles
        SelectMode = true;                      // Select Mode By Default
        settingsUICanvas.SetActive(false);      // Disable settings UI

        tempRenderer.enabled = false;

        // Start in edit mode
        PauseGame();
    }

    private void Update(){
        if(!SelectMode || !EditMode) return;

        var mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        tempRenderer.SetPosition(1, mousePosition);
        
        // Dragging connections
        if(Input.GetMouseButtonDown(1))
        {
            _mech = GetMechanicFromRaycast(mousePosition);
            if(_mech) _isDragging = true;
        }
        
        if (_isDragging)
        {
            tempRenderer.enabled = true;
            tempRenderer.SetPosition(0, _mech.transform.position);
        }
        
        if (Input.GetMouseButtonUp(1) && _isDragging)
        {
            _isDragging = false;
            mechanicUnderMouse = GetMechanicFromRaycast(mousePosition);
            
            if (_mech && mechanicUnderMouse != _mech && mechanicUnderMouse)
            {
                var connection = Instantiate(connectionPrefab, connectionWireParent);
                var res = connection.SetupConnection(_mech, mechanicUnderMouse);
                if(!res) DestroyImmediate(connection);
            }
            
            tempRenderer.enabled = false;
            mechanicUnderMouse = null;
        }
        
        if (EventSystem.current.IsPointerOverGameObject()) 
        {
            // Don't select objects if clicking UI
            return;
        }
        if (!Input.GetMouseButtonDown(0)) return;
        
        _mech = GetMechanicFromRaycast(mousePosition);
        if(!_mech) return;
        
        // Set Selected mechanic and enable settings
        _selectedMechanic = _mech;
        EnableSettingsUI();
        titleText.text = _mech.name;

        _selectedMechanic.ShowAttributesInUI();
    }

    private Mechanic GetMechanicFromRaycast(Vector3 mousePosition)
    {
        var hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        Mechanic mechanic = null;
        if (hit.collider) mechanic = hit.collider.GetComponentInParent<Mechanic>();
        return mechanic;
    }
    
    private void TestGame(){
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) {
            // TODO: Some UI to show player not present
            Debug.LogWarning("Player Position not set.");
            return;
        }
        
        EditMode = false;
        Time.timeScale = 1f;
        infiniteGridGO.SetActive(false);
        cameraMovement.enabled = false;

        // Disable Interaction with UI
        connectionWireParent.gameObject.SetActive(false);
        levelEditorCanvasGroup.interactable = false;
        saveLoadCanvasGroup.interactable = false;
        
        // Enable testing camera
        CameraSwitcher.Instance.EnableCinemachineControl();
        
        // Make all layers same alpha
        LE_TileManager.Instance.EnableAllLayers();

        foreach (Transform mech in mechanicManager.mechanicsParent)
        {
            var openable = mech.GetComponent<Mechanic>().actualMechanicGO.GetComponent<IOpenable>();
            if (openable != null)
            {
                openable.Setup();
            }
        }
    }

    public void PauseGame(){
        EditMode = true;
        Time.timeScale = 0f;
        // playPauseText.text = "TEST";
        infiniteGridGO.SetActive(true);
        cameraMovement.enabled = true;
        
        connectionWireParent.gameObject.SetActive(true);
        levelEditorCanvasGroup.interactable = true;
        saveLoadCanvasGroup.interactable = true;
        
        // Enable Level Editor Camera Controls
        CameraSwitcher.Instance.EnableLECameraControl();
    }

    public void ToggleGameState(){
        if (EditMode)
        {
            // levelManager.SaveMap();
            TestGame();
        }
        else
        {
            // levelManager.LoadMap();
            PauseGame();
        }
    }

    private void EnableSettingsUI(){
        settingsUICanvas.SetActive(true);
    }

    public void DisableSettingsUI(){
        settingsUICanvas.SetActive(false);
    }

    public void EnableTilePlaceMode(){
        SelectMode = false;
        mechanicManager.canPlace = false;
        mechanicManager.ClearPreviews();
        tileManager.canPlace = true;
    }
    public void EnableMechanicPlaceMode(){
        SelectMode = false;
        mechanicManager.canPlace = true;
        tileManager.canPlace = false;
    }

    public void DisablePlaceMode(){
        mechanicManager.canPlace = false;
        mechanicManager.ClearPreviews();
        tileManager.canPlace = false;
        SelectMode = true;
    }

    
    #if UNITY_EDITOR
    // TODO: This is editor only, implement another for saving in game
    public void SaveLevelAsPrefab()
    {
        if (levelName.text == "") return;
        if (!Directory.Exists("Assets/Prefabs/Levels"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Levels");
        string localPath = "Assets/Prefabs/Levels/" + levelName.text + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        Debug.Log(localPath);
        PrefabUtility.SaveAsPrefabAsset(levelObject, localPath);
        // if (prefabSuccess) Debug.Log("Prefab was saved successfully");
        // else Debug.Log("Prefab failed to save" + prefabSuccess);
    }

    // TODO: This is editor only, implement another for loading in game
    public void LoadGameFromPrefab()
    {
        if (levelName.text == "") return;
        string localPath = "Assets/Prefabs/Levels/" + levelName.text + ".prefab";
        GameObject levelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(localPath);
        if(levelPrefab != null){
            // Just destroy existing work for now
            Destroy(levelObject);
            levelObject = Instantiate(levelPrefab);
        }
        else{
            Debug.Log("Level " + levelName.text + " doesn't exist");
        }
    }

    #endif
}