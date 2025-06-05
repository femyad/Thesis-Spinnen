using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAllignment : MonoBehaviour
{
    [Header("Alignment Settings")]
    public float raycastDistance = 5f;          
    public float heightOffset = 1.0f;           
    public float positionLerpSpeed = 5.0f;      
    public float rotationLerpSpeed = 5.0f;     

    void Update()
    {
        AlignWithTerrain();
    }

    void AlignWithTerrain()
    {
       
        Vector3 rayOrigin = transform.position + Vector3.up * raycastDistance;
        Ray ray = new Ray(rayOrigin, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance * 2f))
        {
            // Debug.DrawLine(rayOrigin, hit.point, Color.red);

            // Target pos = hit point plus offset, Vector3.up was hit.normal, voor future ln 30
            Vector3 targetPosition = hit.point + Vector3.up * heightOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);

            // Calculate rotation aligning up with the terrain normal
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        }
    }
}
