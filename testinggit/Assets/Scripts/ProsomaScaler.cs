using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProsomaScaler : MonoBehaviour
{
    [Header("Prosoma references")]
    public Transform prosomaRoot;
    public Transform prosoma;

    [Header("Scaling Shrinkage Compensation")]
    public Vector3 overlapCompensation = Vector3.zero;

    private class PartData
    {
        public Transform part;
        public Vector3 originalOffsetFromPivot;
    }

    private List<PartData> parts = new List<PartData>();

    void Start()
    {
        foreach (Transform child in prosomaRoot)
        {
            if (child == prosoma)
                continue;

            Vector3 offset = child.position - prosoma.position;

            parts.Add(new PartData
            {
                part = child,
                originalOffsetFromPivot = offset
            });
        }
    }

    void LateUpdate()
    {
        Vector3 compensatedScale = new Vector3(
            prosoma.localScale.x * (1 - overlapCompensation.x),
            prosoma.localScale.y * (1 - overlapCompensation.y),
            prosoma.localScale.z * (1 - overlapCompensation.z)
        );

        foreach (var part in parts)
        {
            Vector3 scaledOffset = new Vector3(
                part.originalOffsetFromPivot.x * compensatedScale.x,
                part.originalOffsetFromPivot.y * compensatedScale.y,
                part.originalOffsetFromPivot.z * compensatedScale.z
            );

            part.part.position = prosoma.position + scaledOffset;
        }
    }
}