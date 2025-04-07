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

    private class LegData
    {
        public Transform leg;
        public Vector3 originalOffsetFromPivot;
    }

    private List<LegData> legs = new List<LegData>();

    void Start()
    {
        foreach (Transform child in prosomaRoot)
        {
            if (child == prosoma)
                continue;

            Vector3 offset = child.position - prosoma.position;

            legs.Add(new LegData
            {
                leg = child,
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

        foreach (var leg in legs)
        {
            Vector3 scaledOffset = new Vector3(
                leg.originalOffsetFromPivot.x * compensatedScale.x,
                leg.originalOffsetFromPivot.y * compensatedScale.y,
                leg.originalOffsetFromPivot.z * compensatedScale.z
            );

            leg.leg.position = prosoma.position + scaledOffset;
        }
    }
}
