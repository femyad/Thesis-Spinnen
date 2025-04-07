using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderLegScaler : MonoBehaviour
{

    /*
    //the scaled meshes
    [Header("Leg Segments")]
    public Transform coxa;
    public Transform trochanter;
    public Transform femur;
    public Transform patella;
    public Transform tibia;
    public Transform metatarsus;
    public Transform tarsus;

    //the roots, empty objects for positioning the scaled meshes
    [Header("Root objects")]
    public Transform trochanterRoot;
    public Transform femurRoot;
    public Transform patellaRoot;
    public Transform tibiaRoot;
    public Transform metatarsusRoot;
    public Transform tarsusRoot;
    */

    [Header("Leg Root GameObjects (e.g. LegL1, LegL2...)")]
    public List<Transform> legRoots;

    // the amount of overlap between the leg segments, to compensate for joints
    [Header("Joint Overlap Settings")]
    public float overlapCoxaToTrochanter = 0.1f;
    public float overlapTrochanterToFemur = 0.1f;
    public float overlapFemurToPatella = 0.1f;
    public float overlapPatellaToTibia = 0.1f;
    public float overlapTibiaToMetatarsus = 0.1f;
    public float overlapMetatarsusToTarsus = 0.1f;

    private class LegChain
    {
        public Transform coxa, trochanter, femur, patella, tibia, metatarsus, tarsus;
        public Transform trochanterRoot, femurRoot, patellaRoot, tibiaRoot, metatarsusRoot, tarsusRoot;
    }


    private List<LegChain> allLegs = new List<LegChain>();


    void Start()
    {
        foreach (Transform leg in legRoots)
        {
            var chain = new LegChain();

            Transform coxaRoot = leg.Find("coxaRoot");
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

            allLegs.Add(chain);
        }

    }

    void LateUpdate()
    {

        /*
        // Direction is local Y+ (the leg extends this way)
        // the use the scale.y as the literal length
        // then position the next root at the end of the current bone
        Vector3 directionCoxa = coxa.TransformDirection(Vector3.up);
        float scaledLengthCoxa = coxa.localScale.y;
        trochanterRoot.position = coxa.position + directionCoxa * (scaledLengthCoxa - overlapCoxaToTrochanter);

        Vector3 directionTrochanter = trochanter.TransformDirection(Vector3.up);
        float scaledLengthTrochanter = trochanter.localScale.y;
        femurRoot.position = trochanterRoot.position + directionTrochanter * (scaledLengthTrochanter - overlapTrochanterToFemur);

        Vector3 directionFemur = femur.TransformDirection(Vector3.up);
        float scaledLengthFemur = femur.localScale.y;
        patellaRoot.position = femurRoot.position + directionFemur * (scaledLengthFemur - overlapFemurToPatella);

        Vector3 directionPatella = patella.TransformDirection(Vector3.up);
        float scaledLengthPatella = patella.localScale.y;
        tibiaRoot.position = patellaRoot.position + directionPatella * (scaledLengthPatella - overlapPatellaToTibia);

        Vector3 directionTibia = tibia.TransformDirection(Vector3.up);
        float scaledLengthTibia = tibia.localScale.y;
        metatarsusRoot.position = tibiaRoot.position + directionTibia * (scaledLengthTibia - overlapTibiaToMetatarsus);

        Vector3 directionMetatarsus = metatarsus.TransformDirection(Vector3.up);
        float scaledLengthMetatarsus = metatarsus.localScale.y;
        tarsusRoot.position = metatarsusRoot.position + directionMetatarsus * (scaledLengthMetatarsus - overlapMetatarsusToTarsus);

        Debug.DrawLine(coxa.position, trochanterRoot.position, Color.red);
        Debug.DrawLine(trochanter.position, femurRoot.position, Color.green);

        */

        foreach (var leg in allLegs)
        {
            leg.trochanterRoot.position = leg.coxa.position + leg.coxa.TransformDirection(Vector3.up) * (leg.coxa.localScale.y - overlapCoxaToTrochanter);
            leg.femurRoot.position = leg.trochanterRoot.position + leg.trochanter.TransformDirection(Vector3.up) * (leg.trochanter.localScale.y - overlapTrochanterToFemur);
            leg.patellaRoot.position = leg.femurRoot.position + leg.femur.TransformDirection(Vector3.up) * (leg.femur.localScale.y - overlapFemurToPatella);
            leg.tibiaRoot.position = leg.patellaRoot.position + leg.patella.TransformDirection(Vector3.up) * (leg.patella.localScale.y - overlapPatellaToTibia);
            leg.metatarsusRoot.position = leg.tibiaRoot.position + leg.tibia.TransformDirection(Vector3.up) * (leg.tibia.localScale.y - overlapTibiaToMetatarsus);
            leg.tarsusRoot.position = leg.metatarsusRoot.position + leg.metatarsus.TransformDirection(Vector3.up) * (leg.metatarsus.localScale.y - overlapMetatarsusToTarsus);
        }



    }

}
