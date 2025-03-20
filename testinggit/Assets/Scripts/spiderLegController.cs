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
        public Vector3 lockedPosition;  // Initial locked position
        public bool isMoving = false;
    }

    public List<Leg> legs = new List<Leg>();
    public LayerMask groundLayer;
    public float stepHeight = 1.0f;
    public float stepSpeed = 5.0f;
    public float groundCheckDistance = 50f;
    public float stepDistanceThreshold = 1.5f;
    public float legRadius = 1.5f;
    public float stepDelay = 0.2f;
    
    public int[][] stepGroups = new int[][]
    {
        new int[] { 0, 2 },
        new int[] { 1, 3 },
        new int[] { 4, 6 },
        new int[] { 5, 7 }
    };

    private TranslateMovement movementScript;
    private Transform bodyTransform;
    private int currentStepGroup = 0;
    private bool isLegPairMoving = false;

    private void Start()
    {
        movementScript = GetComponent<TranslateMovement>();
        bodyTransform = transform;

        foreach (var leg in legs)
        {
            if (leg.bodyTarget != null)
            {
                // Start IKTarget at the body target position
                leg.IKTarget.position = leg.bodyTarget.position;
                leg.lockedPosition = leg.bodyTarget.position; // HOE ECHT LOCKEN?
            }

            Debug.Log($"[INIT] Leg {leg.IKTarget.name} starts at {leg.IKTarget.position}");

            // Ensure the bodyTarget stays parented correctly
            if (leg.bodyTarget != null && leg.bodyTarget.parent != bodyTransform)
            {
                leg.bodyTarget.SetParent(bodyTransform, true); // Keep world position
            }
        }
    }



    private void Update()
    {
        if (movementScript == null || isLegPairMoving) return;
        float speed = Mathf.Abs(movementScript.GetCurrentSpeed());
        
        if (speed > 0.1f)
        {
            CheckAndMoveLegs();
        }
        
        //  IK targets follow their body targets 
        foreach (var leg in legs)
        {
            if (!leg.isMoving && leg.bodyTarget != null)
            {
                Vector3 worldBodyTarget = leg.bodyTarget.position;
                if (Vector3.Distance(leg.IKTarget.position, worldBodyTarget) > stepDistanceThreshold)
                {
                    StartCoroutine(MoveLegToBodyTarget(leg, worldBodyTarget));
                }
            }
        }
    }

    private void CheckAndMoveLegs()
    {
        int[] legPair = stepGroups[currentStepGroup];
        List<Leg> legsToMove = new List<Leg>();

        foreach (int legIndex in legPair)
        {
            Leg leg = legs[legIndex];
            if (!leg.isMoving && Vector3.Distance(leg.lockedPosition, GetLegTargetPosition(leg)) > stepDistanceThreshold)
            {
                legsToMove.Add(leg);
            }
        }

        if (legsToMove.Count > 0)
        {
            StartCoroutine(MoveLegPair(legsToMove));
        }
    }

    private IEnumerator MoveLegPair(List<Leg> legPair)
    {
        isLegPairMoving = true;
        List<IEnumerator> legMovements = new List<IEnumerator>();

        foreach (var leg in legPair)
        {
            legMovements.Add(MoveLeg(leg));
        }

        foreach (var movement in legMovements)
        {
            yield return StartCoroutine(movement);
        }

        yield return new WaitForSeconds(stepDelay);
        currentStepGroup = (currentStepGroup + 1) % stepGroups.Length;
        isLegPairMoving = false;
    }

    private IEnumerator MoveLeg(Leg leg)
    {
        if (leg.isMoving) yield break;
        
        leg.isMoving = true;
        Vector3 startPosition = leg.lockedPosition;
        Vector3 targetPosition = GetGroundPosition(GetLegTargetPosition(leg));
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

    private IEnumerator MoveLegToBodyTarget(Leg leg, Vector3 worldBodyTarget)
    {
        if (leg.isMoving) yield break;
        leg.isMoving = true;
        
        Vector3 startPosition = leg.IKTarget.position;
        Vector3 targetPosition = GetGroundPosition(worldBodyTarget);
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

    private Vector3 GetLegTargetPosition(Leg leg)
    {
        Vector3 direction = (leg.lockedPosition - bodyTransform.position).normalized;
        if (direction == Vector3.zero) direction = bodyTransform.right;
        return bodyTransform.position + direction * legRadius;
    }

    private Vector3 GetGroundPosition(Vector3 position, Transform fallbackTarget = null)
    {
        RaycastHit hit;
        Vector3 rayOrigin = position + Vector3.up * groundCheckDistance; // Starting position of the ray
        Vector3 rayDirection = Vector3.down; // Direction of the ray

        
        // Debug.DrawRay(rayOrigin, rayDirection * groundCheckDistance * 2, Color.red, 1f);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, groundCheckDistance * 2, groundLayer))
        {
            Debug.Log("Raycast werkt en geeft pos?"); 
            return new Vector3(hit.point.x, hit.point.y, hit.point.z); 
            // return new Vector3(position.x, hit.point.y, position.z); 

        }

        // If raycast fails, use body target or keep the position
        Debug.Log("Raycast failed. Returning fallback pos.");
        return fallbackTarget != null ? fallbackTarget.position : position;
        // return position;
    }

}
