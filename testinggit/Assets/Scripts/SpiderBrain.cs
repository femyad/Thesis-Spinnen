using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBrain : MonoBehaviour
{
    //Spiderbrain handles when legs need to step based on movement/rotation.
    //leg config://
    public float stepHeight = 0.1f;
    public float stepDuration = 0.2f;
    public float dragDuration = 0.4f;
    public float pauseTime = 0.3f;

    //step detection://

    public float thresholdDistance = 1f;

    public float moveSpeed = 0.1f;

    private float distTraveled = 0;

    private Vector3 previousPos;

    public List<TargetStepper> legGroup1;

    public List<TargetStepper> legGroup2;

    private List<TargetStepper> currentLegGroup;

    public List<TargetStepper> targets;

    public float rotationSpeed = 100f;

    private Quaternion previousRotation;

    //idle movement
    private float idleTimer = 0f;
    private bool hasIdled = false;
    public float idleThreshold = 3f; // Time before realigning

    public GameObject debugSphere;

    // algemene movement van alg
    void Start()
    {
         previousPos = transform.position;
         previousRotation = transform.rotation;
         currentLegGroup = legGroup1;
    }

    void Update()
{
    UpdateTargetStepperParameters(stepHeight, stepDuration, dragDuration, pauseTime);
    float verticalInput = Input.GetAxis("Vertical");
    float horizontalInput = Input.GetAxis("Horizontal");

    if (Mathf.Abs(verticalInput) > 0.01f || Mathf.Abs(horizontalInput) > 0.01f)
    {
        ProcesMovement(verticalInput, horizontalInput);
        idleTimer = 0f;
        hasIdled = false;
    }
    else
    {
        idleTimer += Time.deltaTime;
        if (!hasIdled && idleTimer > idleThreshold)
        {
            TriggerIdleRealign();
            hasIdled = true;
        }
    }
}


    public void UpdateTargetStepperParameters(float stepHeight,float stepDuration,float dragDuration, float pauseTime){
        foreach(TargetStepper targetStepper in targets){
            targetStepper.stepHeight = stepHeight;
            targetStepper.stepDuration = stepDuration;
            targetStepper.dragDuration = dragDuration;
            targetStepper.pauseTime = pauseTime;
        }
    }

    public void ProcesMovement (float verticalInput, float horizontalInput){
        debugSphere.SetActive(false);
        // float rotationSpeed = 100f; 
        transform.Rotate(Vector3.up * horizontalInput * rotationSpeed * Time.deltaTime);
        Vector3 moveDir = Vector3.forward * verticalInput * moveSpeed * Time.deltaTime;
        transform.Translate(moveDir, Space.Self);
        float deltaMove = Vector3.Distance (transform.position, previousPos);
        float deltaAngle = Quaternion.Angle(transform.rotation, previousRotation);
        float rotationToDistanceFactor = 0.12f; // hoe snel optelt voor rotate
        distTraveled += deltaMove + (deltaAngle * rotationToDistanceFactor);
        distTraveled += deltaMove;
        Debug.Log(distTraveled);
        if(Math.Abs(distTraveled)> thresholdDistance){
            // debugSphere.SetActive (true);
            Debug.Log("time to move!");
            foreach(TargetStepper legTarget in currentLegGroup){
                legTarget.StepToTarget();

            }
            if(currentLegGroup == legGroup1){
                currentLegGroup = legGroup2;
                
            }
            else{
                currentLegGroup = legGroup1;

            }
            distTraveled = 0;
        }
        previousPos = transform.position;
        previousRotation = transform.rotation;
     }

    void TriggerIdleRealign()
    {
        Debug.Log("Idle for too long — realigning legs.");
        foreach (TargetStepper stepper in targets)
        {
            stepper.StepToTarget();
        }
    }

}