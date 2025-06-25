using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpiderSegmentCustomizer : MonoBehaviour
{
    public GameObject spiderRoot; // Drag "Testspider2"
    public TMP_Text currentSegmentLabel;
    public Slider xSlider, ySlider, zSlider;

    private string currentSegmentName = "patella";
    private Dictionary<string, GameObject> legRoots = new();
    private List<Toggle> legToggles = new();

    public TMP_Text xValueText;
    public TMP_Text yValueText;
    public TMP_Text zValueText;


    public List<Material> spiderSkinMaterials = new();

    [HideInInspector]
    public Toggle applyToAllSegmentsToggle;


    [Header("UI Panels")]
    public GameObject panelLegs;
    public GameObject panelAbdomen;
    public GameObject panelProsoma;
    public GameObject panelBaseModel;

    [Header("Tab Buttons")]
    public Button buttonLegs;
    public Button buttonAbdomen;
    public Button buttonProsoma;
    public Button buttonBaseModel;

    [Header("Tab Colors")]
    public Color activeTabColor = Color.white;
    public Color inactiveTabColor = Color.gray;



    private Dictionary<string, Color> colorMap = new()
    {
        {"Color_Red", Color.red},
        {"Color_Green", Color.green},
        {"Color_Blue", Color.blue},
        {"Color_Black", Color.black},
        {"Color_White", Color.white}
    };

    void Start()
    {
        // Auto-detect leg roots
        Transform[] all = spiderRoot.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in all)
        {
            if (child.name.StartsWith("Leg"))
            {
                legRoots[child.name] = child.gameObject;
                Debug.Log($"Found leg: {child.name}");
            }
        }


        // Auto-detect toggles
        Toggle[] allToggles = GetComponentsInChildren<Toggle>(true);
        foreach (Toggle toggle in allToggles)
        {
            if (toggle.name.StartsWith("Toggle_Leg"))
                legToggles.Add(toggle);
        }
        foreach (var toggle in legToggles)
        {
            toggle.isOn = false; // Uncheck all by default
        }



        // Auto-detect color buttons
        foreach (var entry in colorMap)
        {
            GameObject btn = GameObject.Find(entry.Key);
            if (btn != null)
                btn.GetComponent<Button>().onClick.AddListener(() => SetColor(entry.Value));
        }

        // Auto-detect segment buttons
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            string btnName = btn.gameObject.name.ToLower();
            if (btnName.EndsWith("_button"))
            {
                string segment = btnName.Replace("_button", "");
                btn.onClick.AddListener(() => SetSegment(segment));
            }
        }

        //Auto detect Apply to All Segments toggle
        GameObject toggleObj = GameObject.Find("Toggle_ApplyToAllSegments");
        if (toggleObj != null)
        {
            applyToAllSegmentsToggle = toggleObj.GetComponent<Toggle>();
            Debug.Log("Auto-hooked ApplyToAllSegments toggle.");
        }
        else
        {
            Debug.LogWarning("Toggle_ApplyToAllSegments not found!");
        }



        // Select one leg by default so GetSelectedLegSegments() returns something
        Toggle firstLeg = legToggles.Find(t => t != null);
        if (firstLeg != null)
            firstLeg.isOn = true;

        // Manually update sliders after toggling (in case SetValueWithoutNotify didn’t get called)
        SetSegment("patella");


        //for the skin materials
        for (int i = 0; i < spiderSkinMaterials.Count; i++)
        {
            Material mat = spiderSkinMaterials[i];
            if (mat == null)
            {
                Debug.LogWarning($"Material at index {i} is null!");
                continue;
            }

            string btnName = "Button_" + mat.name.Replace("Mat_", "");
            GameObject btn = GameObject.Find(btnName);
            int index = i;

            if (btn != null)
            {
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.Log($"Button '{btnName}' clicked! Applying material: {spiderSkinMaterials[index].name}");
                    SetMaterial(spiderSkinMaterials[index]);
                });

                Debug.Log($"Linked {btnName} to material {mat.name}");
            }
            else
            {
                Debug.LogWarning($"Button '{btnName}' not found for material '{mat.name}'");
            }
        }

        //for activating different panels settings
        ShowPanel("legs");



    }

    public void SetSegment(string name)
    {
        currentSegmentName = name;
        currentSegmentLabel.text = char.ToUpper(name[0]) + name.Substring(1);

        // Set sliders to the scale of first matching segment
        var segments = GetSelectedLegSegments();
        if (segments.Count > 0)
        {
            var scale = segments[0].transform.localScale;
            // Update sliders (without triggering change)
            xSlider.SetValueWithoutNotify(scale.x);
            ySlider.SetValueWithoutNotify(scale.y);
            zSlider.SetValueWithoutNotify(scale.z);

            // Update text displays too
            xValueText.text = scale.x.ToString("F2");
            yValueText.text = scale.y.ToString("F2");
            zValueText.text = scale.z.ToString("F2");
        }



    }

    public void SetColor(Color color)
    {
        var targets = applyToAllSegmentsToggle.isOn
            ? GetAllSegmentsOfSelectedLegs()
            : GetSelectedLegSegments();

        foreach (var obj in targets)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = color;
        }
    }


    public void SetXScale(float value)
    {
        xValueText.text = value.ToString("F2");
        ApplyScale(value, "x");
    }

    public void SetYScale(float value)
    {
        yValueText.text = value.ToString("F2");
        ApplyScale(value, "y");
    }

    public void SetZScale(float value)
    {
        zValueText.text = value.ToString("F2");
        ApplyScale(value, "z");
    }


    private void ApplyScale(float value, string axis)
    {
        var segments = GetSelectedLegSegments();
        foreach (var obj in segments)
        {
            Vector3 scale = obj.transform.localScale;
            scale = axis switch
            {
                "x" => new Vector3(value, scale.y, scale.z),
                "y" => new Vector3(scale.x, value, scale.z),
                "z" => new Vector3(scale.x, scale.y, value),
                _ => scale
            };
            obj.transform.localScale = scale;
        }
    }

    private List<GameObject> GetSelectedLegSegments()
    {
        var result = new List<GameObject>();
        foreach (var toggle in legToggles)
        {
            if (!toggle.isOn) continue;

            string legName = toggle.name.Replace("Toggle_", "");
            if (!legRoots.ContainsKey(legName)) continue;

            var segment = FindSegmentInLeg(legRoots[legName].transform, currentSegmentName);
            if (segment != null)
                result.Add(segment.gameObject);
        }
        return result;
    }

    private Transform FindSegmentInLeg(Transform legRoot, string segmentName)
    {
        foreach (Transform child in legRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.ToLower() == segmentName.ToLower())
                return child;
        }
        return null;
    }


    public void SelectAllLegs()
    {
        foreach (var toggle in legToggles)
            toggle.isOn = true;

        // Optionally refresh sliders again
        SetSegment(currentSegmentName);
    }

    public void DeselectAllLegs()
    {
        foreach (var toggle in legToggles)
            toggle.isOn = false;
    }

    public void SetMaterial(Material mat)
    {
        var targets = applyToAllSegmentsToggle.isOn
            ? GetAllSegmentsOfSelectedLegs()
            : GetSelectedLegSegments();

        foreach (var obj in targets)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material = mat;
        }
    }

    private List<GameObject> GetAllSegmentsOfSelectedLegs()
    {
        var result = new List<GameObject>();

        foreach (var toggle in legToggles)
        {
            if (!toggle.isOn) continue;

            string legName = toggle.name.Replace("Toggle_", "");
            if (!legRoots.TryGetValue(legName, out GameObject legRoot)) continue;

            foreach (Transform segment in legRoot.GetComponentsInChildren<Transform>(true))
            {
                if (segment.GetComponent<Renderer>() != null)
                    result.Add(segment.gameObject);
            }
        }

        return result;
    }

    public void ShowPanel(string name)
    {
        // Toggle panel visibility
        panelLegs.SetActive(name == "legs");
        panelAbdomen.SetActive(name == "abdomen");
        panelProsoma.SetActive(name == "prosoma");
        panelBaseModel.SetActive(name == "baseModel");

        // Update tab colors
        UpdateTabColor(buttonLegs, name == "legs");
        UpdateTabColor(buttonAbdomen, name == "abdomen");
        UpdateTabColor(buttonProsoma, name == "prosoma");
        UpdateTabColor(buttonBaseModel, name == "baseModel");
    }

    private void UpdateTabColor(Button btn, bool isActive)
    {
        var image = btn.GetComponent<Image>();
        if (image != null)
        {
            image.color = isActive ? activeTabColor : inactiveTabColor;
        }
    }


}
