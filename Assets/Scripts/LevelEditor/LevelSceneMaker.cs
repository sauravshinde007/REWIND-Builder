using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.SceneManagement;

public class LevelEditor : EditorWindow
{
    public SceneAsset templateScene; // Assign in the Editor Window
    public GameObject currentLevel;  // The object to copy
    public string defaultLevelName = "NewLevel";
    
    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditor>("Level Editor");
    }

    void OnGUI()
    {
        GUILayout.Label("Create New Level", EditorStyles.boldLabel);

        templateScene = (SceneAsset)EditorGUILayout.ObjectField("Template Scene", templateScene, typeof(SceneAsset), false);
        currentLevel = (GameObject)EditorGUILayout.ObjectField("Current Level", currentLevel, typeof(GameObject), true);
        defaultLevelName = EditorGUILayout.TextField("Default Level Name", defaultLevelName);

        if (currentLevel)
        {
            defaultLevelName = currentLevel.name;
        }

        if (GUILayout.Button("Create New Level"))
        {
            CreateNewLevel();
        }
    }

    void CreateNewLevel()
    {
        if (templateScene == null)
        {
            Debug.LogError("❌ No template scene assigned!");
            return;
        }

        string templatePath = AssetDatabase.GetAssetPath(templateScene);
        string scenePath = $"Assets/Scenes/Levels/{defaultLevelName}.unity";

        // Copy the template scene to a new file
        File.Copy(templatePath, scenePath, true);
        AssetDatabase.Refresh();

        // Open the new scene
        var newScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Add CurrentLevel object if assigned
        if (currentLevel)
        {
            var copiedLevel = Instantiate(currentLevel);
            SceneManager.MoveGameObjectToScene(copiedLevel, newScene);
            copiedLevel.name = "CurrentLevel";
        }

        // Save the scene
        EditorSceneManager.SaveScene(newScene);
        AssetDatabase.Refresh();

        Debug.Log($"✅ New scene saved: {scenePath}");
    }
}
