using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LegPair
{
    public Transform[] leftLegSegments = new Transform[7];  // Segments for the left leg
    public Transform[] rightLegSegments = new Transform[7]; // Segments for the right leg

    [Range(0.1f, 10f)] public float[] segmentLengths = new float[7];  // Lengths for each segment
    [Range(0.1f, 10f)] public float[] segmentDiameters = new float[7]; // Diameters for each segment
}

[ExecuteAlways]
public class spiderParameters : MonoBehaviour
{
    public Transform body;       // Reference to the body object
    public Transform abdomen;    // Reference to the abdomen object
    public List<LegPair> legPairs = new List<LegPair>(4); // List of 4 leg pairs

    [Range(0.1f, 10f)] public float bodyScale = 1f;       // Scale factor for the body
    [Range(0.1f, 10f)] public float abdomenScale = 1f;    // Scale factor for the abdomen

    private Vector3[,] originalSegmentScales; // To store original scales for resetting

        void Start()
    {
        foreach (LegPair pair in legPairs)
        {
            // Initialize segment lengths and diameters to match the segment count
            int segmentCount = pair.leftLegSegments.Length;

            if (pair.segmentLengths == null || pair.segmentLengths.Length != segmentCount)
            {
                pair.segmentLengths = new float[segmentCount];
                for (int i = 0; i < segmentCount; i++)
                    pair.segmentLengths[i] = pair.leftLegSegments[i].localScale.y; // Default length
            }

            if (pair.segmentDiameters == null || pair.segmentDiameters.Length != segmentCount)
            {
                pair.segmentDiameters = new float[segmentCount];
                for (int i = 0; i < segmentCount; i++)
                    pair.segmentDiameters[i] = pair.leftLegSegments[i].localScale.x; // Default diameter
            }
        }
    }


        void Update()
    {
        foreach (LegPair pair in legPairs)
        {
            for (int i = 0; i < pair.leftLegSegments.Length; i++)
            {
                // Safeguard against potential array mismatches
                if (i < pair.segmentLengths.Length && i < pair.segmentDiameters.Length)
                {
                    // Apply length and diameter to left leg segment
                    Transform leftSegment = pair.leftLegSegments[i];
                    Vector3 leftScale = leftSegment.localScale;
                    leftScale.x = pair.segmentLengths[i]; // Length
                    leftScale.y = leftScale.z = pair.segmentDiameters[i]; // Diameter
                    leftSegment.localScale = leftScale;

                    // Apply length and diameter to right leg segment
                    Transform rightSegment = pair.rightLegSegments[i];
                    Vector3 rightScale = rightSegment.localScale;
                    rightScale.x = pair.segmentLengths[i]; // Length
                    rightScale.y = rightScale.z = pair.segmentDiameters[i]; // Diameter
                    rightSegment.localScale = rightScale;
                }
            }
        }
    }

}
