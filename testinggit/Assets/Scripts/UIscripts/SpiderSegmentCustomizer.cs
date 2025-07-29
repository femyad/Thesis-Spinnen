using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Drawing;
using Unity.VisualScripting;
using Color = UnityEngine.Color;


public class SpiderSegmentCustomizer : MonoBehaviour
{
    public GameObject spiderRoot; // Drag "Testspider2"
    public TMP_Text currentSegmentLabel;
    public Slider xSlider, ySlider, zSlider;

    private string currentSegmentName = "patella";
    private Dictionary<string, GameObject> legRoots = new();
    private List<Toggle> legToggles = new();

    // for color picker
    private Color lastColor;
    public FlexibleColorPicker colorPicker;


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
    public GameObject panelFinishing;

    [Header("Tab Buttons")]
    public Button buttonLegs;
    public Button buttonAbdomen;
    public Button buttonProsoma;
    public Button buttonBaseModel;
    public Button buttonFinishing;



    [Header("Tab Colors")]
    public Color activeTabColor = Color.white;
    public Color inactiveTabColor = Color.gray;

    [Header("Segment Buttons")]
    public List<Button> segmentButtons = new List<Button>();

    [Header("Segment Colors")]
    public Color activeSegmentColor = Color.white;
    public Color inactiveSegmentColor = Color.gray;


    [Header("Leg Checklist Colors")]
    public Color activeLegColor = Color.green;
    public Color inactiveLegColor = Color.white;

    [Header("Cinemachine Virtual Cameras")]
    public Cinemachine.CinemachineVirtualCamera cmBaseModel;
    public Cinemachine.CinemachineVirtualCamera cmLegs;
    public Cinemachine.CinemachineVirtualCamera cmAbdomen;
    public Cinemachine.CinemachineVirtualCamera cmProsoma;
    public Cinemachine.CinemachineVirtualCamera cmFinishing;

    
    // for launching the spider
    public GameObject customizationUI; // reference to the parent Panel
    public GameObject spiderGameMode; // e.g. camera, input scripts, etc.

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
        foreach (var toggle in legToggles)
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(delegate { UpdateLegHighlights(); });
            }
        }
        // set highlights correctly on start too
        UpdateLegHighlights();

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
        SetSegment("coxa");


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


        //for color picker
        if (colorPicker != null)
        {
            lastColor = colorPicker.color;
            colorPicker.SetColor(lastColor); //  this line forces the big box to match
        }


        //for activating different panels settings
        ShowPanel("baseModel");



    }


    private void Update()
    {
        if (colorPicker == null) return;

        Color current = colorPicker.color;

        if (current != lastColor)
        {
            lastColor = current;
            SetColor(current); // this applies color to currentSegment or all selected
        }
    }

    public void SetSegment(string name)
    {


        foreach (var btn in segmentButtons)
        {
            if (btn == null) continue;

            string btnName = btn.name.ToLower();

            // segment buttons are usually named like "coxa_button"
            bool isActive = btnName.StartsWith(name.ToLower());

            var img = btn.GetComponent<Image>();
            if (img != null)
                img.color = isActive ? activeSegmentColor : inactiveSegmentColor;
        }


        currentSegmentName = name;

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
        UpdateLegHighlights();

    }

    public void DeselectAllLegs()
    {
        foreach (var toggle in legToggles)
            toggle.isOn = false;
        UpdateLegHighlights();

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
        panelBaseModel.SetActive(name == "baseModel");
        panelLegs.SetActive(name == "legs");
        panelAbdomen.SetActive(name == "abdomen");
        panelProsoma.SetActive(name == "prosoma");
        panelFinishing.SetActive(name == "finishing");

        // Update tab colors
        UpdateTabColor(buttonBaseModel, name == "baseModel");
        UpdateTabColor(buttonLegs, name == "legs");
        UpdateTabColor(buttonAbdomen, name == "abdomen");
        UpdateTabColor(buttonProsoma, name == "prosoma");
        UpdateTabColor(buttonFinishing, name == "finishing");
       
        if (name == "legs")
        {
            SetSegment("coxa");  // auto-pick the coxa segment
        }
        SetCameraView(name);

    }

    private void UpdateTabColor(Button btn, bool isActive)
    {
        var image = btn.GetComponent<Image>();
        if (image != null)
        {
            image.color = isActive ? activeTabColor : inactiveTabColor;
        }
    }

    public void UpdateLegHighlights()
    {
        foreach (var toggle in legToggles)
        {
            if (toggle == null) continue;

            if (toggle.targetGraphic != null)
            {
                toggle.targetGraphic.color = toggle.isOn ? activeLegColor : inactiveLegColor;
            }
        }
    }

    public void SetCameraView(string view)
    {
        cmBaseModel.Priority = (view == "baseModel" ? 10 : 0);
        cmLegs.Priority = (view == "legs") ? 10 : 0;
        cmAbdomen.Priority = (view == "abdomen") ? 10 : 0;
        cmProsoma.Priority = (view == "prosoma") ? 10 : 0;
        cmFinishing.Priority = (view == "finishing") ? 10 : 0;
    }



    public void LaunchSpider()
    {
        Debug.Log("Launching spider! ");

        customizationUI.SetActive(false);     // hide UI
        spiderGameMode.SetActive(true);       // show game mode logic

        // optionally enable movement script:
        //spiderRoot.GetComponent<SpiderMovement>()?.enabled = true;
    }


    


}
