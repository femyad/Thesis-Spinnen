using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;


[System.Serializable]
public class TransformData
{
    public string path;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}







[System.Serializable]
public class TransformDataList
{
    public List<TransformData> items = new List<TransformData>();
}

public class SpiderTransformManager : MonoBehaviour
{
    [Header("Spider Config Settings")]
    public string spiderName = "NewSpider";  // Set the spider type name here


    [Header("UI Elements")]
    public TMP_InputField spiderNameInput;
    public TMP_Dropdown configDropdown;
    public Button saveButton;
    public Button loadButton;
    public Button refreshButton;



    void Start()
    {
        // Optional: hook buttons dynamically if needed
        if (saveButton != null) saveButton.onClick.AddListener(SaveFromUI);
        if (loadButton != null) loadButton.onClick.AddListener(LoadFromUI);
        if (refreshButton != null) refreshButton.onClick.AddListener(RefreshDropdown);

        RefreshDropdown(); // Load available configs on start
    }




    // Save current transforms to JSON
    public void SaveTransforms(string savePath)
    {
        TransformDataList dataList = new TransformDataList();

        // Start from children to skip the root object itself
        foreach (Transform child in transform)
        {
            TraverseHierarchy(child, "", dataList);
        }

        string json = JsonUtility.ToJson(dataList, true);
        System.IO.File.WriteAllText(savePath, json);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        Debug.Log($" Saved spider config to: {savePath}");
    }



    // Load transforms from JSON string
    public void LoadTransforms(string jsonText)
    {
        TransformDataList dataList = JsonUtility.FromJson<TransformDataList>(jsonText);

        foreach (var data in dataList.items)
        {
            if (string.IsNullOrEmpty(data.path))
            {
                Debug.LogWarning(" Empty path detected, skipping.");
                continue;
            }

            Transform target = transform.Find(data.path);
            if (target != null)
            {
                target.localPosition = data.position;
                target.localRotation = data.rotation;
                target.localScale = data.scale;
            }
            else
            {
                Debug.LogWarning($" Transform not found: {data.path}");
            }
        }

        Debug.Log(" Spider settings loaded successfully!");
    }



    // Recursive method to collect transform data
    private void TraverseHierarchy(Transform current, string path, TransformDataList dataList)
    {
        string currentPath = string.IsNullOrEmpty(path) ? current.name : path + "/" + current.name;

        // Check if this GameObject has a visible mesh
        bool hasMesh = current.GetComponent<MeshRenderer>() != null || current.GetComponent<SkinnedMeshRenderer>() != null;

        if (hasMesh)
        {
            TransformData data = new TransformData
            {
                path = currentPath,
                position = current.localPosition,
                rotation = current.localRotation,
                scale = current.localScale
            };

            dataList.items.Add(data);
        }

        // Continue traversing children regardless
        foreach (Transform child in current)
        {
            TraverseHierarchy(child, currentPath, dataList);
        }
    }



    public void SaveFromUI()
    {
        string name = spiderNameInput.text;
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Spider name is empty. Not saving.");
            return;
        }

        string folderPath = Path.Combine(Application.dataPath, "JSON files", name + "Settings");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, name + ".json");
        SaveTransforms(filePath);
    }


    public void LoadFromUI()
    {
        if (configDropdown.options.Count == 0) return;

        string selectedName = configDropdown.options[configDropdown.value].text;

        string fullPath = Directory.GetFiles(Path.Combine(Application.dataPath, "JSON files"), "*.json", SearchOption.AllDirectories)
                                   .FirstOrDefault(f => f.EndsWith(selectedName + ".json"));

        if (!string.IsNullOrEmpty(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            LoadTransforms(json);
        }
        else
        {
            Debug.LogWarning("Selected config not found.");
        }
    }




    public void RefreshDropdown()
    {
        string basePath = Path.Combine(Application.dataPath, "JSON files");
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        string[] files = Directory.GetFiles(basePath, "*.json", SearchOption.AllDirectories);
        List<string> options = new List<string>();

        foreach (var file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            options.Add(name);
            Debug.Log("Found config: " + name); // For debug
        }

        configDropdown.ClearOptions();
        configDropdown.AddOptions(options);
    }




}
