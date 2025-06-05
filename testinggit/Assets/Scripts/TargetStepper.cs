using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Transform legTarget;

    

    void Start()
    {
        startPos = transform.localPosition;
        // StartCoroutine(StepLoop());
    }

    IEnumerator StepLoop()
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
    }

    private IEnumerator StepToTargetCR(){
         float time = 0;
        //  Vector3 targetPos = legTarget.position;
        //in while, target op terrain, raycast beide richtingen.
         Vector3 startPos = transform.position;
            while (time < 1f)
            {
                time += Time.deltaTime / stepDuration;
                float height = Mathf.Sin(time * Mathf.PI) * stepHeight;
                transform.position = Vector3.Lerp(startPos, legTarget.position, time) + Vector3.up * height;
                yield return null;
            }
    }
}
