using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProsomaScaler : MonoBehaviour
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


    void Start()
    {
        InitializeProsoma();
        SetupProsomaParts();
    }



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