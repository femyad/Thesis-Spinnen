using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbdomenCustomizer : MonoBehaviour
{
    [Header("Target Objects")]
    public GameObject abdomenObject;
    public GameObject spinneretObject;
    public GameObject spiderRoot;

    [Header("Abdomen Panel References")]
    public GameObject panelAbdomen1;
    public GameObject panelAbdomen2;

    [Header("Scale Sliders")]
    public Slider xScaleSlider;
    public Slider yScaleSlider;
    public Slider zScaleSlider;
    public TMP_Text xScaleValue;
    public TMP_Text yScaleValue;
    public TMP_Text zScaleValue;

    [Header("Rotation Sliders")]
    public Slider xRotationSlider;
    public Slider yRotationSlider;
    public Slider zRotationSlider;
    public TMP_Text xRotationValue;
    public TMP_Text yRotationValue;
    public TMP_Text zRotationValue;

    [Header("Spinneret Size Sliders")]
    public Slider spinneretXSlider;
    public Slider spinneretYSlider;
    public Slider spinneretZSlider;
    public TMP_Text spinneretXValue;
    public TMP_Text spinneretYValue;
    public TMP_Text spinneretZValue;

    [Header("Abdomen Compensation Sliders")]
    public Slider compXSlider;
    public Slider compYSlider;
    public Slider compZSlider;
    public TMP_Text compXValue;
    public TMP_Text compYValue;
    public TMP_Text compZValue;

    [Header("Abdomen Shapes")]
    public List<Button> abdomenShapeButtons;
    public List<GameObject> abdomenPrefabs;

    [Header("Spinneret Shapes")]
    public List<Button> spinneretButtons;
    public List<GameObject> spinneretPrefabs;

    [Header("Textures")]
    public List<Button> textureButtons;
    public List<Material> textureMaterials;

    [Header("Color Picker")]
    public FlexibleColorPicker colorPicker;

    private Material abdomenMaterialInstance;
    private GeneralScaler generalScaler;

    void Start()
    {
        InitSliders();
        InitAbdomenButtons();
        InitSpinneretButtons();
        InitTextureButtons();
        SetupInitialMaterial();
        SyncSlidersWithAbdomen();

        generalScaler = spiderRoot.GetComponent<GeneralScaler>();
        SyncCompensationSliders();
        panelAbdomen2.SetActive(false);

    }

    void Update()
    {
        if (abdomenMaterialInstance != null)
        {
            abdomenMaterialInstance.color = colorPicker.color;
        }
    }

    void SetupInitialMaterial()
    {
        if (abdomenObject == null) return;

        Renderer rend = abdomenObject.GetComponent<Renderer>();
        if (rend != null)
        {
            abdomenMaterialInstance = new Material(rend.material);
            rend.material = abdomenMaterialInstance;

            if (spinneretObject != null)
            {
                Renderer sRend = spinneretObject.GetComponent<Renderer>();
                if (sRend != null)
                    sRend.material = abdomenMaterialInstance;
            }

            if (colorPicker != null)
                colorPicker.SetColor(abdomenMaterialInstance.color);
        }
    }

    void InitSliders()
    {
        xScaleSlider.onValueChanged.AddListener((v) => {
            Vector3 s = abdomenObject.transform.localScale;
            s.x = v;
            abdomenObject.transform.localScale = s;
            xScaleValue.text = v.ToString("F2");
        });

        yScaleSlider.onValueChanged.AddListener((v) => {
            Vector3 s = abdomenObject.transform.localScale;
            s.y = v;
            abdomenObject.transform.localScale = s;
            yScaleValue.text = v.ToString("F2");
        });

        zScaleSlider.onValueChanged.AddListener((v) => {
            Vector3 s = abdomenObject.transform.localScale;
            s.z = v;
            abdomenObject.transform.localScale = s;
            zScaleValue.text = v.ToString("F2");
        });

        xRotationSlider.onValueChanged.AddListener((v) => {
            Vector3 r = abdomenObject.transform.localEulerAngles;
            r.x = v;
            abdomenObject.transform.localEulerAngles = r;
            xRotationValue.text = v.ToString("F0") + "°";
        });

        yRotationSlider.onValueChanged.AddListener((v) => {
            Vector3 r = abdomenObject.transform.localEulerAngles;
            r.y = v;
            abdomenObject.transform.localEulerAngles = r;
            yRotationValue.text = v.ToString("F0") + "°";
        });

        zRotationSlider.onValueChanged.AddListener((v) => {
            Vector3 r = abdomenObject.transform.localEulerAngles;
            r.z = v;
            abdomenObject.transform.localEulerAngles = r;
            zRotationValue.text = v.ToString("F0") + "°";
        });

        spinneretXSlider.onValueChanged.AddListener((v) => {
            Vector3 s = spinneretObject.transform.localScale;
            s.x = v;
            spinneretObject.transform.localScale = s;
            spinneretXValue.text = v.ToString("F2");
        });

        spinneretYSlider.onValueChanged.AddListener((v) => {
            Vector3 s = spinneretObject.transform.localScale;
            s.y = v;
            spinneretObject.transform.localScale = s;
            spinneretYValue.text = v.ToString("F2");
        });

        spinneretZSlider.onValueChanged.AddListener((v) => {
            Vector3 s = spinneretObject.transform.localScale;
            s.z = v;
            spinneretObject.transform.localScale = s;
            spinneretZValue.text = v.ToString("F2");
        });

        compXSlider.onValueChanged.AddListener((v) => {
            if (generalScaler != null)
            {
                generalScaler.abdomenOverlapCompensation.x = v;
                compXValue.text = v.ToString("F2");
            }
        });

        compYSlider.onValueChanged.AddListener((v) => {
            if (generalScaler != null)
            {
                generalScaler.abdomenOverlapCompensation.y = v;
                compYValue.text = v.ToString("F2");
            }
        });

        compZSlider.onValueChanged.AddListener((v) => {
            if (generalScaler != null)
            {
                generalScaler.abdomenOverlapCompensation.z = v;
                compZValue.text = v.ToString("F2");
            }
        });
    }

    void SyncSlidersWithAbdomen()
    {
        if (abdomenObject != null)
        {
            Vector3 scale = abdomenObject.transform.localScale;
            Vector3 rot = abdomenObject.transform.localEulerAngles;

            xScaleSlider.SetValueWithoutNotify(scale.x);
            yScaleSlider.SetValueWithoutNotify(scale.y);
            zScaleSlider.SetValueWithoutNotify(scale.z);
            xScaleValue.text = scale.x.ToString("F2");
            yScaleValue.text = scale.y.ToString("F2");
            zScaleValue.text = scale.z.ToString("F2");

            xRotationSlider.SetValueWithoutNotify(rot.x);
            yRotationSlider.SetValueWithoutNotify(rot.y);
            zRotationSlider.SetValueWithoutNotify(rot.z);
            xRotationValue.text = rot.x.ToString("F0") + "°";
            yRotationValue.text = rot.y.ToString("F0") + "°";
            zRotationValue.text = rot.z.ToString("F0") + "°";
        }

        if (spinneretObject != null)
        {
            Vector3 spScale = spinneretObject.transform.localScale;

            spinneretXSlider.SetValueWithoutNotify(spScale.x);
            spinneretYSlider.SetValueWithoutNotify(spScale.y);
            spinneretZSlider.SetValueWithoutNotify(spScale.z);

            spinneretXValue.text = spScale.x.ToString("F2");
            spinneretYValue.text = spScale.y.ToString("F2");
            spinneretZValue.text = spScale.z.ToString("F2");
        }
    }

    void SyncCompensationSliders()
    {
        if (generalScaler == null) return;
        Vector3 comp = generalScaler.abdomenOverlapCompensation;

        compXSlider.SetValueWithoutNotify(comp.x);
        compYSlider.SetValueWithoutNotify(comp.y);
        compZSlider.SetValueWithoutNotify(comp.z);

        compXValue.text = comp.x.ToString("F2");
        compYValue.text = comp.y.ToString("F2");
        compZValue.text = comp.z.ToString("F2");
    }

    void InitAbdomenButtons()
    {
        for (int i = 0; i < abdomenShapeButtons.Count; i++)
        {
            int index = i;
            abdomenShapeButtons[i].onClick.AddListener(() =>
            {
                var newMesh = ExtractMeshFromPrefab(abdomenPrefabs[index]);
                if (newMesh == null)
                {
                    Debug.LogWarning($"No mesh found in abdomen prefab {abdomenPrefabs[index].name}");
                    return;
                }

                // apply to the existing abdomenObject
                var mf = abdomenObject.GetComponent<MeshFilter>();
                if (mf) mf.sharedMesh = newMesh;
                else
                {
                    var smr = abdomenObject.GetComponent<SkinnedMeshRenderer>();
                    if (smr) smr.sharedMesh = newMesh;
                    else Debug.LogError("Abdomen object has no MeshFilter or SkinnedMeshRenderer!");
                }

                // keep material consistent
                if (abdomenMaterialInstance != null)
                {
                    var r = abdomenObject.GetComponent<Renderer>();
                    if (r) r.material = abdomenMaterialInstance;
                }

                // keep sliders in sync
                SyncSlidersWithAbdomen();
            });
        }
    }


    void InitSpinneretButtons()
    {
        for (int i = 0; i < spinneretButtons.Count; i++)
        {
            int index = i;
            spinneretButtons[i].onClick.AddListener(() =>
            {
                var newMesh = ExtractMeshFromPrefab(spinneretPrefabs[index]);
                if (newMesh == null)
                {
                    Debug.LogWarning($"No mesh found in spinneret prefab {spinneretPrefabs[index].name}");
                    return;
                }

                // apply to the existing spinneretObject
                var mf = spinneretObject.GetComponent<MeshFilter>();
                if (mf) mf.sharedMesh = newMesh;
                else
                {
                    var smr = spinneretObject.GetComponent<SkinnedMeshRenderer>();
                    if (smr) smr.sharedMesh = newMesh;
                    else Debug.LogError("Spinneret object has no MeshFilter or SkinnedMeshRenderer!");
                }

                if (abdomenMaterialInstance != null)
                {
                    var r = spinneretObject.GetComponent<Renderer>();
                    if (r) r.material = abdomenMaterialInstance;
                }

                // sync spinneret size fields
                SyncSlidersWithAbdomen();
            });
        }
    }


    Mesh ExtractMeshFromPrefab(GameObject prefab)
    {
        var mf = prefab.GetComponentInChildren<MeshFilter>(true);
        if (mf && mf.sharedMesh) return mf.sharedMesh;

        var smr = prefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (smr && smr.sharedMesh) return smr.sharedMesh;

        return null;
    }


    void InitTextureButtons()
    {
        for (int i = 0; i < textureButtons.Count; i++)
        {
            int index = i;
            textureButtons[i].onClick.AddListener(() => {
                SetAbdomenMaterial(textureMaterials[index]);
            });
        }
    }

    void SetAbdomenMaterial(Material mat)
    {
        if (abdomenObject != null)
        {
            Renderer rend = abdomenObject.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = mat;
                abdomenMaterialInstance = rend.material;
            }
        }

        if (spinneretObject != null)
        {
            Renderer rend = spinneretObject.GetComponent<Renderer>();
            if (rend != null)
                rend.material = abdomenMaterialInstance;
        }

        if (colorPicker != null)
            colorPicker.SetColor(abdomenMaterialInstance.color);
    }

    public void ShowAdvancedAbdomenPanel()
    {
        
        // Optionally hide everything else in Abdomen1 except Abdomen2
        foreach (Transform child in panelAbdomen1.transform)
        {
            if (child.gameObject != panelAbdomen2)
                child.gameObject.SetActive(false);
        }

        // Show Abdomen2
        panelAbdomen2.SetActive(true);
    }

    public void ReturnToBasicAbdomenPanel()
    {
        

        // Restore all original children of Abdomen1
        foreach (Transform child in panelAbdomen1.transform)
        {
            child.gameObject.SetActive(true);
        }
        // Hide Abdomen2
        panelAbdomen2.SetActive(false);
    }
}