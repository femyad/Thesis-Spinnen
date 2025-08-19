using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargetStepper : MonoBehaviour
{
    public Vector3 stepOffset = new Vector3(0f, 0, 1f); // direction and distance to step
    public float stepHeight = 0.1f;
    public float stepDuration = 0.2f;
    public float dragDuration = 0.4f;
    public float pauseTime = 0.3f;

    [Tooltip("Delay before this leg starts stepping, in seconds")]
    public float startDelay = 0f;

    private Vector3 startPos;

    public Transform legTarget; //body target 

    //navmesh..
    public float navSampleRadius = 1f;
    private bool isStepping = false;

    

    void Start()
    {
        startPos = transform.localPosition;
        // StartCoroutine(StepLoop());
    }

    IEnumerator StepLoop() //wordt niet aangeroepen nu! zie start
    {
        // Delay start if needed
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        while (true)
        {
            Vector3 targetPos = startPos + stepOffset;

            // Step forward (with arc)
            float time = 0;
            while (time < 1f)
            {
                time += Time.deltaTime / stepDuration;
                float height = Mathf.Sin(time * Mathf.PI) * stepHeight;
                transform.localPosition = Vector3.Lerp(startPos, targetPos, time) + Vector3.up * height;
                yield return null;
            }

            // Drag back 
            time = 0;
            while (time < 1f)
            {
                time += Time.deltaTime / dragDuration;
                transform.localPosition = Vector3.Lerp(targetPos, startPos, time);
                yield return null;
            }

            yield return new WaitForSeconds(pauseTime);
        }
    }

    public void StepToTarget(){
        StartCoroutine(StepToTargetCR());
        // if (!isStepping)
        //     StartCoroutine(StepToTargetCR());
    }

    private IEnumerator StepToTargetCR()
    {
        isStepping = true;

        Vector3 startPos = transform.position;
        Vector3 desiredTargetPos = legTarget.position + stepOffset;
        Vector3 targetPos = desiredTargetPos;

        // Sample nearest point on NavMesh
        if (NavMesh.SamplePosition(desiredTargetPos, out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas))
        {
            targetPos = hit.position;
        }
        else
        {
            Debug.LogWarning("No valid NavMesh point found near target.");
            isStepping = false;
            yield break;
        }

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / stepDuration;
            float heightOffset = Mathf.Sin(time * Mathf.PI) * stepHeight;
            Vector3 stepPos = Vector3.Lerp(startPos, targetPos, time) + Vector3.up * heightOffset;
            transform.position = stepPos;
            yield return null;
        }

        transform.position = targetPos;
        isStepping = false;

        // m.b.v. raycasts///////////////////////////////////////////////////////
        // float time = 0f;
        // Vector3 startPos = transform.position;
        // Vector3 targetPos = legTarget.position;

        // while (time < 1f)
        // {
        //     time += Time.deltaTime / stepDuration;
        //     float height = Mathf.Sin(time * Mathf.PI) * stepHeight;

        //     // Raycast up/down 
        //     RaycastHit hit;
        //     if (Physics.Raycast(legTarget.position + Vector3.up * 2f, Vector3.down, out hit, 100f))
        //     {
        //         targetPos = hit.point;
        //     }
        //     else if (Physics.Raycast(legTarget.position + Vector3.down * 2f, Vector3.up, out hit, 100f))
        //     {
        //         targetPos = hit.point;
        //     }

        //     Vector3 stepPos = Vector3.Lerp(startPos, targetPos, time) + Vector3.up * height;
        //     transform.position = stepPos;

        //     yield return null;
        // }

        // transform.position = targetPos; // to ground
    }

}
