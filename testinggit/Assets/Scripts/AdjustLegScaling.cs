using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustLegScaling : MonoBehaviour
{
    private float initialLength;  // Original length of the segment
    public Transform nextSegmentRoot; // Assign "trochanterRoot" in Inspector

    void Start()
    {
        if (nextSegmentRoot == null) return;

        // Get the original world-space length of this segment
        initialLength = GetWorldLength();
    }

    void Update()
    {
        if (nextSegmentRoot == null) return;

        float newLength = GetWorldLength();

        if (Mathf.Abs(newLength - initialLength) > 0.001f) // Check if length changed
        {
            AdjustPosition(newLength);
            initialLength = newLength;
        }
    }

    void AdjustPosition(float newLength)
    {
        if (nextSegmentRoot == null) return;

        // Get the leg direction (local up is the correct axis)
        Vector3 legDirection = transform.up.normalized;

        // Calculate how much the length has changed
        float lengthDifference = newLength - initialLength;

        // Move the next segment outward by that exact difference
        nextSegmentRoot.position += legDirection * lengthDifference;
    }

    float GetWorldLength()
    {
        // Since scale represents length, get the actual world-space size
        return transform.lossyScale.y;
    }
}
