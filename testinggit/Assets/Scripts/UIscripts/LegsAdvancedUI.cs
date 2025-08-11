using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LegsAdvancedUI : MonoBehaviour
{
    [Header("References")]
    public GeneralScaler generalScaler;   // drag RoadToFinalSpider (with GeneralScaler) here
    public GameObject panelLegs;          // base legs panel container
    public GameObject panelLegs2;         // advanced legs panel (child of panelLegs, initially inactive)

    [Header("Leg Pair Buttons (tabs)")]
    public Button buttonSet1_L1R1;
    public Button buttonSet2_L2R2;
    public Button buttonSet3_L3R3;
    public Button buttonSet4_L4R4;

    [Header("Leg Pair Button Colors")]
    public Color activeLegSetColor = Color.white;
    public Color inactiveLegSetColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Advanced Legs Nav Buttons")]
    public Button buttonLegsAdvanced;     // on base panel: “More Legs settings”
    public Button buttonLegsBack;         // on Panel_Legs2: “Back”
    public Button buttonLegsDefault;      // on Panel_Legs2: “BaseModel Default”

    [Header("Overlap Sliders + Values")]
    public Slider sCoxaTrochanter; public TMP_Text vCoxaTrochanter;
    public Slider sTrochanterFemur; public TMP_Text vTrochanterFemur;
    public Slider sFemurPatella; public TMP_Text vFemurPatella;
    public Slider sPatellaTibia; public TMP_Text vPatellaTibia;
    public Slider sTibiaMetatarsus; public TMP_Text vTibiaMetatarsus;
    public Slider sMetatarsusTarsus; public TMP_Text vMetatarsusTarsus;
    public Slider sOverlapMultiplier; public TMP_Text vOverlapMultiplier; // maps to minOverlapMultiplier

    // live pointers to GeneralScaler’s sets
    private GeneralScaler.JointOverlapSettings[] legSets = new GeneralScaler.JointOverlapSettings[4];
    // snapshot taken on Start() for reset
    private GeneralScaler.JointOverlapSettings[] legSetsDefaults = new GeneralScaler.JointOverlapSettings[4];

    private int currentLegSetIndex = 0;

    void Start()
    {
        if (generalScaler == null)
        {
            Debug.LogError("LegsAdvancedUI: GeneralScaler reference is missing.");
            enabled = false;
            return;
        }

        // wire the sets
        legSets[0] = generalScaler.overlapSet1;
        legSets[1] = generalScaler.overlapSet2;
        legSets[2] = generalScaler.overlapSet3;
        legSets[3] = generalScaler.overlapSet4;

        // snapshot defaults
        for (int i = 0; i < 4; i++) legSetsDefaults[i] = CopySet(legSets[i]);

        // buttons
        if (buttonLegsAdvanced) buttonLegsAdvanced.onClick.AddListener(ShowAdvancedLegsPanel);
        if (buttonLegsBack) buttonLegsBack.onClick.AddListener(ReturnToBasicLegsPanel);
        if (buttonLegsDefault) buttonLegsDefault.onClick.AddListener(ResetCurrentLegSetToDefault);

        if (buttonSet1_L1R1) buttonSet1_L1R1.onClick.AddListener(() => SelectLegSet(0));
        if (buttonSet2_L2R2) buttonSet2_L2R2.onClick.AddListener(() => SelectLegSet(1));
        if (buttonSet3_L3R3) buttonSet3_L3R3.onClick.AddListener(() => SelectLegSet(2));
        if (buttonSet4_L4R4) buttonSet4_L4R4.onClick.AddListener(() => SelectLegSet(3));

        // sliders -> write-through to active set (ignore exponent per your instruction)
        sCoxaTrochanter.onValueChanged.AddListener(v => { legSets[currentLegSetIndex].overlapCoxaToTrochanter = v; vCoxaTrochanter.text = v.ToString("F2"); });
        sTrochanterFemur.onValueChanged.AddListener(v => { legSets[currentLegSetIndex].overlapTrochanterToFemur = v; vTrochanterFemur.text = v.ToString("F2"); });
        sFemurPatella.onValueChanged.AddListener(v => { legSets[currentLegSetIndex].overlapFemurToPatella = v; vFemurPatella.text = v.ToString("F2"); });
        sPatellaTibia.onValueChanged.AddListener(v => { legSets[currentLegSetIndex].overlapPatellaToTibia = v; vPatellaTibia.text = v.ToString("F2"); });
        sTibiaMetatarsus.onValueChanged.AddListener(v => { legSets[currentLegSetIndex].overlapTibiaToMetatarsus = v; vTibiaMetatarsus.text = v.ToString("F2"); });
        sMetatarsusTarsus.onValueChanged.AddListener(v => { legSets[currentLegSetIndex].overlapMetatarsusToTarsus = v; vMetatarsusTarsus.text = v.ToString("F2"); });
        sOverlapMultiplier.onValueChanged.AddListener(v => { legSets[currentLegSetIndex].minOverlapMultiplier = v; vOverlapMultiplier.text = v.ToString("F2"); });

        // default selection + hide advanced
        SelectLegSet(0);
        if (panelLegs2) panelLegs2.SetActive(false);
    }

    // -------------------- UI Flow --------------------
    public void ShowAdvancedLegsPanel()
    {
        if (!panelLegs || !panelLegs2) return;
        foreach (Transform child in panelLegs.transform)
            if (child.gameObject != panelLegs2) child.gameObject.SetActive(false);
        panelLegs2.SetActive(true);
    }

    public void ReturnToBasicLegsPanel()
    {
        if (!panelLegs || !panelLegs2) return;
        foreach (Transform child in panelLegs.transform)
            child.gameObject.SetActive(true);
        panelLegs2.SetActive(false);
    }

    // ---------------- Leg-set selection ----------------
    private void SelectLegSet(int index)
    {
        currentLegSetIndex = Mathf.Clamp(index, 0, 3);
        RefreshLegSetButtons();
        SyncLegOverlapUIFromSet();
    }

    private void RefreshLegSetButtons()
    {
        Paint(buttonSet1_L1R1, currentLegSetIndex == 0);
        Paint(buttonSet2_L2R2, currentLegSetIndex == 1);
        Paint(buttonSet3_L3R3, currentLegSetIndex == 2);
        Paint(buttonSet4_L4R4, currentLegSetIndex == 3);
    }

    private void Paint(Button b, bool active)
    {
        if (!b) return;
        var img = b.GetComponent<Image>();
        if (img) img.color = active ? activeLegSetColor : inactiveLegSetColor;
    }

    // -------------- Sync UI from set (no events) --------------
    private void SyncLegOverlapUIFromSet()
    {
        var set = legSets[currentLegSetIndex];

        sCoxaTrochanter.SetValueWithoutNotify(set.overlapCoxaToTrochanter); vCoxaTrochanter.text = set.overlapCoxaToTrochanter.ToString("F2");
        sTrochanterFemur.SetValueWithoutNotify(set.overlapTrochanterToFemur); vTrochanterFemur.text = set.overlapTrochanterToFemur.ToString("F2");
        sFemurPatella.SetValueWithoutNotify(set.overlapFemurToPatella); vFemurPatella.text = set.overlapFemurToPatella.ToString("F2");
        sPatellaTibia.SetValueWithoutNotify(set.overlapPatellaToTibia); vPatellaTibia.text = set.overlapPatellaToTibia.ToString("F2");
        sTibiaMetatarsus.SetValueWithoutNotify(set.overlapTibiaToMetatarsus); vTibiaMetatarsus.text = set.overlapTibiaToMetatarsus.ToString("F2");
        sMetatarsusTarsus.SetValueWithoutNotify(set.overlapMetatarsusToTarsus); vMetatarsusTarsus.text = set.overlapMetatarsusToTarsus.ToString("F2");
        sOverlapMultiplier.SetValueWithoutNotify(set.minOverlapMultiplier); vOverlapMultiplier.text = set.minOverlapMultiplier.ToString("F2");
    }

    // ---------------- Reset to defaults ----------------
    public void ResetCurrentLegSetToDefault()
    {
        var def = legSetsDefaults[currentLegSetIndex];
        var live = legSets[currentLegSetIndex];

        live.overlapCoxaToTrochanter = def.overlapCoxaToTrochanter;
        live.overlapTrochanterToFemur = def.overlapTrochanterToFemur;
        live.overlapFemurToPatella = def.overlapFemurToPatella;
        live.overlapPatellaToTibia = def.overlapPatellaToTibia;
        live.overlapTibiaToMetatarsus = def.overlapTibiaToMetatarsus;
        live.overlapMetatarsusToTarsus = def.overlapMetatarsusToTarsus;
        live.minOverlapMultiplier = def.minOverlapMultiplier;

        SyncLegOverlapUIFromSet();
    }

    // ---------------- Util ----------------
    private static GeneralScaler.JointOverlapSettings CopySet(GeneralScaler.JointOverlapSettings src)
    {
        var dst = new GeneralScaler.JointOverlapSettings();
        dst.overlapCoxaToTrochanter = src.overlapCoxaToTrochanter;
        dst.overlapTrochanterToFemur = src.overlapTrochanterToFemur;
        dst.overlapFemurToPatella = src.overlapFemurToPatella;
        dst.overlapPatellaToTibia = src.overlapPatellaToTibia;
        dst.overlapTibiaToMetatarsus = src.overlapTibiaToMetatarsus;
        dst.overlapMetatarsusToTarsus = src.overlapMetatarsusToTarsus;
        dst.minOverlapMultiplier = src.minOverlapMultiplier;
        dst.overlapExponent = src.overlapExponent; // unused in UI
        return dst;
    }
}
