using System.Collections.Generic;
using UnityEngine;

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
}
