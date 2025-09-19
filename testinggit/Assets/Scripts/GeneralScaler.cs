using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


/// <summary>
/// GeneralScaler is responsible for scaling and repositioning different body parts of the spider model,
/// including the prosoma, abdomen, spinnerets, and legs. It ensures that attached parts stay aligned and
/// properly overlap when the model is scaled.
/// </summary>

public class GeneralScaler : MonoBehaviour
{

    /// <summary>
    /// Defines a group of overlap settings saved by a label. Used for saving/loading multiple configurations.
    /// </summary>
    public class OverlapSet
    {
        public string label;
        public OverlapValue[] overlaps;
    }

    /// <summary>
    /// Stores an individual overlap value with a name.
    /// </summary>
    [System.Serializable]
    public class OverlapValue
    {
        public string name;
        public float value;
    }

    public List<OverlapSet> overlapSets = new List<OverlapSet>();


    //================= PROSOMA references =================//
    private Transform prosomaRoot; // Root node of prosoma
    private Transform prosoma; // Main prosoma body

    [Header("Prosoma Scaling Compensation")]
    public Vector3 prosomaOverlapCompensation = Vector3.zero; // Compensation values to avoid overlap after scaling

    private class ProsomaParts
    {
        public Transform part;
        public Vector3 originalOffsetFromPivot; // Distance from prosoma center at initialization
    }
    private List<ProsomaParts> prosomaParts = new List<ProsomaParts>();

    //prosomaparts
    // Prosoma external parts
    private Transform pedipalpLeft;
    private Transform pedipalpRight;
    private Transform cheliceraeLeft;
    private Transform cheliceraeRight;
    private Transform eyes;

    // Initial offsets of external parts from prosoma
    private Vector3 offsetPedipalpL;
    private Vector3 offsetPedipalpR;
    private Vector3 offsetCheliceraeL;
    private Vector3 offsetCheliceraeR;
    private Vector3 offsetEyes;


    // Initial rotations for external parts
    private Quaternion initialProsomaRotation;
    private Quaternion initialPedipalpLRot;
    private Quaternion initialPedipalpRRot;
    private Quaternion initialCheliceraeLRot;
    private Quaternion initialCheliceraeRRot;
    private Quaternion initialEyesRot;

    // Leg attachment points on prosoma
    private Transform[] legTransforms = new Transform[8];
    private Vector3[] legOffsets = new Vector3[8];
    private Quaternion[] initialLegRotations = new Quaternion[8];




    //================= ABDOMEN =================//
    private Transform abdomenRoot;
    private Transform abdomen;

    [Header("Absomen Scaling Compensation")]
    public Vector3 abdomenOverlapCompensation = Vector3.zero; // Compensation values to avoid overlap after scaling

    private class AbdomenParts
    {
        public Transform part;
        public Vector3 originalOffsetFromPivot;
    }
    private List<AbdomenParts> abdomenParts = new List<AbdomenParts>();

    //spinnerets
    private Vector3 spinneretLocalOffset; // Distance from abdomen center
    private Transform spinneretRoot;




    //================= LEG SETTINGS =================//

    /// <summary>
    /// Overlap values for each joint of the leg chain. Controls how much segments overlap when scaled.
    /// </summary>
    [System.Serializable]
    public class JointOverlapSettings
    {
        public float overlapCoxaToTrochanter = 0.1f;
        public float overlapTrochanterToFemur = 0.1f;
        public float overlapFemurToPatella = 0.1f;
        public float overlapPatellaToTibia = 0.1f;
        public float overlapTibiaToMetatarsus = 0.1f;
        public float overlapMetatarsusToTarsus = 0.1f;

        [Range(0f, 1f)]
        [Tooltip("Minimum overlap multiplier when the joint is fully bent.")]
        public float minOverlapMultiplier = 0.1f;
        public float overlapExponent = 1.5f;
    }

    [Header("Joint overlap settings for LegL1 and LegR1")]
    public JointOverlapSettings overlapSet1;

    [Header("Joint overlap settings for LegL2 and LegR2")]
    public JointOverlapSettings overlapSet2;

    [Header("Joint overlap settings for LegL3 and LegR3")]
    public JointOverlapSettings overlapSet3;

    [Header("Joint overlap settings for LegL4 and LegR4")]
    public JointOverlapSettings overlapSet4;


    /// <summary>
    /// Represents the full chain of a spider leg with references to each segment and its overlap settings.
    /// </summary>
    private class LegChain
    {
        public Transform coxaRoot, femurRoot, patellaRoot, metatarsusRoot, tarsusRoot, tipRoot;
        public Transform coxa, trochanter, femur, patella, tibia, metatarsus, tarsus;
        public JointOverlapSettings overlap;
    }

    private List<LegChain> allLegs = new List<LegChain>();
    private Dictionary<Transform, float> previousScales = new Dictionary<Transform, float>();



    //================= UNITY METHODS =================//

    void Start()
    {
        InitializeLegs();
        InitializeAbdomen();
        SetupAbdomenParts();
        InitializeProsoma();
        SetupProsomaParts();


        spinneretRoot = abdomenRoot.Find("spinneretRoot");
        if (abdomen != null && spinneretRoot != null)
        {
            spinneretLocalOffset = spinneretRoot.position - abdomen.position;
        }
    }

    
    void LateUpdate()
    {

        //=========Prosoma scaling =========//

        // Apply scaling compensation for prosoma
        Vector3 prosomaCompensatedScale = new Vector3(
           prosoma.localScale.x * (1 - prosomaOverlapCompensation.x),
           prosoma.localScale.y * (1 - prosomaOverlapCompensation.y),
           prosoma.localScale.z * (1 - prosomaOverlapCompensation.z)
       );

        // Update all prosoma child parts
        foreach (var part in prosomaParts)
        {
            Vector3 scaledOffset = new Vector3(
                part.originalOffsetFromPivot.x * prosomaCompensatedScale.x,
                part.originalOffsetFromPivot.y * prosomaCompensatedScale.y,
                part.originalOffsetFromPivot.z * prosomaCompensatedScale.z
            );

            part.part.position = prosoma.position + scaledOffset;
        }

        if (prosoma != null)
        {
            // Apply compensation and rotation to prosoma attachments (pedipalps, chelicerae, eyes, legs)
            Vector3 compScale = new Vector3(
                prosoma.localScale.x * (1 - prosomaOverlapCompensation.x),
                prosoma.localScale.y * (1 - prosomaOverlapCompensation.y),
                prosoma.localScale.z * (1 - prosomaOverlapCompensation.z)
            );

            Quaternion deltaRot = prosoma.rotation * Quaternion.Inverse(initialProsomaRotation);
            Vector3 pos = prosoma.position;

            if (pedipalpLeft != null)
            {
                Vector3 scaled = Vector3.Scale(offsetPedipalpL, compScale);
                pedipalpLeft.position = pos + prosoma.rotation * scaled;
                pedipalpLeft.rotation = deltaRot * initialPedipalpLRot;
            }

            if (pedipalpRight != null)
            {
                Vector3 scaled = Vector3.Scale(offsetPedipalpR, compScale);
                pedipalpRight.position = pos + prosoma.rotation * scaled;
                pedipalpRight.rotation = deltaRot * initialPedipalpRRot;
            }

            if (cheliceraeLeft != null)
            {
                Vector3 scaled = Vector3.Scale(offsetCheliceraeL, compScale);
                cheliceraeLeft.position = pos + prosoma.rotation * scaled;
                cheliceraeLeft.rotation = deltaRot * initialCheliceraeLRot;
            }

            if (cheliceraeRight != null)
            {
                Vector3 scaled = Vector3.Scale(offsetCheliceraeR, compScale);
                cheliceraeRight.position = pos + prosoma.rotation * scaled;
                cheliceraeRight.rotation = deltaRot * initialCheliceraeRRot;
            }

            if (eyes != null)
            {
                Vector3 scaled = Vector3.Scale(offsetEyes, compScale);
                eyes.position = pos + prosoma.rotation * scaled;
                eyes.rotation = deltaRot * initialEyesRot;
            }

            //Legs
            for (int i = 0; i < 8; i++)
            {
                if (legTransforms[i] == null) continue;

                Vector3 scaledOffset = Vector3.Scale(legOffsets[i], compScale);
                legTransforms[i].position = prosoma.position + prosoma.rotation * scaledOffset;

                Quaternion deltaRotation = prosoma.rotation * Quaternion.Inverse(initialProsomaRotation);
                legTransforms[i].rotation = deltaRotation * initialLegRotations[i];
            }

        }





        //====== Abdomen scaling and part updates======//
        Vector3 abdomenCompensatedScale = new Vector3(
            abdomen.localScale.x * (1 - abdomenOverlapCompensation.x),
            abdomen.localScale.y * (1 - abdomenOverlapCompensation.y),
            abdomen.localScale.z * (1 - abdomenOverlapCompensation.z)
        );

        foreach (var part in abdomenParts)
        {
            Vector3 scaledOffset = new Vector3(
                part.originalOffsetFromPivot.x * abdomenCompensatedScale.x,
                part.originalOffsetFromPivot.y * abdomenCompensatedScale.y,
                part.originalOffsetFromPivot.z * abdomenCompensatedScale.z
            );

            part.part.position = abdomen.position + scaledOffset;
        }

        // Sync spinneret position and rotation
        if (spinneretRoot != null)
        {
            Vector3 scaledSpinneretOffset = new Vector3(
                spinneretLocalOffset.x * abdomenCompensatedScale.x,
                spinneretLocalOffset.y * abdomenCompensatedScale.y,
                spinneretLocalOffset.z * abdomenCompensatedScale.z
            );

            spinneretRoot.position = abdomen.position + abdomen.rotation * scaledSpinneretOffset;
            spinneretRoot.rotation = abdomen.rotation;
        }




        //Update all legs
        foreach (var leg in allLegs)
        {
            var o = leg.overlap;

            //PositionJoint(leg.coxa, leg.coxaRoot, leg.femurRoot, o.overlapCoxaToTrochanter, o); // will be used together with trochanter
            PositionJoint(leg.trochanter, leg.coxaRoot, leg.femurRoot, o.overlapTrochanterToFemur, o);
            PositionJoint(leg.femur, leg.femurRoot, leg.patellaRoot, o.overlapFemurToPatella, o);
            //PositionJoint(leg.patella, leg.patellaRoot, leg.tibia, o.overlapPatellaToTibia, o); //will be used together with tibia
            PositionJoint(leg.tibia, leg.patellaRoot, leg.metatarsusRoot, o.overlapTibiaToMetatarsus, o);
            PositionJoint(leg.metatarsus, leg.metatarsusRoot, leg.tarsusRoot, o.overlapMetatarsusToTarsus, o);
            PositionJoint(leg.tarsus, leg.tarsusRoot, leg.tipRoot, 0f, o); // No overlap past tipRoot
        }
    }


    /// <summary>
    /// Updates a joint’s position based on scaling difference and overlap values.
    /// </summary>
    private void PositionJoint(Transform mesh, Transform currentRoot, Transform nextRoot, float baseOverlap, JointOverlapSettings settings)
    {

        float currentScale = mesh.localScale.y;

        // Prevent zero or negative scales from breaking calculations
        float safeScale = Mathf.Max(currentScale, 0.0001f);
        float previousScale = previousScales.TryGetValue(mesh, out float oldScale) ? oldScale : safeScale;

        float delta = safeScale - previousScale;
        float overlap = AdjustedOverlap(baseOverlap, settings.minOverlapMultiplier, safeScale, settings.overlapExponent);

        // Calculate offset in local right direction
        Vector3 offset = currentRoot.TransformDirection(Vector3.right) * (delta + overlap);

        // Skip if offset is invalid
        if (float.IsNaN(offset.x) || float.IsInfinity(offset.x) ||
            float.IsNaN(offset.y) || float.IsInfinity(offset.y) ||
            float.IsNaN(offset.z) || float.IsInfinity(offset.z))
        {
            Debug.LogWarning($"Invalid offset detected for {mesh.name}, skipping position update. Offset: {offset}");
            return;
        }

        nextRoot.position = currentRoot.position - offset;

        previousScales[mesh] = safeScale;
    }


    /// <summary>
    /// Applies nonlinear adjustment to overlap based on scale and exponent.
    /// </summary>
    private float AdjustedOverlap(float baseOverlap, float minMultiplier, float currentScale, float exponent)
    {
        float nonlinearScale = Mathf.Pow(currentScale, exponent);
        return baseOverlap * minMultiplier * nonlinearScale;
    }

    /// <summary>
    /// Finds all legs under prosomaRoot and builds their bone chain.
    /// </summary>
    private void InitializeLegs()
    {
        allLegs.Clear();

        Transform prosomaRoot = null;

        // Search for prosomaRoot transform
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "prosomaRoot")
            {
                prosomaRoot = t;
                Debug.LogError(" 'prosomaRoot' found under " + transform.name);
                break;
            }
        }

        if (prosomaRoot == null)
        {
            Debug.LogError("Could not find 'prosomaRoot' anywhere under " + transform.name);
            return;
        }


        // Build leg chain for each Leg child
        foreach (Transform child in prosomaRoot)
        {
            if (!child.name.StartsWith("Leg")) continue;

            Transform coxaRoot = child.Find("coxaRoot");
            if (coxaRoot == null) continue;

            var chain = new LegChain();
            chain.coxaRoot = coxaRoot;

            chain.coxa = coxaRoot.Find("coxa");
            chain.trochanter = coxaRoot.Find("trochanter");
            chain.femurRoot = coxaRoot.Find("femurRoot");

            chain.femur = chain.femurRoot?.Find("femur");
            chain.patellaRoot = chain.femurRoot?.Find("patellaRoot");

            chain.patella = chain.patellaRoot?.Find("patella");
            chain.tibia = chain.patellaRoot?.Find("tibia");
            chain.metatarsusRoot = chain.patellaRoot?.Find("metatarsusRoot");

            chain.metatarsus = chain.metatarsusRoot?.Find("metatarsus");
            chain.tarsusRoot = chain.metatarsusRoot?.Find("tarsusRoot");

            chain.tarsus = chain.tarsusRoot?.Find("tarsus");
            chain.tipRoot = chain.tarsusRoot?.Find("tipRoot");

            // Match overlap set to leg index
            int legIndex = -1;
            if (child.name.Length >= 5 && int.TryParse(child.name.Substring(4, 1), out legIndex))
            {
                switch (legIndex)
                {
                    case 1: chain.overlap = overlapSet1; break;
                    case 2: chain.overlap = overlapSet2; break;
                    case 3: chain.overlap = overlapSet3; break;
                    case 4: chain.overlap = overlapSet4; break;
                    default: Debug.LogWarning($"Unknown leg index in name: {child.name}"); break;
                }
            }

            allLegs.Add(chain);
        }

    }


    /// <summary>
    /// Finds abdomen root and abdomen transforms by name.
    /// </summary>
    private void InitializeAbdomen() 
    {
        // Automatically find the abdomenRoot and abdomen by name
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allChildren)
        {
            if (t.name == "abdomenRoot")
                abdomenRoot = t;
            else if (t.name == "abdomen")
                abdomen = t;
        }

        if (abdomenRoot == null || abdomen == null)
        {
            Debug.LogError("AbdomenScaler: Required transforms not found by name.");
        }
    }

    /// <summary>
    /// Finds prosoma root and prosoma transforms by name.
    /// </summary>
    private void InitializeProsoma()
    {
        // Automatically find the prosomaRoot and prosoma by name
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allChildren)
        {
            if (t.name == "prosomaRoot")
                prosomaRoot = t;
            else if (t.name == "prosoma")
                prosoma = t;
        }

        if (prosomaRoot == null || prosoma == null)
        {
            Debug.LogError("ProsomaScaler: Required transforms not found by name.");
        }
    }


    /// <summary>
    /// Records abdomen child parts and their initial offsets.
    /// </summary>
    private void SetupAbdomenParts()
    {
        if (abdomenRoot == null || abdomen == null)
            return;

        abdomenParts.Clear(); // optional if re-initializing

        foreach (Transform child in abdomenRoot)
        {
            if (child == abdomen)
                continue;

            Vector3 offset = child.position - abdomen.position;

            abdomenParts.Add(new AbdomenParts
            {
                part = child,
                originalOffsetFromPivot = offset
            });
        }
    }


    /// <summary>
    /// Records prosoma child parts and their initial offsets.
    /// </summary>
    private void SetupProsomaParts()
    {
        if (prosomaRoot == null || prosoma == null)
            return;

        prosomaParts.Clear();

        foreach (Transform child in prosomaRoot)
        {
            if (child == prosoma)
                continue;

            Vector3 offset = child.position - prosoma.position;

            prosomaParts.Add(new ProsomaParts
            {
                part = child,
                originalOffsetFromPivot = offset
            });
        }
    }


    /// <summary>
    /// Sets up prosoma attachments (pedipalps, chelicerae, eyes) with initial offsets and rotations.
    /// </summary>
    public void SetupProsomaAttachments(
    Transform pedipalpL, Transform pedipalpR,
    Transform cheliceraeL, Transform cheliceraeR,
    Transform eyes,
    Vector3 offsetL, Vector3 offsetR,
    Vector3 offsetCL, Vector3 offsetCR,
    Vector3 offsetE,
    Quaternion prosomaRot,
    Quaternion rotL, Quaternion rotR,
    Quaternion rotCL, Quaternion rotCR,
    Quaternion rotE
    )
    {
        pedipalpLeft = pedipalpL;
        pedipalpRight = pedipalpR;
        cheliceraeLeft = cheliceraeL;
        cheliceraeRight = cheliceraeR;
        this.eyes = eyes;

        offsetPedipalpL = offsetL;
        offsetPedipalpR = offsetR;
        offsetCheliceraeL = offsetCL;
        offsetCheliceraeR = offsetCR;
        offsetEyes = offsetE;

        initialProsomaRotation = prosomaRot;
        initialPedipalpLRot = rotL;
        initialPedipalpRRot = rotR;
        initialCheliceraeLRot = rotCL;
        initialCheliceraeRRot = rotCR;
        initialEyesRot = rotE;
    }


    // sets up leg attachment points with initial offsets and rotations
    public void SetupLegAttachments(
    Transform[] legs,
    Vector3[] offsets,
    Quaternion[] rotations
    )
    {
        for (int i = 0; i < 8; i++)
        {
            legTransforms[i] = legs[i];
            legOffsets[i] = offsets[i];
            initialLegRotations[i] = rotations[i];
        }
    }


}
