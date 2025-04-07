using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegL1 : MonoBehaviour
{
    public Transform coxa;
    public Transform trochanter;

    //public Transform femur;
    //public Transform patella;
    //public Transform tibia;
    //public Transform metatarsus;
    //public Transform tarsus;

    


    public float coxaScaleX = 1f;
    public float coxaScaleY = 1f;
    public float coxaScaleZ = 1f;

    public float trochanterScaleX = 1f;
    public float trochanterScaleY = 1f;
    public float trochanterScaleZ = 1f;



    private Vector3 originalCoxaScale; 
    private Vector3 originalCoxaRotation;
    private Vector3 originalCoxaPosition;

    private Vector3 originalTrochanterScale;
    private Vector3 originalTrochanterRotation;
    private Vector3 originalTrochanterPosition;


    // Start is called before the first frame update
    void Start()
    {
        if (coxa == null)
        {
            Debug.LogError("Coxa object is not assigned in the inspector"); //slide the correct coxa to the leg
            return;
        }

        if (trochanter == null)
        {
            Debug.LogError("Trochanter object is not assigned in the inspector");
        }

        // Save the original properties of the coxa at the start
        originalCoxaScale = coxa.localScale;
        //originalCoxaRotation = coxa.localRotation


        // Save the original properties of the trochanter at the start
        originalTrochanterScale = trochanter.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (coxa != null)
        {
            coxa.localScale = new Vector3(
                originalCoxaScale.x * coxaScaleX,
                originalCoxaScale.y * coxaScaleY,
                originalCoxaScale.z * coxaScaleZ
            );
        }

        if (trochanter != null)
        {
            trochanter.localScale = new Vector3(
                originalTrochanterScale.x * trochanterScaleX,
                originalTrochanterScale.y * trochanterScaleY,
                originalTrochanterScale.z * trochanterScaleZ
                );
        }
    }
}
