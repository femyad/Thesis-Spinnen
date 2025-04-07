using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbdomenScaler : MonoBehaviour
{
    [Header("Abdomen references")]
    public Transform abdomenRoot;
    public Transform abdomen;

    [Header("Scaling Compensation")]
    public Vector3 overlapCompensation = Vector3.zero;

    private class PartData
    {
        public Transform part;
        public Vector3 originalOffsetFromPivot;
    }

    private List<PartData> parts = new List<PartData>();

    void Start()
    {
        foreach (Transform child in abdomenRoot)
        {
            if (child == abdomen)
                continue;

            Vector3 offset = child.position - abdomen.position;

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
            abdomen.localScale.x * (1 - overlapCompensation.x),
            abdomen.localScale.y * (1 - overlapCompensation.y),
            abdomen.localScale.z * (1 - overlapCompensation.z)
        );

        foreach (var part in parts)
        {
            Vector3 scaledOffset = new Vector3(
                part.originalOffsetFromPivot.x * compensatedScale.x,
                part.originalOffsetFromPivot.y * compensatedScale.y,
                part.originalOffsetFromPivot.z * compensatedScale.z
            );

            part.part.position = abdomen.position + scaledOffset;
        }
    }
}
