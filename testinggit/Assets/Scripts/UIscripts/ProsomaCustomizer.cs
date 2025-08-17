using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ProsomaCustomizer : MonoBehaviour
{
    [Header("Target Objects")]
    public GameObject prosomaObject;
    public GameObject pedipalpLeft;
    public GameObject pedipalpRight;
    public GameObject cheliceraeLeft;
    public GameObject cheliceraeRight;
    public GameObject eyes;
    public GameObject spiderRoot;

    [Header("Panels")]
    public GameObject panelProsoma;
    public GameObject panelProsoma2;

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

    [Header("Advanced: Pedipalps")]
    public Slider pedipalpXSlider;
    public Slider pedipalpYSlider;
    public Slider pedipalpZSlider;
    public TMP_Text pedipalpXValue;
    public TMP_Text pedipalpYValue;
    public TMP_Text pedipalpZValue;

    [Header("Advanced: Chelicerae")]
    public Slider cheliceraeXSlider;
    public Slider cheliceraeYSlider;
    public Slider cheliceraeZSlider;
    public TMP_Text cheliceraeXValue;
    public TMP_Text cheliceraeYValue;
    public TMP_Text cheliceraeZValue;

    [Header("Advanced: Eyes")]
    public Slider eyesXSlider;
    public Slider eyesYSlider;
    public Slider eyesZSlider;
    public TMP_Text eyesXValue;
    public TMP_Text eyesYValue;
    public TMP_Text eyesZValue;

    [Header("Prosoma Compensation")]
    public Slider compXSlider;
    public Slider compYSlider;
    public Slider compZSlider;
    public TMP_Text compXValue;
    public TMP_Text compYValue;
    public TMP_Text compZValue;

    [Header("Color Picker")]
    public FlexibleColorPicker colorPicker;

    [Header("Textures")]
    public List<Button> textureButtons;
    public List<Material> textureMaterials;

    [Header("Prosoma Shape Selection")]
    public List<GameObject> prosomaPrefabs;
    public List<Button> prosomaShapeButtons;


    [Header("Eye Shape Selection")]
    public List<GameObject> eyePrefabs;
    public List<Button> eyeShapeButtons;

    private Material sharedMaterial;
    private GeneralScaler generalScaler;

    private Vector3 offsetPedipalpL;
    private Vector3 offsetPedipalpR;
    private Vector3 offsetCheliceraeL;
    private Vector3 offsetCheliceraeR;
    private Vector3 offsetEyes;

    private Quaternion initialProsomaRotation;
    private Quaternion initialPedipalpLRot;
    private Quaternion initialPedipalpRRot;
    private Quaternion initialCheliceraeLRot;
    private Quaternion initialCheliceraeRRot;
    private Quaternion initialEyesRot;

    void Start()
    {
        InitSliders();
        InitTextureButtons();
        InitProsomaShapeButtons();
        InitEyeShapeButtons();

        generalScaler = spiderRoot.GetComponent<GeneralScaler>();

        sharedMaterial = new Material(Shader.Find("Standard"));
        ApplyMaterialToAllTargets(sharedMaterial);
        colorPicker.SetColor(sharedMaterial.color);

        initialProsomaRotation = prosomaObject.transform.rotation;

        if (pedipalpLeft)
        {
            offsetPedipalpL = pedipalpLeft.transform.position - prosomaObject.transform.position;
            initialPedipalpLRot = pedipalpLeft.transform.rotation;
        }

        if (pedipalpRight)
        {
            offsetPedipalpR = pedipalpRight.transform.position - prosomaObject.transform.position;
            initialPedipalpRRot = pedipalpRight.transform.rotation;
        }

        if (cheliceraeLeft)
        {
            offsetCheliceraeL = cheliceraeLeft.transform.position - prosomaObject.transform.position;
            initialCheliceraeLRot = cheliceraeLeft.transform.rotation;
        }

        if (cheliceraeRight)
        {
            offsetCheliceraeR = cheliceraeRight.transform.position - prosomaObject.transform.position;
            initialCheliceraeRRot = cheliceraeRight.transform.rotation;
        }

        if (eyes)
        {
            offsetEyes = eyes.transform.position - prosomaObject.transform.position;
            initialEyesRot = eyes.transform.rotation;
        }


        generalScaler.SetupProsomaAttachments(
        pedipalpLeft?.transform,
        pedipalpRight?.transform,
        cheliceraeLeft?.transform,
        cheliceraeRight?.transform,
        eyes?.transform,
        offsetPedipalpL,
        offsetPedipalpR,
        offsetCheliceraeL,
        offsetCheliceraeR,
        offsetEyes,
        initialProsomaRotation,
        initialPedipalpLRot,
        initialPedipalpRRot,
        initialCheliceraeLRot,
        initialCheliceraeRRot,
        initialEyesRot
        );

        Transform[] legRefs = new Transform[8];
        Vector3[] legOffsets = new Vector3[8];
        Quaternion[] legRotations = new Quaternion[8];

        legRefs[0] = GameObject.Find("LegL1")?.transform;
        legRefs[1] = GameObject.Find("LegL2")?.transform;
        legRefs[2] = GameObject.Find("LegL3")?.transform;
        legRefs[3] = GameObject.Find("LegL4")?.transform;
        legRefs[4] = GameObject.Find("LegR1")?.transform;
        legRefs[5] = GameObject.Find("LegR2")?.transform;
        legRefs[6] = GameObject.Find("LegR3")?.transform;
        legRefs[7] = GameObject.Find("LegR4")?.transform;

        for (int i = 0; i < 8; i++)
        {
            if (legRefs[i] != null)
            {
                legOffsets[i] = legRefs[i].position - prosomaObject.transform.position;
                legRotations[i] = legRefs[i].rotation;
            }
        }

        generalScaler.SetupLegAttachments(legRefs, legOffsets, legRotations);



        SyncTransformValues();
        SyncCompensationSliders();
        SyncSizeSliders();

        panelProsoma2.SetActive(false);
    }



    void LateUpdate()
    {
        if (sharedMaterial != null)
            sharedMaterial.color = colorPicker.color;

        Vector3 compScale = new Vector3(
            prosomaObject.transform.localScale.x * (1 - generalScaler.prosomaOverlapCompensation.x),
            prosomaObject.transform.localScale.y * (1 - generalScaler.prosomaOverlapCompensation.y),
            prosomaObject.transform.localScale.z * (1 - generalScaler.prosomaOverlapCompensation.z)
        );

        Quaternion currentProsomaRot = prosomaObject.transform.rotation;
        Quaternion deltaRotation = currentProsomaRot * Quaternion.Inverse(initialProsomaRotation);
        Vector3 pos = prosomaObject.transform.position;

        if (pedipalpLeft)
        {
            Vector3 scaled = Vector3.Scale(offsetPedipalpL, compScale);
            pedipalpLeft.transform.position = pos + currentProsomaRot * scaled;
            pedipalpLeft.transform.rotation = deltaRotation * initialPedipalpLRot;
        }

        if (pedipalpRight)
        {
            Vector3 scaled = Vector3.Scale(offsetPedipalpR, compScale);
            pedipalpRight.transform.position = pos + currentProsomaRot * scaled;
            pedipalpRight.transform.rotation = deltaRotation * initialPedipalpRRot;
        }

        if (cheliceraeLeft)
        {
            Vector3 scaled = Vector3.Scale(offsetCheliceraeL, compScale);
            cheliceraeLeft.transform.position = pos + currentProsomaRot * scaled;
            cheliceraeLeft.transform.rotation = deltaRotation * initialCheliceraeLRot;
        }

        if (cheliceraeRight)
        {
            Vector3 scaled = Vector3.Scale(offsetCheliceraeR, compScale);
            cheliceraeRight.transform.position = pos + currentProsomaRot * scaled;
            cheliceraeRight.transform.rotation = deltaRotation * initialCheliceraeRRot;
        }

        if (eyes)
        {
            Vector3 scaled = Vector3.Scale(offsetEyes, compScale);
            eyes.transform.position = pos + currentProsomaRot * scaled;
            eyes.transform.rotation = deltaRotation * initialEyesRot;
        }
    }


    void InitSliders()
    {
        xScaleSlider.onValueChanged.AddListener(v => {
            Vector3 s = prosomaObject.transform.localScale;
            s.x = v;
            prosomaObject.transform.localScale = s;
            xScaleValue.text = v.ToString("F2");
        });

        yScaleSlider.onValueChanged.AddListener(v => {
            Vector3 s = prosomaObject.transform.localScale;
            s.y = v;
            prosomaObject.transform.localScale = s;
            yScaleValue.text = v.ToString("F2");
        });

        zScaleSlider.onValueChanged.AddListener(v => {
            Vector3 s = prosomaObject.transform.localScale;
            s.z = v;
            prosomaObject.transform.localScale = s;
            zScaleValue.text = v.ToString("F2");
        });

        xRotationSlider.onValueChanged.AddListener(v => {
            Vector3 r = prosomaObject.transform.localEulerAngles;
            r.x = v;
            prosomaObject.transform.localEulerAngles = r;
            xRotationValue.text = r.x.ToString("F0") + "°";
        });

        yRotationSlider.onValueChanged.AddListener(v => {
            Vector3 r = prosomaObject.transform.localEulerAngles;
            r.y = v;
            prosomaObject.transform.localEulerAngles = r;
            yRotationValue.text = r.y.ToString("F0") + "°";
        });

        zRotationSlider.onValueChanged.AddListener(v => {
            Vector3 r = prosomaObject.transform.localEulerAngles;
            r.z = v;
            prosomaObject.transform.localEulerAngles = r;
            zRotationValue.text = r.z.ToString("F0") + "°";
        });

        SetupSizeSlider(pedipalpLeft, pedipalpRight, pedipalpXSlider, pedipalpXValue, "x");
        SetupSizeSlider(pedipalpLeft, pedipalpRight, pedipalpYSlider, pedipalpYValue, "y");
        SetupSizeSlider(pedipalpLeft, pedipalpRight, pedipalpZSlider, pedipalpZValue, "z");

        SetupSizeSlider(cheliceraeLeft, cheliceraeRight, cheliceraeXSlider, cheliceraeXValue, "x");
        SetupSizeSlider(cheliceraeLeft, cheliceraeRight, cheliceraeYSlider, cheliceraeYValue, "y");
        SetupSizeSlider(cheliceraeLeft, cheliceraeRight, cheliceraeZSlider, cheliceraeZValue, "z");

        SetupSizeSlider(eyes, null, eyesXSlider, eyesXValue, "x");
        SetupSizeSlider(eyes, null, eyesYSlider, eyesYValue, "y");
        SetupSizeSlider(eyes, null, eyesZSlider, eyesZValue, "z");

        compXSlider.onValueChanged.AddListener(v => {
            generalScaler.prosomaOverlapCompensation.x = v;
            compXValue.text = v.ToString("F2");
        });

        compYSlider.onValueChanged.AddListener(v => {
            generalScaler.prosomaOverlapCompensation.y = v;
            compYValue.text = v.ToString("F2");
        });

        compZSlider.onValueChanged.AddListener(v => {
            generalScaler.prosomaOverlapCompensation.z = v;
            compZValue.text = v.ToString("F2");
        });
    }

    void SetupSizeSlider(GameObject left, GameObject right, Slider slider, TMP_Text label, string axis)
    {
        slider.onValueChanged.AddListener(v => {
            if (left != null) left.transform.localScale = ApplyAxis(left.transform.localScale, v, axis);
            if (right != null) right.transform.localScale = ApplyAxis(right.transform.localScale, v, axis);
            if (label != null) label.text = v.ToString("F2");
        });
    }

    Vector3 ApplyAxis(Vector3 original, float value, string axis)
    {
        return axis switch
        {
            "x" => new Vector3(value, original.y, original.z),
            "y" => new Vector3(original.x, value, original.z),
            "z" => new Vector3(original.x, original.y, value),
            _ => original
        };
    }

    void ApplyMaterialToAllTargets(Material mat)
    {
        foreach (var part in new[] { prosomaObject, pedipalpLeft, pedipalpRight, cheliceraeLeft, cheliceraeRight, eyes })
        {
            if (part == null) continue;
            foreach (var r in part.GetComponentsInChildren<Renderer>(true))
                r.material = mat;
        }
    }

    void InitTextureButtons()
    {
        for (int i = 0; i < textureButtons.Count; i++)
        {
            int index = i;
            textureButtons[i].onClick.AddListener(() => {
                sharedMaterial = new Material(textureMaterials[index]);
                ApplyMaterialToAllTargets(sharedMaterial);
                colorPicker.SetColor(sharedMaterial.color);
            });
        }
    }

    /* void InitProsomaShapeButtons()
     {
         for (int i = 0; i < prosomaShapeButtons.Count; i++)
         {
             int index = i;
             prosomaShapeButtons[i].onClick.AddListener(() => {
                 foreach (var p in prosomaPrefabs) p.SetActive(false);
                 prosomaPrefabs[index].SetActive(true);
                 prosomaObject = prosomaPrefabs[index];
                 ApplyMaterialToAllTargets(sharedMaterial);
                 SyncTransformValues();
             });
         }
     }*/



    void InitProsomaShapeButtons()
    {
        for (int i = 0; i < prosomaShapeButtons.Count; i++)
        {
            int index = i;
            prosomaShapeButtons[i].onClick.AddListener(() =>
            {
                var newMesh = ExtractMeshFromPrefab(prosomaPrefabs[index]);
                if (newMesh == null)
                {
                    Debug.LogWarning($"No mesh found in prefab {prosomaPrefabs[index].name}");
                    return;
                }

                // Just apply directly to the current prosoma object
                var mf = prosomaObject.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    mf.sharedMesh = newMesh;
                }
                else
                {
                    var smr = prosomaObject.GetComponent<SkinnedMeshRenderer>();
                    if (smr != null) smr.sharedMesh = newMesh;
                    else Debug.LogError("Prosoma object has no MeshFilter or SkinnedMeshRenderer!");
                }

                ApplyMaterialToAllTargets(sharedMaterial);
                SyncTransformValues();
            });
        }
    }



    void InitEyeShapeButtons()
    {
        for (int i = 0; i < eyeShapeButtons.Count; i++)
        {
            int index = i;
            eyeShapeButtons[i].onClick.AddListener(() =>
            {
                var newMesh = ExtractMeshFromPrefab(eyePrefabs[index]);
                if (newMesh == null)
                {
                    Debug.LogWarning($"No mesh found in eye prefab {eyePrefabs[index].name}");
                    return;
                }

                // Apply the mesh to the existing 'eyes' object (no parenting/activating)
                var mf = eyes.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    mf.sharedMesh = newMesh;
                }
                else
                {
                    var smr = eyes.GetComponent<SkinnedMeshRenderer>();
                    if (smr != null) smr.sharedMesh = newMesh;
                    else Debug.LogError("Eyes object has no MeshFilter or SkinnedMeshRenderer!");
                }

                ApplyMaterialToAllTargets(sharedMaterial); // keep material consistent
                                                           // No need to change transforms; everything stays put.
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

    void SyncTransformValues()
    {
        Vector3 s = prosomaObject.transform.localScale;
        Vector3 r = prosomaObject.transform.localEulerAngles;

        xScaleSlider.SetValueWithoutNotify(s.x);
        yScaleSlider.SetValueWithoutNotify(s.y);
        zScaleSlider.SetValueWithoutNotify(s.z);
        xScaleValue.text = s.x.ToString("F2");
        yScaleValue.text = s.y.ToString("F2");
        zScaleValue.text = s.z.ToString("F2");

        xRotationSlider.SetValueWithoutNotify(r.x);
        yRotationSlider.SetValueWithoutNotify(r.y);
        zRotationSlider.SetValueWithoutNotify(r.z);
        xRotationValue.text = r.x.ToString("F0") + "°";
        yRotationValue.text = r.y.ToString("F0") + "°";
        zRotationValue.text = r.z.ToString("F0") + "°";
    }

    void SyncSizeSliders()
    {
        void Sync(GameObject obj, Slider x, TMP_Text xt, Slider y, TMP_Text yt, Slider z, TMP_Text zt)
        {
            if (obj == null) return;
            Vector3 s = obj.transform.localScale;
            x.SetValueWithoutNotify(s.x); xt.text = s.x.ToString("F2");
            y.SetValueWithoutNotify(s.y); yt.text = s.y.ToString("F2");
            z.SetValueWithoutNotify(s.z); zt.text = s.z.ToString("F2");
        }

        Sync(pedipalpLeft, pedipalpXSlider, pedipalpXValue, pedipalpYSlider, pedipalpYValue, pedipalpZSlider, pedipalpZValue);
        Sync(cheliceraeLeft, cheliceraeXSlider, cheliceraeXValue, cheliceraeYSlider, cheliceraeYValue, cheliceraeZSlider, cheliceraeZValue);
        Sync(eyes, eyesXSlider, eyesXValue, eyesYSlider, eyesYValue, eyesZSlider, eyesZValue);
    }

    void SyncCompensationSliders()
    {
        Vector3 c = generalScaler.prosomaOverlapCompensation;
        compXSlider.SetValueWithoutNotify(c.x); compXValue.text = c.x.ToString("F2");
        compYSlider.SetValueWithoutNotify(c.y); compYValue.text = c.y.ToString("F2");
        compZSlider.SetValueWithoutNotify(c.z); compZValue.text = c.z.ToString("F2");
    }

    public void ShowAdvancedProsomaPanel()
    {
        foreach (Transform child in panelProsoma.transform)
        {
            if (child.gameObject != panelProsoma2)
                child.gameObject.SetActive(false);
        }
        panelProsoma2.SetActive(true);
    }

    public void ReturnToBasicProsomaPanel()
    {
        foreach (Transform child in panelProsoma.transform)
        {
            child.gameObject.SetActive(true);
        }
        panelProsoma2.SetActive(false);
    }
}
