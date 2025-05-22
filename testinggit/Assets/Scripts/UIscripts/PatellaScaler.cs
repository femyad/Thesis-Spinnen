using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatellaScaler : MonoBehaviour
{
    public List<GameObject> allPatellas;  // Drag all patellas here
    public GameObject singlePatella;      // Drag single patella here

    public Toggle applyToAllToggle;


    // Map color by button name
    private Dictionary<string, Color> colorMap = new Dictionary<string, Color>()
    {
        {"Color_Red", Color.red},
        {"Color_Green", Color.green},
        {"Color_Blue", Color.blue},
        {"Color_Black", Color.black},
        {"Color_White", Color.white}
    };

    void Start()
    {
        foreach (var entry in colorMap)
        {
            GameObject btn = GameObject.Find(entry.Key);
            if (btn != null)
            {
                btn.GetComponent<Button>().onClick.AddListener(() => SetColor(entry.Value));
            }
        }
    }

    public void SetColor(Color newColor)
    {
        if (applyToAllToggle.isOn)
        {
            foreach (GameObject patella in allPatellas)
            {
                var renderer = patella.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = newColor;
            }
        }
        else
        {
            var renderer = singlePatella.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = newColor;
        }
    }

    public void SetXScale(float value)
    {
        ApplyScale(value, axis: "x");
    }

    public void SetYScale(float value)
    {
        ApplyScale(value, axis: "y");
    }

    public void SetZScale(float value)
    {
        ApplyScale(value, axis: "z");
    }

    private void ApplyScale(float value, string axis)
    {
        if (applyToAllToggle.isOn)
        {
            foreach (GameObject patella in allPatellas)
            {
                Vector3 scale = patella.transform.localScale;
                scale = UpdateAxis(scale, value, axis);
                patella.transform.localScale = scale;
            }
        }
        else
        {
            Vector3 scale = singlePatella.transform.localScale;
            scale = UpdateAxis(scale, value, axis);
            singlePatella.transform.localScale = scale;
        }
    }

    private Vector3 UpdateAxis(Vector3 scale, float value, string axis)
    {
        switch (axis)
        {
            case "x": scale.x = value; break;
            case "y": scale.y = value; break;
            case "z": scale.z = value; break;
        }
        return scale;
    }
}
