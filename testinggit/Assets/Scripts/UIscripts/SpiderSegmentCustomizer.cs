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
        foreach (Transform child in spiderRoot.transform)
        {
            if (child.name.StartsWith("Leg"))
                legRoots[child.name] = child.gameObject;
        }

        // Auto-detect toggles
        Toggle[] allToggles = GetComponentsInChildren<Toggle>(true);
        foreach (Toggle toggle in allToggles)
        {
            if (toggle.name.StartsWith("Toggle_Leg"))
                legToggles.Add(toggle);
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

        SetSegment("patella");
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
            xSlider.SetValueWithoutNotify(scale.x);
            ySlider.SetValueWithoutNotify(scale.y);
            zSlider.SetValueWithoutNotify(scale.z);
        }
    }

    public void SetColor(Color color)
    {
        var segments = GetSelectedLegSegments();
        foreach (var obj in segments)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = color;
        }
    }

    public void SetXScale(float value) => ApplyScale(value, "x");
    public void SetYScale(float value) => ApplyScale(value, "y");
    public void SetZScale(float value) => ApplyScale(value, "z");

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
}
