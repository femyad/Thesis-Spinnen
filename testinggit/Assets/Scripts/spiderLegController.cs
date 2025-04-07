using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiderLegController : MonoBehaviour
{
    [System.Serializable]
    public class Leg
    {
        public Transform IKTarget;      // The IK target for the leg
        public Transform bodyTarget;    // The body-relative target
        public Vector3 lockedPosition;  // Where the foot is "planted"
        public bool isMoving = false;
    }

    public List<Leg> legs = new List<Leg>();
    public LayerMask groundLayer;
    public float stepHeight = 1.0f;         // Height of the step arc
    public float stepSpeed = 5.0f;          // Speed of movement
    public float stepDistanceThreshold = 1.5f;  // Distance at which the leg moves
    private Transform bodyTransform;

    private void Start()
    {
        bodyTransform = transform;

        foreach (var leg in legs)
        {
            if (leg.bodyTarget != null)
            {
                leg.IKTarget.position = leg.bodyTarget.position;
                leg.lockedPosition = leg.bodyTarget.position;
            }
        }
    }

    private void Update()
    {
        foreach (var leg in legs)
        {
            if (!leg.isMoving && leg.bodyTarget != null)
            {
                float distance = Vector3.Distance(leg.IKTarget.position, leg.bodyTarget.position);
                
                if (distance > stepDistanceThreshold)
                {
                    StartCoroutine(MoveLegToBodyTarget(leg));
                }
            }
        }
    }

    private IEnumerator MoveLegToBodyTarget(Leg leg)
    {
        if (leg.isMoving) yield break; // Prevent multiple movements
        leg.isMoving = true;

        Vector3 startPosition = leg.IKTarget.position;
        Vector3 targetPosition = GetGroundPosition(leg.bodyTarget.position);
        leg.lockedPosition = targetPosition;

        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * stepSpeed;
            float heightOffset = Mathf.Sin(time * Mathf.PI) * stepHeight;
            leg.IKTarget.position = Vector3.Lerp(startPosition, targetPosition, time) + Vector3.up * heightOffset;
            yield return null;
        }

        leg.IKTarget.position = targetPosition;
        leg.lockedPosition = targetPosition;
        leg.isMoving = false;
    }

    private Vector3 GetGroundPosition(Vector3 position)
    {
        RaycastHit hit;
        Vector3 rayOrigin = position + Vector3.up * 50f; // High starting point for raycast
        Vector3 rayDirection = Vector3.down; 

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, 100f, groundLayer))
        {
            return hit.point; 
        }

        return position; // If no ground detected, return original position
    }
}
