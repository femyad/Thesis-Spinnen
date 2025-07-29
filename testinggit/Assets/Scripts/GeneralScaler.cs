using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralScaler : MonoBehaviour
{
    //Prosoma references
    private Transform prosomaRoot;
    private Transform prosoma;

    [Header("Prosoma Scaling Compensation")]
    public Vector3 prosomaOverlapCompensation = Vector3.zero;

    private class ProsomaParts
    {
        public Transform part;
        public Vector3 originalOffsetFromPivot;
    }
    private List<ProsomaParts> prosomaParts = new List<ProsomaParts>();



    //Abdomen references
    private Transform abdomenRoot;
    private Transform abdomen;

    [Header("Absomen Scaling Compensation")]
    public Vector3 abdomenOverlapCompensation = Vector3.zero;

    private class AbdomenParts
    {
        public Transform part;
        public Vector3 originalOffsetFromPivot;
    }
    private List<AbdomenParts> abdomenParts = new List<AbdomenParts>();





    //Leg references
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

    private class LegChain
    {
        public Transform coxaRoot, femurRoot, patellaRoot, metatarsusRoot, tarsusRoot, tipRoot;
        public Transform coxa, trochanter, femur, patella, tibia, metatarsus, tarsus;
        public JointOverlapSettings overlap;
    }

    private List<LegChain> allLegs = new List<LegChain>();
    private Dictionary<Transform, float> previousScales = new Dictionary<Transform, float>();


    void Start()
    {
        InitializeLegs();
        InitializeAbdomen();
        SetupAbdomenParts();
        InitializeProsoma();
        SetupProsomaParts();

        /*if (Application.isPlaying)
        {
            UpdateLegPositions();
        }*/
    }

    //[ContextMenu("Update Leg Positions")]
    void LateUpdate()
    {

        //Prosoma
        Vector3 prosomaCompensatedScale = new Vector3(
           prosoma.localScale.x * (1 - prosomaOverlapCompensation.x),
           prosoma.localScale.y * (1 - prosomaOverlapCompensation.y),
           prosoma.localScale.z * (1 - prosomaOverlapCompensation.z)
       );

        foreach (var part in prosomaParts)
        {
            Vector3 scaledOffset = new Vector3(
                part.originalOffsetFromPivot.x * prosomaCompensatedScale.x,
                part.originalOffsetFromPivot.y * prosomaCompensatedScale.y,
                part.originalOffsetFromPivot.z * prosomaCompensatedScale.z
            );

            part.part.position = prosoma.position + scaledOffset;
        }


        //Abdomen
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




        //Legs
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


    private void PositionJoint(Transform mesh, Transform currentRoot, Transform nextRoot, float baseOverlap, JointOverlapSettings settings)
    {

        float currentScale = mesh.localScale.y;

        // Prevent zero or negative scales from breaking calculations
        float safeScale = Mathf.Max(currentScale, 0.0001f);
        float previousScale = previousScales.TryGetValue(mesh, out float oldScale) ? oldScale : safeScale;

        float delta = safeScale - previousScale;
        float overlap = AdjustedOverlap(baseOverlap, settings.minOverlapMultiplier, safeScale, settings.overlapExponent);

        Vector3 offset = currentRoot.TransformDirection(Vector3.right) * (delta + overlap);

        // Validate offset to avoid NaN or Infinity
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

    private float AdjustedOverlap(float baseOverlap, float minMultiplier, float currentScale, float exponent)
    {
        float nonlinearScale = Mathf.Pow(currentScale, exponent);
        return baseOverlap * minMultiplier * nonlinearScale;
    }


    private void InitializeLegs()
    {
        allLegs.Clear();

        /*Transform prosomaRoot = transform.Find("prosomaRoot");
        if (prosomaRoot == null)
        {
            Debug.LogError("Could not find 'prosomaRoot' under " + transform.name);
            return;
        }*/

        Transform prosomaRoot = null;

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
}
