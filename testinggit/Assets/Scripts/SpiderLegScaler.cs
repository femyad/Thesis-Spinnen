using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[ExecuteAlways]
public class SpiderLegScaler : MonoBehaviour
{
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
        public Transform coxaRoot, trochanterRoot, femurRoot, patellaRoot, tibiaRoot, metatarsusRoot, tarsusRoot;
        public Transform coxa, trochanter, femur, patella, tibia, metatarsus, tarsus;
        public JointOverlapSettings overlap;
    }

    private List<LegChain> allLegs = new List<LegChain>();
    private Dictionary<Transform, float> previousScales = new Dictionary<Transform, float>();


    void Start()
    {
        Debug.Log("SpiderLegScaler Start called");
        InitializeLegs();
       
    }
   
    //[ContextMenu("Update Leg Positions")]
    void LateUpdate()
    {
       

        if (allLegs.Count == 0)
        {
            Debug.LogWarning("No legs initialized!");
            return;
            }


        foreach (var leg in allLegs)
        {
            Debug.Log($"Updating leg with overlap set: {leg.overlap}");
            var o = leg.overlap;

            PositionJoint(leg.coxa, leg.coxaRoot, leg.trochanterRoot, o.overlapCoxaToTrochanter, o);
            PositionJoint(leg.trochanter, leg.trochanterRoot, leg.femurRoot, o.overlapTrochanterToFemur, o);
            PositionJoint(leg.femur, leg.femurRoot, leg.patellaRoot, o.overlapFemurToPatella, o);
            PositionJoint(leg.patella, leg.patellaRoot, leg.tibiaRoot, o.overlapPatellaToTibia, o);
            PositionJoint(leg.tibia, leg.tibiaRoot, leg.metatarsusRoot, o.overlapTibiaToMetatarsus, o);
            PositionJoint(leg.metatarsus, leg.metatarsusRoot, leg.tarsusRoot, o.overlapMetatarsusToTarsus, o);
        }
    }


    private void PositionJoint(Transform mesh, Transform currentRoot, Transform nextRoot, float baseOverlap, JointOverlapSettings settings)
    {
        /*float currentScale = mesh.localScale.y;
        float previousScale = previousScales.TryGetValue(mesh, out float oldScale) ? oldScale : currentScale;

        float delta = currentScale - previousScale;
        float overlap = AdjustedOverlap(baseOverlap, settings.minOverlapMultiplier, currentScale, settings.overlapExponent);

        Debug.Log($"Positioning joint {mesh.name}: currentScale={currentScale}, delta={delta}, overlap={overlap}");


        nextRoot.position = currentRoot.position - currentRoot.TransformDirection(Vector3.right) * (delta + overlap);

        previousScales[mesh] = currentScale;*/

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
        Debug.Log("Initializing Legs...");
        allLegs.Clear();

        Transform prosomaRoot = transform.Find("prosomaRoot");
        if (prosomaRoot == null)
        {
            Debug.LogError("Could not find 'prosomaRoot' under " + transform.name);
            return;
        }
        int legCount = 0;

        foreach (Transform child in prosomaRoot)
        {
            if (!child.name.StartsWith("Leg")) continue;

            Transform coxaRoot = child.Find("coxaRoot");
            if (coxaRoot == null) continue;

            var chain = new LegChain();

            chain.coxaRoot = coxaRoot;
            chain.coxa = coxaRoot.Find("coxa");
            chain.trochanterRoot = coxaRoot.Find("trochanterRoot");

            chain.trochanter = chain.trochanterRoot.Find("trochanter");
            chain.femurRoot = chain.trochanterRoot.Find("femurRoot");

            chain.femur = chain.femurRoot.Find("femur");
            chain.patellaRoot = chain.femurRoot.Find("patellaRoot");

            chain.patella = chain.patellaRoot.Find("patella");
            chain.tibiaRoot = chain.patellaRoot.Find("tibiaRoot");

            chain.tibia = chain.tibiaRoot.Find("tibia");
            chain.metatarsusRoot = chain.tibiaRoot.Find("metatarsusRoot");

            chain.metatarsus = chain.metatarsusRoot.Find("metatarsus");
            chain.tarsusRoot = chain.metatarsusRoot.Find("tarsusRoot");

            chain.tarsus = chain.tarsusRoot.Find("tarsus");

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
            legCount++;
        }
        Debug.Log($"Initialized {legCount} legs.");
    }

}