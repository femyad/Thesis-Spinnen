using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


//
// ====== DATA CLASSES ======
//


// Stores transform, material, color, and mesh info for one object
[System.Serializable]
public class TransformData
{
    public string path; // hierarchy path from root( used for finding the object again)
    public Vector3 position; // local position
    public Quaternion rotation; // local rotation
    public Vector3 scale; //local scale
    public string materialName; //material reference
    public Color color; //material color

    // mesh reference for abdomen, spinnerets, prosoma, eyes, etc.
    public string meshName;
}


// Holds a list of TransformData objects
[System.Serializable]
public class TransformDataList
{
    public List<TransformData> items = new List<TransformData>();
}



// Stores scaling compensation and overlap settings from GeneralScaler
[System.Serializable]
public class GeneralScalerData
{
    public Vector3 prosomaCompensation;
    public Vector3 abdomenCompensation;

    // dynamic list values you already use elsewhere
    public List<float[]> overlapSets = new List<float[]>();

    // per-set Min Overlap Multiplier (already working)
    public List<float> minOverlapMultipliers = new List<float>();

    // SAME PATTERN as minOverlapMultipliers: one list per joint, 4 values (Set1..Set4)
    public List<float> overlapCoxaToTrochanter = new List<float>();
    public List<float> overlapTrochanterToFemur = new List<float>();
    public List<float> overlapFemurToPatella = new List<float>();
    public List<float> overlapPatellaToTibia = new List<float>();
    public List<float> overlapTibiaToMetatarsus = new List<float>();
    public List<float> overlapMetatarsusToTarsus = new List<float>();
}



// Root save data that contains both transform and scaler data
[System.Serializable]
public class SpiderSaveData
{
    public TransformDataList transformData = new TransformDataList();
    public GeneralScalerData scalerData = new GeneralScalerData();
}



//
// ====== MAIN CLASS ======
//

/// <summary>
/// Handles saving and loading of spider configurations:
/// - Saves hierarchy transforms, materials, meshes
/// - Saves GeneralScaler settings
/// - Supports loading configs back into the scene
/// - Provides UI integration for save/load
/// </summary>
public class SpiderTransformManager : MonoBehaviour
{
    [Header("Spider Config Settings")]
    public string spiderName = "NewSpider";
    public GeneralScaler generalScaler;

    [Header("UI Elements")]
    public TMP_InputField spiderNameInput;
    public TMP_Dropdown configDropdown;
    public Button saveButton;
    public Button loadButton;
    public Button refreshButton;

    void Start()
    {
        // Bind UI buttons to methods
        if (saveButton != null) saveButton.onClick.AddListener(SaveFromUI);
        if (loadButton != null) loadButton.onClick.AddListener(LoadFromUI);
        if (refreshButton != null) refreshButton.onClick.AddListener(RefreshDropdown);

        RefreshDropdown(); // fill dropdown at start
    }

    // ======================= SAVE =======================
    /// <summary>
    /// Saves hierarchy transforms, materials, meshes, and GeneralScaler settings into a JSON file.
    /// </summary>
    public void SaveTransforms(string savePath)
    {
        SpiderSaveData fullData = new SpiderSaveData();

        // collect transforms/materials/meshes
        foreach (Transform child in transform)
            TraverseHierarchy(child, "", fullData.transformData);

        // collect GeneralScaler values
        if (generalScaler != null)
        {
            fullData.scalerData.prosomaCompensation = generalScaler.prosomaOverlapCompensation;
            fullData.scalerData.abdomenCompensation = generalScaler.abdomenOverlapCompensation;

            // dynamic overlap arrays
            fullData.scalerData.overlapSets.Clear();
            foreach (var set in generalScaler.overlapSets)
            {
                if (set == null || set.overlaps == null) continue;
                float[] values = new float[set.overlaps.Length];
                for (int i = 0; i < values.Length; i++)
                    values[i] = set.overlaps[i].value;
                fullData.scalerData.overlapSets.Add(values);
            }

            // per-set Min Overlap Multiplier
            fullData.scalerData.minOverlapMultipliers.Clear();
            fullData.scalerData.minOverlapMultipliers.Add(generalScaler.overlapSet1 != null ? generalScaler.overlapSet1.minOverlapMultiplier : 0f);
            fullData.scalerData.minOverlapMultipliers.Add(generalScaler.overlapSet2 != null ? generalScaler.overlapSet2.minOverlapMultiplier : 0f);
            fullData.scalerData.minOverlapMultipliers.Add(generalScaler.overlapSet3 != null ? generalScaler.overlapSet3.minOverlapMultiplier : 0f);
            fullData.scalerData.minOverlapMultipliers.Add(generalScaler.overlapSet4 != null ? generalScaler.overlapSet4.minOverlapMultiplier : 0f);

            // six joint-overlap lists (same pattern as minOverlapMultipliers)
            fullData.scalerData.overlapCoxaToTrochanter = SaveJoint(j => j.overlapCoxaToTrochanter);
            fullData.scalerData.overlapTrochanterToFemur = SaveJoint(j => j.overlapTrochanterToFemur);
            fullData.scalerData.overlapFemurToPatella = SaveJoint(j => j.overlapFemurToPatella);
            fullData.scalerData.overlapPatellaToTibia = SaveJoint(j => j.overlapPatellaToTibia);
            fullData.scalerData.overlapTibiaToMetatarsus = SaveJoint(j => j.overlapTibiaToMetatarsus);
            fullData.scalerData.overlapMetatarsusToTarsus = SaveJoint(j => j.overlapMetatarsusToTarsus);
        }

        // write to file
        string json = JsonUtility.ToJson(fullData, true);
        File.WriteAllText(savePath, json);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
        Debug.Log($"Saved spider config to: {savePath}");
    }

    // helper to gather a 4-value list (set1..set4) for a specific joint field
    private List<float> SaveJoint(System.Func<GeneralScaler.JointOverlapSettings, float> getter)
    {
        var list = new List<float>(4);
        list.Add(generalScaler.overlapSet1 != null ? getter(generalScaler.overlapSet1) : 0f);
        list.Add(generalScaler.overlapSet2 != null ? getter(generalScaler.overlapSet2) : 0f);
        list.Add(generalScaler.overlapSet3 != null ? getter(generalScaler.overlapSet3) : 0f);
        list.Add(generalScaler.overlapSet4 != null ? getter(generalScaler.overlapSet4) : 0f);
        return list;
    }

    // ======================= LOAD =======================
    /// <summary>
    /// Loads hierarchy transforms, materials, meshes, and GeneralScaler settings from JSON text.
    /// </summary>
    public void LoadTransforms(string jsonText)
    {
        SpiderSaveData fullData = JsonUtility.FromJson<SpiderSaveData>(jsonText);
        var dataList = fullData.transformData;

        // restore transforms/materials/meshes
        foreach (var data in dataList.items)
        {
            if (string.IsNullOrEmpty(data.path)) continue;

            Transform target = transform.Find(data.path);
            if (target == null) continue;

            target.localPosition = data.position;
            target.localRotation = data.rotation;
            target.localScale = data.scale;

            // material + color
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = null;
                if (!string.IsNullOrEmpty(data.materialName))
                {
                    // try a common folder first
                    mat = Resources.Load<Material>("SpiderSkinMaterials/" + data.materialName);
                    if (mat == null)
                    {
                        // search anywhere under Resources
                        var allMats = Resources.LoadAll<Material>(string.Empty);
                        foreach (var m in allMats)
                        {
                            if (m != null && m.name == data.materialName) { mat = m; break; }
                        }
                    }
                }

                if (mat != null)
                {
                    renderer.material = new Material(mat);
                    if (renderer.material.HasProperty("_Color"))
                        renderer.material.color = data.color;
                }
            }

            // mesh (optional)
            if (!string.IsNullOrEmpty(data.meshName))
            {
                MeshFilter mf = target.GetComponent<MeshFilter>();
                SkinnedMeshRenderer smr = target.GetComponent<SkinnedMeshRenderer>();
                Mesh current = mf ? mf.sharedMesh : (smr ? smr.sharedMesh : null);

                if (current == null || current.name != data.meshName)
                {
                    Mesh loaded = TryLoadMeshByName(data.meshName);
                    if (loaded != null)
                    {
                        if (mf) mf.sharedMesh = loaded;
                        if (smr) smr.sharedMesh = loaded;
                    }
                    else
                    {
                        Debug.LogWarning($"[{name}] Could not find mesh '{data.meshName}' for '{data.path}'.");
                    }
                }
            }
        }

        // restore GeneralScaler values
        if (generalScaler != null && fullData.scalerData != null)
        {
            generalScaler.prosomaOverlapCompensation = fullData.scalerData.prosomaCompensation;
            generalScaler.abdomenOverlapCompensation = fullData.scalerData.abdomenCompensation;

            // dynamic list values
            for (int i = 0; i < generalScaler.overlapSets.Count; i++)
            {
                if (i < fullData.scalerData.overlapSets.Count)
                {
                    float[] values = fullData.scalerData.overlapSets[i];
                    for (int j = 0; j < values.Length && j < generalScaler.overlapSets[i].overlaps.Length; j++)
                        generalScaler.overlapSets[i].overlaps[j].value = values[j];
                }
            }

            // per-set Min Overlap Multiplier
            var mins = fullData.scalerData.minOverlapMultipliers;
            if (mins != null)
            {
                if (generalScaler.overlapSet1 != null && mins.Count > 0) generalScaler.overlapSet1.minOverlapMultiplier = mins[0];
                if (generalScaler.overlapSet2 != null && mins.Count > 1) generalScaler.overlapSet2.minOverlapMultiplier = mins[1];
                if (generalScaler.overlapSet3 != null && mins.Count > 2) generalScaler.overlapSet3.minOverlapMultiplier = mins[2];
                if (generalScaler.overlapSet4 != null && mins.Count > 3) generalScaler.overlapSet4.minOverlapMultiplier = mins[3];
            }

            // six joint-overlap lists (same pattern as minOverlapMultipliers)
            ApplyJointList(fullData.scalerData.overlapCoxaToTrochanter, (s, v) => s.overlapCoxaToTrochanter = v);
            ApplyJointList(fullData.scalerData.overlapTrochanterToFemur, (s, v) => s.overlapTrochanterToFemur = v);
            ApplyJointList(fullData.scalerData.overlapFemurToPatella, (s, v) => s.overlapFemurToPatella = v);
            ApplyJointList(fullData.scalerData.overlapPatellaToTibia, (s, v) => s.overlapPatellaToTibia = v);
            ApplyJointList(fullData.scalerData.overlapTibiaToMetatarsus, (s, v) => s.overlapTibiaToMetatarsus = v);
            ApplyJointList(fullData.scalerData.overlapMetatarsusToTarsus, (s, v) => s.overlapMetatarsusToTarsus = v);
        }

        Debug.Log("Spider settings loaded successfully!");
    }

    // set1..set4 from a single list; mirrors minOverlapMultipliers behavior
    private void ApplyJointList(List<float> values, System.Action<GeneralScaler.JointOverlapSettings, float> setter)
    {
        if (values == null) return;

        if (generalScaler.overlapSet1 != null && values.Count > 0) setter(generalScaler.overlapSet1, values[0]);
        if (generalScaler.overlapSet2 != null && values.Count > 1) setter(generalScaler.overlapSet2, values[1]);
        if (generalScaler.overlapSet3 != null && values.Count > 2) setter(generalScaler.overlapSet3, values[2]);
        if (generalScaler.overlapSet4 != null && values.Count > 3) setter(generalScaler.overlapSet4, values[3]);
    }

    // ======================= HELPERS =======================

    /// <summary>
    /// Recursively traverses hierarchy to collect transform, material, and mesh data.
    /// </summary>
    private void TraverseHierarchy(Transform current, string path, TransformDataList dataList)
    {
        string currentPath = string.IsNullOrEmpty(path) ? current.name : path + "/" + current.name;

        Renderer renderer = current.GetComponent<Renderer>();
        if (renderer != null)
        {
            // material snapshot
            Material mat = renderer.sharedMaterial;
            string matName = mat != null ? mat.name.Replace(" (Instance)", "") : "";
            Color col = mat != null && mat.HasProperty("_Color") ? mat.color : Color.white;

            // mesh snapshot
            string meshName = "";
            MeshFilter mf = current.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null) meshName = mf.sharedMesh.name;
            SkinnedMeshRenderer smr = current.GetComponent<SkinnedMeshRenderer>();
            if (string.IsNullOrEmpty(meshName) && smr != null && smr.sharedMesh != null) meshName = smr.sharedMesh.name;

            dataList.items.Add(new TransformData
            {
                path = currentPath,
                position = current.localPosition,
                rotation = current.localRotation,
                scale = current.localScale,
                materialName = matName,
                color = col,
                meshName = meshName
            });
        }

        foreach (Transform child in current)
            TraverseHierarchy(child, currentPath, dataList);
    }

    /// <summary>
    /// Attempts to load a mesh from Resources by name.
    /// </summary>
    private Mesh TryLoadMeshByName(string meshName)
    {
        if (string.IsNullOrEmpty(meshName)) return null;

        // try common folder first
        Mesh m = Resources.Load<Mesh>("SpiderMeshes/" + meshName);
        if (m != null) return m;

        // fallback: search all meshes in Resources
        var all = Resources.LoadAll<Mesh>(string.Empty);
        for (int i = 0; i < all.Length; i++)
            if (all[i] != null && all[i].name == meshName)
                return all[i];

        return null;
    }



    // ======================= UI — with "settings" folder structure =======================


    /// <summary>
    /// Saves configuration from UI input (spider name).
    /// Creates folder and writes JSON file.
    /// </summary>
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

    /// <summary>
    /// Loads selected configuration from UI dropdown.
    /// </summary>
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

    /// <summary>
    /// Refreshes dropdown list by scanning JSON config files.
    /// </summary>
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
            Debug.Log("Found config: " + name);
        }

        configDropdown.ClearOptions();
        configDropdown.AddOptions(options);
    }
}
