using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

[CustomEditor(typeof(SpiderTransformManager))]
public class SpiderTransformManagerEditor : Editor
{
    private string[] availableConfigs;
    private int selectedConfigIndex = 0;
    private string configFolderPath = "Assets/JSON files";

    // Unique key to store selection per object
    private string prefsKey => $"SpiderConfig_Selected_{target.GetInstanceID()}";

    private void OnEnable()
    {
        LoadAvailableConfigs();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpiderTransformManager manager = (SpiderTransformManager)target;

        GUILayout.Space(10);
        GUILayout.Label(" Spider Config Manager", EditorStyles.boldLabel);

        if (GUILayout.Button(" Save Current Settings"))
        {
            if (string.IsNullOrEmpty(manager.spiderName))
            {
                Debug.LogError(" Spider name is empty! Please set it.");
                return;
            }

            string folderPath = Path.Combine(configFolderPath, manager.spiderName + "Settings");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, manager.spiderName + ".json");
            manager.SaveTransforms(filePath);

            // Refresh configs after saving
            LoadAvailableConfigs();
        }

        GUILayout.Space(10);

        if (GUILayout.Button(" Refresh Config List"))
        {
            LoadAvailableConfigs();
        }

        if (availableConfigs != null && availableConfigs.Length > 0)
        {
            int newSelectedIndex = EditorGUILayout.Popup("Select Config", selectedConfigIndex, availableConfigs);

            if (newSelectedIndex != selectedConfigIndex)
            {
                selectedConfigIndex = newSelectedIndex;
                EditorPrefs.SetString(prefsKey, availableConfigs[selectedConfigIndex]);

                string selectedFile = availableConfigs[selectedConfigIndex];
                string fullPath = Directory.GetFiles(configFolderPath, "*.json", SearchOption.AllDirectories)
                                            .FirstOrDefault(f => f.EndsWith(selectedFile));

                if (!string.IsNullOrEmpty(fullPath))
                {
                    string jsonText = File.ReadAllText(fullPath);
                    manager.LoadTransforms(jsonText);
                    Debug.Log($"Auto-loaded config: {selectedFile}");
                }
                else
                {
                    Debug.LogError("Selected config file not found!");
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No configs found. Click 'Refresh Config List' to search.", MessageType.Info);
        }
    }

    private void LoadAvailableConfigs()
    {
        if (Directory.Exists(configFolderPath))
        {
            availableConfigs = Directory.GetFiles(configFolderPath, "*.json", SearchOption.AllDirectories)
                                        .Select(f => Path.GetFileName(f))
                                        .OrderBy(name => name)
                                        .ToArray();

            string savedConfig = EditorPrefs.GetString(prefsKey, null);

            if (!string.IsNullOrEmpty(savedConfig))
            {
                int index = System.Array.IndexOf(availableConfigs, savedConfig);
                selectedConfigIndex = index >= 0 ? index : 0;
            }
            else
            {
                selectedConfigIndex = 0;
            }

            Debug.Log($" Found {availableConfigs.Length} config(s).");
        }
        else
        {
            availableConfigs = new string[0];
            selectedConfigIndex = 0;
            Debug.LogWarning(" Config folder not found!");
        }
    }
}
