using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

// All Sectors enum
public enum Sector
{
    The_Lab,
    Neon_Slums,
    Syndicate_Tower,
    Sewage_Network,
    The_Sprawl
}

// Manager to save and load and clear levels
public class LevelManager : MonoBehaviour
{
    [SerializeField] private Transform tileLayersParent;
    [SerializeField] private Transform mechanicsParent;
    [SerializeField] private Transform connectionsParent;
    
    [Header("Only Required in Level Editor")]
    [SerializeField] private TMP_Dropdown sectorDropdown;
    [SerializeField] private TMP_InputField levelNameInputField;
    [SerializeField] private TMP_InputField levelIndexInputField;
    
    [Header("Connection prefabs")]
    [SerializeField] private MechanicConnection connectionPrefab;

    private string defaultLevelName = "Hallway";

    private const string SavePath = "Assets/Prefabs/Levels/";
    private string _fullLevelName = "";

    void Start()
    {
        // Setup Connections
        SetupConnections();
        
        // Return if not in Level Editor
        if (!sectorDropdown || !levelNameInputField || !levelIndexInputField) return;
        sectorDropdown.ClearOptions();
        foreach (var sector in Enum.GetValues(typeof(Sector)))
        {
            sectorDropdown.options.Add(new TMP_Dropdown.OptionData() { text = sector.ToString().Replace("_", " ") });
        }

        sectorDropdown.RefreshShownValue();

        levelIndexInputField.characterValidation = TMP_InputField.CharacterValidation.Digit;
        levelNameInputField.text = defaultLevelName;
        levelIndexInputField.text = "1";
    }

    private void SetupConnections()
    {
        foreach (Transform connectionObj in connectionsParent)
        {
            var connection = connectionObj.GetComponent<MechanicConnection>();
            connection.ConnectMechanics();
        }
    }

    public void SaveMapInLevelEditor()
    {
        var sector = (Sector)sectorDropdown.value;
        var levelName = levelNameInputField.text.Replace(" ", "_");
        var levelIndex  = int.Parse(levelIndexInputField.text);

        _fullLevelName = sector + "-" + levelName + "-" + levelIndex;

        if (levelName.Length == 0) { levelName = defaultLevelName; }
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player)
        {
            Debug.LogError("Player Spawn Point not found");
            return;
        }
        
        SaveMap(sector, levelName, levelIndex, player);
        
    }

    public void SaveMap(Sector sector, string levelName, int levelIndex, GameObject player) {
        var newLevel = ScriptableObject.CreateInstance<ScriptableLevel>();
        
        // Set Player Spawn Point
        newLevel.playerSpawnPosition = player.transform.position;

        // Initialize all types of saving mechanics
        newLevel.TileLayers = new List<List<SavedTile>>();
        newLevel.LayerInfos = LE_LayerManager.Instance.allLayers;
        newLevel.mechanics = new List<MechanicInfo>();
        newLevel.connections = new List<ConnectionInfo>();

        // Setup Level Properties
        newLevel.levelName = levelName;
        newLevel.levelIndex = levelIndex;
        newLevel.sector = sector;

        var mechCtr = 0;
        // Gather all mechanics in the scene
        foreach (Transform obj in mechanicsParent)
        {
            var mechanic = obj.GetComponent<Mechanic>();
            if (mechanic.actualMechanicGO == null)
            {
                Destroy(mechanic);
                continue;
            }
            var imech = mechanic.actualMechanicGO.GetComponent<IMechanic>();
            if(!mechanic || imech == null) continue;
            
            var info = new MechanicInfo();
            info.position = Vector3Int.RoundToInt(obj.position);
            info.Mechanic = mechanic;
            info.name = mechanic.name + "_" + mechCtr++;
            mechanic.name = info.name;
            // For letting this be visible in the editor
            obj.name = info.name;
            
            newLevel.mechanics.Add(info);
        }

        var connCtr = 0;
        // Gather all connections
        foreach (Transform obj in connectionsParent)
        {
            var conn = obj.GetComponent<MechanicConnection>();
            conn.name += connCtr++;
            var info = new ConnectionInfo
            {
                name = conn.name,
                fromMechanic = conn.fromMechanic,
                toMechanic = conn.toMechanic
            };
            newLevel.connections.Add(info);
        }

        // All Tile layers
        foreach (Transform obj in tileLayersParent)
        {
            var tileLayer = obj.GetComponent<Tilemap>();
            if (tileLayer) newLevel.TileLayers.Add(GetTilesFromMap(tileLayer).ToList());
        }
        
        // Save the level
        LevelDataSaver.SaveLevel(newLevel, sector, levelName, levelIndex);
        return;

        IEnumerable<SavedTile> GetTilesFromMap(Tilemap map)
        {
            foreach (var pos in map.cellBounds.allPositionsWithin)
            {
                if (!map.HasTile(pos))
                {
                    continue;
                }
                var levelTile = map.GetTile<LevelTile>(pos);

                yield return new SavedTile()
                {
                    position = pos,
                    tile = levelTile,
                };
            }
        }
    }

    private void ClearMap()
    {
        var player = (GameObject.FindGameObjectWithTag("Player"));
        if(player != null) DestroyImmediate(player);
        foreach (Transform obj in mechanicsParent) { if(obj)Destroy(obj.gameObject); }
        foreach (Transform obj in tileLayersParent) { if(obj)Destroy(obj.gameObject); }
        LE_LayerManager.Instance.ClearLayers();
    }

    public void LoadMapInLevelEditor()
    {
        var sector = (Sector)sectorDropdown.value;
        var levelName = levelNameInputField.text.Replace(" ", "_");
        var levelIndex  = int.Parse(levelIndexInputField.text);

        if (levelName.Length == 0) { levelName = "Hallway"; }
        
        LoadMap(sector, levelName, levelIndex);
    }

    public void LoadMap(Sector sector, string levelName, int levelIndex)
    {
        
        ScriptableLevel level = null;
        level = LevelDataSaver.LoadLevel(sector, levelName, levelIndex);

        if (!level)
        {
            Debug.LogError($"Level {levelIndex} doesn't exist");
            return;
        }

        ClearMap();
        
        // Load PlayerSpawnPoint
        var playerPosition = level.playerSpawnPosition;
        LE_MechanicManager.Instance.SetPlayerSpawnPoint(playerPosition);
        
        // Load tiles
        for (var i = 0; i < level.TileLayers.Count; i++)
        {
            var layer = level.LayerInfos[i];
            LE_LayerManager.Instance.AddLayer(layer.name, layer.collidable);
        }
        
        for (var i = 0; i < level.TileLayers.Count; i++)
        {
            var layer = level.TileLayers[i];
            foreach (var savedTile in layer)
            {
                LE_TileManager.Instance.tileLayers[i].SetTile(savedTile.position, savedTile.tile);
            }
        }

        // Load Mechanics
        foreach (var mechInfo in level.mechanics)
        {
            var mech = mechInfo.Mechanic;
            mech.transform.SetParent(mechanicsParent);
            mech.DisablePreview();
        }
        
        // TODO: Load Connections
        foreach (var connInfo in level.connections)
        {
            var connection = Instantiate(connectionPrefab, connectionsParent);
            connection.SetupConnection(connInfo.fromMechanic, connInfo.toMechanic);
        }
    }
    
#if UNITY_EDITOR
    public void SavePrefab()
    {
        if (!gameObject.scene.IsValid()) 
        {
            Debug.LogError("Cannot save prefab: The object is not in a valid scene.");
            return;
        }
        
        SaveMapInLevelEditor();
        
        LE_TileManager.Instance.EnableAllLayers();
        
        var prefabSavePath = SavePath + _fullLevelName + ".prefab";

        var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, prefabSavePath);
        if (prefab != null)
        {
            Debug.Log($"Prefab saved at: {prefabSavePath}");
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("Failed to save prefab!");
        }
    }
#endif
}
