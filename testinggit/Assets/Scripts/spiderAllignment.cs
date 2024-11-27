using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spiderAllignment : MonoBehaviour
{
    public float raycastDistance = 200f; // Distance to cast ray downwards
    public float heightOffset = 100f; 
    public float rotationSpeed = 5.0f; // Speed at which the spider rotates to align with the terrain

   
        void Update()
    {
        AlignWithTerrain(); 
    }

    void AlignWithTerrain()
    {
        // Cast a ray from the spider's position downwards to find the terrain height
        Ray ray = new Ray(transform.position +  Vector3.up * heightOffset, Vector3.down);
        RaycastHit hit;
        
        
        // Perform the raycast and check if it hits anything
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
             Debug.DrawLine(transform.position + Vector3.up * heightOffset * 100, hit.point, Color.red, 0.1f);//vaste positie proberen

             // Adjust the spider's position to match the height of the terrain
            Vector3 targetPosition = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            transform.position = targetPosition; // bekijken

            // Align the spider's rotation with the terrain's normal
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
