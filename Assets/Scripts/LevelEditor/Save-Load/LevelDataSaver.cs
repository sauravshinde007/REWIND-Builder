using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class LevelDataSaver
{
    private static string SerializeLevel(ScriptableLevel level)
    {
        // Using StringBuilder to create level
        var sb = new StringBuilder();

        // Add level name and index
        sb.Append($"{level.levelName}-{level.levelIndex}\n");
        
        // Store Player SpawnPoint
        sb.Append($"{Mathf.Floor(level.playerSpawnPosition.x)}:{Mathf.Floor(level.playerSpawnPosition.y)}\n");

        // Saving Tiles
        for (var index = 0; index < level.TileLayers.Count; index++)
        {
            var layer = level.TileLayers[index];
            var layerInfo = level.LayerInfos[index];
            sb.Append($"L{index}({layerInfo.name},{layerInfo.collidable})-");
            
            foreach (var savedTile in layer)
            {
                var pos = savedTile.position;
                var type = savedTile.tile.name;

                sb.Append($"{pos.x}:{pos.y}:{type},");
            }
            sb.Append(";\n");
        }

        // Saving Mechanics
        foreach (var mechInfo in level.mechanics)
        {
            if (mechInfo.Mechanic == null)
            {
                Debug.Log("Mechanic is null");
                continue;
            }
            
            sb.Append($"ME-{mechInfo.name}:{mechInfo.position.x}:{mechInfo.position.y}:");
            var imech = mechInfo.Mechanic.actualMechanicGO.GetComponent<IMechanic>();
            var type = imech.GetType();
            
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var attrib = field.GetCustomAttribute<MechanicAttribute>();
                if (attrib == null) continue;
    
                if (field.FieldType == typeof(float)) {
                    sb.Append($"{field.Name}={(float)field.GetValue(imech)}=float,");
                }
                else if (field.FieldType == typeof(int)) {
                    sb.Append($"{field.Name}={(int)field.GetValue(imech)}=int,");
                }
                else if (field.FieldType == typeof(bool)) {
                    sb.Append($"{field.Name}={(bool)field.GetValue(imech)}=bool,");
                }
                else if (field.FieldType == typeof(string))
                {
                    sb.Append($"{field.Name}={(string)field.GetValue(imech)}=string,");
                }
            }

            sb.Append(";\n");
        }
        
        // Saving Connections
        foreach (var connInfo in level.connections)
        {
            var toMech = connInfo.toMechanic;
            var fromMech = connInfo.fromMechanic;

            if (toMech == null || fromMech == null) continue;
            
            sb.Append($"CO-{fromMech.name}:{toMech.name};\n");
        }

        // Return as a normal string
        return sb.ToString();
    }

    private static ScriptableLevel DeserializeLevel(string[] dataLines)
    {
        var newLevel = ScriptableObject.CreateInstance<ScriptableLevel>();

        newLevel.name = dataLines[0];
        var spawnPoint = dataLines[1].Split(':');
        newLevel.playerSpawnPosition = new Vector2(float.Parse(spawnPoint[0]), float.Parse(spawnPoint[1]));
        
        // Don't change the order
        newLevel.LayerInfos = new List<LayerInfo>();
        newLevel.TileLayers = GenerateTilesFromFile(dataLines).ToList();
        newLevel.mechanics = GenerateMechanicsFromFile(dataLines).ToList();
        newLevel.connections = GenerateConnectionFromFile(dataLines).ToList();

        return newLevel;

        // Mechanics Loading
        IEnumerable<MechanicInfo> GenerateMechanicsFromFile(string[] dataLines)
        {
            var mechanics = new List<MechanicInfo>();
            var allMechanics = LE_MechanicManager.Instance.allMechanics;
            for (var i = 0; i < dataLines.Length; i++)
            {
                if (!dataLines[i].StartsWith("ME")) continue;
                var mechInfo = new MechanicInfo();
                
                var parts = dataLines[i].Split(':');
                var namePart = parts[0];
                var attributesPart = parts[3].Split(',');
                
                // Position
                var position = new Vector3Int(int.Parse(parts[1]), int.Parse(parts[2]), 0);
                mechInfo.position = position;
                
                // Name-part resolution
                var underscoreIndex = namePart.IndexOf('_');
                var mechName = namePart.Substring(3, underscoreIndex - 3);
                var mechNumber = namePart.Substring(underscoreIndex+1, namePart.Length - underscoreIndex - 1);
                mechInfo.name = mechName + "_" + mechNumber;
                
                IMechanic imechanic = null;
                
                foreach (var mech in allMechanics)
                {
                    if(!mech || mech.name != mechName) continue;
                    
                    var imech = mech.actualMechanicGO.GetComponent<IMechanic>();
                    if (imech == null) continue;
                    
                    // if (imech.GetType().Name != mechName) continue;
                    mechInfo.Mechanic = mech;
                    imechanic = imech;
                    break;
                }
                
                // Attribute part resolution
                for (var index = 0; index < attributesPart.Length - 1; index++)
                {
                    var part = attributesPart[index].Split('=');
                    var attribName = part[0];
                    var attribValue = part[1];
                    var attribType = part[2];

                    var field = imechanic.GetType().GetField(attribName);
                    if (field == null) continue;
                    
                    switch (attribType)
                    {
                        case "float":
                            field.SetValue(imechanic, float.Parse(attribValue));
                            break;
                        case "int":
                            field.SetValue(imechanic, int.Parse(attribValue));
                            break;
                        case "bool":
                            field.SetValue(imechanic, bool.Parse(attribValue));
                            break;
                        case "string":
                            field.SetValue(imechanic, attribValue);
                            break;
                    }
                }
                
                mechInfo.Mechanic = mechInfo.Mechanic.ReplaceMechanic(imechanic, position);
                mechInfo.Mechanic.name = mechName;
                mechanics.Add(mechInfo);
            }

            return mechanics;
        }

        IEnumerable<List<SavedTile>> GenerateTilesFromFile(string[] dataLines)
        {
            var tiles = new List<List<SavedTile>>();
            foreach (var t in dataLines)
            {
                // Only for layers
                if(!t.StartsWith("L")) continue;
                tiles.Add(new List<SavedTile>());

                var startIndex = t.IndexOf('-');
                var endIndex = t.IndexOf(';');
                var tilesData = t.Substring(startIndex+1, endIndex - startIndex);
                
                var layerData = t.Substring(1, startIndex - 1);
                
                var brace1Index = layerData.IndexOf('(');
                var brace2Index = layerData.IndexOf(')');

                var layerNumber = int.Parse(layerData.Substring(0, brace1Index));
                var layerInfo = layerData.Substring(brace1Index+1, brace2Index - brace1Index - 1).Split(',');
                var layerName = layerInfo[0];
                var layerCollidable = bool.Parse(layerInfo[1]);
                
                var info = new LayerInfo
                {
                    name = layerName,
                    collidable = layerCollidable
                };
                
                newLevel.LayerInfos.Add(info);

                var data = tilesData.Split(',');

                for (var index = 0; index < data.Length-1; index++)
                {
                    var val = data[index];
                    
                    var singleTile = val.Split(":");
                    var posX = int.Parse(singleTile[0]);
                    var posY = int.Parse(singleTile[1]);
                    
                    var pos = new Vector3Int(posX, posY, 0);
                    var tileName = singleTile[2];

                    LevelTile levelTile = null;
                    foreach (var tileBase in LE_TileManager.Instance.tileToPlace)
                    {
                        var tile = (LevelTile)tileBase;
                        if (tile.name == tileName)
                        {
                            levelTile = tile;
                        }
                    }

                    var savedTile = new SavedTile()
                    {
                        position = pos,
                        tile = levelTile,
                    };
                    tiles[^1].Add(savedTile);
                }
            }

            return tiles;
        }

        // Connections
        IEnumerable<ConnectionInfo> GenerateConnectionFromFile(string[] dataLines)
        {
            var connections = new List<ConnectionInfo>();
            
            foreach (var line in dataLines)
            {
                if(!line.StartsWith("CO")) continue;

                var connection = new ConnectionInfo();
                
                var part = line;
                
                var startIndex = part.IndexOf('-');
                var endIndex = part.IndexOf(';');
                if(endIndex - startIndex <= 1) continue;
                
                var parts = part.Substring(startIndex + 1, endIndex - startIndex - 1).Split(':');
                var fromMechName = parts[0];
                var toMechName = parts[1];
                
                connection.fromMechanic = GetMechanicFromName(fromMechName);
                connection.toMechanic = GetMechanicFromName(toMechName);
                connections.Add(connection);
            }

            return connections;
        }

        Mechanic GetMechanicFromName(string mechName)
        {
            foreach (var mech in newLevel.mechanics.Where(mech => mech.name == mechName))
            {
                return mech.Mechanic;
            }

            Debug.LogError("Mechanic not found: " + mechName);
            return null;
        }

    }

    public static void SaveLevel(ScriptableLevel level, Sector sector, string levelName, int levelIndex)
    {
        var fileName = $"{sector}-{levelName}-{levelIndex}.rew";
        var filePath = Path.Combine(Application.persistentDataPath, fileName);
        var finalString = SerializeLevel(level);
        
        Debug.Log(filePath);
        
        File.WriteAllText(filePath, finalString);
    }

    public static ScriptableLevel LoadLevel(Sector sector, string levelName, int levelIndex)
    {
        var filePath = Path.Combine(Application.persistentDataPath, $"{sector}-{levelName}-{levelIndex}.rew");
        if (File.Exists(filePath))
        {
            var lineData = File.ReadAllLines(filePath);
            return DeserializeLevel(lineData);
        }
        else
        {
            // Handle Error if no files found
            Debug.LogError($"Did not find a level at: {filePath}");
            return null;
        }

    }
}
