

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbdomenScaler : MonoBehaviour
{
    
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

    void Awake()
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

    void Start()
    {
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

    void LateUpdate()
    {
        Vector3 compensatedScale = new Vector3(
            abdomen.localScale.x * (1 - abdomenOverlapCompensation.x),
            abdomen.localScale.y * (1 - abdomenOverlapCompensation.y),
            abdomen.localScale.z * (1 - abdomenOverlapCompensation.z)
        );

        foreach (var part in abdomenParts)
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

/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbdomenScaler : MonoBehaviour
{
    [Header("Abdomen references")]
    public Transform abdomenRoot;
    public Transform abdomen;

    [Header("Scaling Compensation")]
    public Vector3 abdomenOverlapCompensation = Vector3.zero;

    private class AbdomenParts
    {
        public Transform part;
        public Vector3 originalOffsetFromPivot;
    }

    private List<AbdomenParts> abdomenParts = new List<AbdomenParts>();

    void Start()
    {
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

    void LateUpdate()
    {
        Vector3 compensatedScale = new Vector3(
            abdomen.localScale.x * (1 - abdomenOverlapCompensation.x),
            abdomen.localScale.y * (1 - abdomenOverlapCompensation.y),
            abdomen.localScale.z * (1 - abdomenOverlapCompensation.z)
        );

        foreach (var part in abdomenParts)
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
*/

