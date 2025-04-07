using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SpiderScaleData
{
    public Vector3 prosomaScale = Vector3.one;
    public Vector3 prosomaRotation = Vector3.zero;
    public Vector3 abdomenScale = Vector3.one;
    public Vector3 abdomenRotation = Vector3.zero;
}

public class SpiderScaler : MonoBehaviour
{
    public SpiderScaleData scaleData = new SpiderScaleData(); // Editable in Inspector
    public Transform prosoma;
    public Transform abdomen;

    [Header("Drop JSON File Here")]
    public TextAsset jsonFile;  // Drag JSON file here

    public string saveFileName = "spider_scale.json";  // Default save file name

    void Start()
    {
        LoadScaleData();
        ApplyTransformations();
    }

    void LoadScaleData()
    {
        if (jsonFile != null)
        {
            scaleData = JsonUtility.FromJson<SpiderScaleData>(jsonFile.text);
            Debug.Log("Loaded JSON: " + jsonFile.text);
        }
        else
        {
            Debug.LogWarning("No JSON file assigned! Using Inspector values.");
        }
    }

    void ApplyTransformations()
    {
        if (prosoma != null)
        {
            prosoma.localScale = scaleData.prosomaScale;
            prosoma.localRotation = Quaternion.Euler(scaleData.prosomaRotation);
        }

        if (abdomen != null)
        {
            abdomen.localScale = scaleData.abdomenScale;
            abdomen.localRotation = Quaternion.Euler(scaleData.abdomenRotation);
        }
    }

    // SAVE Function - Updates the original JSON file
    public void SaveScaleData()
    {
#if UNITY_EDITOR
        if (jsonFile != null)
        {
            // Get the original file path
            string path = AssetDatabase.GetAssetPath(jsonFile);

            if (!string.IsNullOrEmpty(path))
            {
                // Convert scaleData to JSON format
                string json = JsonUtility.ToJson(scaleData, true);

                // Write back to the original JSON file
                File.WriteAllText(path, json);

                // Refresh Unity to detect the change
                AssetDatabase.Refresh();

                Debug.Log("Saved JSON to: " + path);
            }
            else
            {
                Debug.LogError("Could not determine the file path of the JSON asset.");
            }
        }
        else
        {
            Debug.LogError("No JSON file assigned. Cannot save.");
        }
#endif
    }
}
