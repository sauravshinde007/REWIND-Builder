using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ScriptableLevel : ScriptableObject
{
    public Vector2 playerSpawnPosition;
    public string levelName;
    public int levelIndex;
    public Sector sector;
    
    public List<List<SavedTile>> TileLayers;
    public List<LayerInfo> LayerInfos;
    public List<MechanicInfo> mechanics;
    public List<ConnectionInfo> connections;
}

[System.Serializable]
public class SavedTile
{
    public Vector3Int position;
    public LevelTile tile;
}

[System.Serializable]
public class MechanicInfo
{
    public string name;
    public Vector3Int position;
    public Mechanic Mechanic;
}

[System.Serializable]
public class ConnectionInfo
{
    public string name;
    public Mechanic toMechanic;
    public Mechanic fromMechanic;
}
